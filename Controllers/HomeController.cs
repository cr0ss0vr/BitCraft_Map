﻿using BitcraftWebMap.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.RegularExpressions;
using BitcraftWebMap.@class;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp;
using Microsoft.VisualBasic;

namespace BitcraftWebMap.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private HexTileOutput hexTileOutput = new HexTileOutput();

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index(bool? alternateColours)
        {
            // Retrieve the alternate colours setting from the form
            var alternateColoursValue = alternateColours ?? false;
            // Store the alternate colours setting in ViewBag to pass it back to the view
            ViewBag.AlternateColours = alternateColoursValue;

            return View();
        }

        public IActionResult RenderImage(bool? alternateColours)
        {
            hexTileOutput.CreateImage(alternateColours);
            using (var stream = new MemoryStream())
            {
                // Convert the Image<Rgba32> object to a byte array
                hexTileOutput.image.SaveAsPng(stream, new PngEncoder());

                // Return the byte array as a response with appropriate content type
                return File(stream.ToArray(), "image/png");
            }
        }

        public IActionResult Upload()
        {
            return View();
        }
        [HttpPost]
        [Route("/Upload/UploadBatch")]
        public async Task<IActionResult> UploadBatch(List<IFormFile> fileInput)
        {
            string uploadFileName = "";
            try
            {
                foreach (var file in fileInput)
                {
                    var fileName = file.FileName;
                    uploadFileName = fileName;
                    if (file.Length != 15380) // 16KB in bytes
                    {
                        // Invalid file size
                        ViewBag.UploadErrorMessage = "Invalid file size: {fileName}";
                        throw new Exception("Invalid file size.");
                    }

                    var regex = new Regex("^alpha2_terrain_chunk_[0-9]{1,4}_[0-9]{1,4}_[0-9]{1,4}$");
                    if (!regex.IsMatch(fileName))
                    {
                        // Invalid file name format
                        ViewBag.UploadErrorMessage = $"Invalid file name format: {fileName}";
                        throw new Exception("Invalid file name format.");
                    }
                    else
                    {
                        //do not write chunks that are from a different 'dimension'
                        if (!fileName.StartsWith("alpha2_terrain_chunk_1_"))
                            continue;
                    }

                    var filePath = "./chunks/" + fileName;
                    if (!System.IO.File.Exists(filePath))
                    {
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                }

                ViewBag.UploadSuccessMessage = $"Files uploaded successfully.";

                return Ok("Batch uploaded successfully!");
            }
            catch (Exception ex)
            {
                ViewBag.UploadErrorMessage = $"File failed to upload: {uploadFileName}";
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }

        }

        [HttpPost]
        [Route("/Home/UpdateRender")]
        public async Task<IActionResult> UpdateRender()
        {
            hexTileOutput.CreateAndSave();
            hexTileOutput.CreateAndSave(true);
            return Ok("Map updated!");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}