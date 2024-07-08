using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = SixLabors.ImageSharp.Color;
using Font = SixLabors.Fonts.Font;
using FontFamily = SixLabors.Fonts.FontFamily;
using FontStyle = SixLabors.Fonts.FontStyle;
using PointF = SixLabors.ImageSharp.PointF;

namespace BitcraftWebMap.@class
{
    public class HexTileOutput
    {
        internal static int width = 4097;
        public Image<Rgba32> image = new Image<Rgba32>(width, 4097);

        [Flags]
        public enum Biome
        {
            Dev,
            CalmForest, // 1
            PineWoods, // 2
            SnowyPeaks, // 3
            BreezyPlains, //4
            AutumnForest, //5
            Tundra,//6
            Desert,//7
            Swamp,//8
            Canyon,//9
            Ocean//10
        }

        public string MapsDir = Path.GetFullPath("./chunks/");

        public Dictionary<uint, Color> BiomeMap = new Dictionary<uint, Color>() {
            { 1, new Color(new Rgba32(180, 203, 147))  },
            { 2, new Color(new Rgba32(124, 133, 114))  },
            { 3, Color.White },
            { 4, new Color(new Rgba32(218, 236, 189)) },
            { 5, new Color(new Rgba32(188, 151, 96)) },
            { 6, new Color(new Rgba32(255, 183, 255)) },
            { 7, new Color(new Rgba32(255,255,255)) },
            { 8, Color.DarkSeaGreen },
            { 9, Color.SlateGrey },
            { 10, new Color(new Rgba32(2,164,245)) },
        };

        public static Dictionary<uint, Color> AltBiomeMap = new Dictionary<uint, Color>() {
             { 1, new Color(new Rgba32(4, 198, 6))  },
             { 2, new Color(new Rgba32(2, 107, 31))  },
             { 3, new Color(new Rgba32(225,225,225)) },
             { 4, new Color(new Rgba32(230, 177, 157)) },
             { 5, new Color(new Rgba32(203, 88, 17)) },
             { 6, new Color(new Rgba32(168, 216, 244)) },
             { 7, new Color(new Rgba32(255, 240, 101)) },
             { 8, new Color(new Rgba32(96, 48, 17)) },
             { 9, new Color(new Rgba32(82, 91, 82)) },
             { 10, new Color(new Rgba32(5, 67, 150)) },
         };

        public Color GetColorForBiome(uint biomeId, bool? AlternativeColours = false)
        {
            var remap = BitConverter.GetBytes(biomeId)[0];

            if (AlternativeColours.HasValue && AlternativeColours.Value == true)
            {
                if (!AltBiomeMap.ContainsKey(remap))
                {
                    var Random = new Random();
                    AltBiomeMap[remap] = new Color(new Rgba32((float)Random.NextDouble(), (float)Random.NextDouble(), (float)Random.NextDouble(), 1f));
                }
                return AltBiomeMap[(uint)remap];
            }

            if (!BiomeMap.ContainsKey(remap))
            {
                var Random = new Random();
                BiomeMap[remap] = new Color(new Rgba32((float)Random.NextDouble(), (float)Random.NextDouble(), (float)Random.NextDouble(), 1f));
            }
            return BiomeMap[remap];
        }

        public void CreateImage(bool? AlternativeColours = false)
        {
            image = new Image<Rgba32>(width, 4097);
            var MapChunks = new List<HexTileCacheData>();
            if (!Directory.Exists(MapsDir))
            {
                Directory.CreateDirectory(MapsDir);
            }
            var MapFiles = Directory.GetFiles(MapsDir, "alpha2_terrain_chunk_1_*");
            int MaxHeightSeen = 0;

            foreach (var file in MapFiles)
            {
                var MapChunk = HexTileReader.ReadFromFile(file);
                //MapChunk.Display();
                MapChunks.Add(MapChunk);
                var MaxHeightChunk = MapChunk.Elevations.Max();
                if (MaxHeightChunk > MaxHeightSeen)
                    MaxHeightSeen = MaxHeightChunk;
                //.ReadLine();
            }

            Console.WriteLine("Done Reading. Process Map: Max Height Was: " + MaxHeightSeen);

            // Fill the image with a blue color (optional)
            image.Mutate(ctx => ctx.BackgroundColor(Color.Black));

            foreach (var file in MapChunks)
            {
                int index = 0;

                for (int y = 0; y < 32; y++)
                {
                    for (int x = 0; x < 32; x++)
                    {
                        var image_x = file.ChunkX * 32 + x;
                        var image_y = file.ChunkY * 32 + y;
                        var Col = GetColorForBiome(file.Biomes[index], AlternativeColours);

                        if (file.WaterLevels[index]
                            < file.Elevations[index])
                        {
                            Col = new Color(new Rgba32(2, 164, 245));
                        }

                        image[image_x, image_y] = Col;
                        index++;
                    }
                }
            }

            FontCollection collection = new();
            FontFamily family = collection.Add(string.Concat(Environment.CurrentDirectory, "/font.ttf"));
            Font font = family.CreateFont(24, FontStyle.Italic);

            image.Mutate(ctx => { ctx.Flip(FlipMode.Vertical); });

            for (int i = 1; i <= 10; i++)
            {
                var Name = Enum.GetName(typeof(Biome), i);
                var Color = GetColorForBiome((uint)i);
                image.Mutate(x => x.DrawText(Name, font, Color, new SixLabors.ImageSharp.PointF(20, i * 25)));
            }

            image.Mutate(x => x.DrawText("Recorded At: " + DateTime.Now.ToShortDateString(), font, Color.White, new PointF(width / 2, 20)));

        }

        public void SaveImage(bool useAltColours = false)
        {
            // Save the image as a JPEG file
            string dir = (Directory.Exists("./wwwroot") ? "./wwwroot/" : "./") + "img/"; 
            string filePath = "output" + (useAltColours ? "_alt" : "") + ".png";

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            image.Save(dir+filePath, new PngEncoder { });
            Console.WriteLine($"Image saved as: {dir+filePath}");

        }

        public void CreateAndSave(bool useAltColours = false)
        {
            CreateImage(useAltColours);
            SaveImage(useAltColours);
        }
    }
}
