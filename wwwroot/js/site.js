// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ready(function () {
    function validateForm(files) {
        var maxFileSize = 700 * 1024 * 1024; // Convert MB to bytes
        var filesSize = 0;

        // Check if no files were selected
        if (files.length === 0) {
            alert("No files selected.");
            return; // Prevent form submission
        }

        // Check each selected file
        for (var i = 0; i < files.length; i++) {
            filesSize += files[i].size; // running total size of the files in bytes
            if (filesSize > maxFileSize) {
                alert("Total file size exceeds the maximum limit of 700MB.");
                return; // Prevent form submission
            }
        }
        uploadFiles(files); // Allow form submission
    }

    function createDiv(id) {
        var div = document.createElement('div');
        div.id = 'batch' + id; // Unique ID for the div
        return div;
    }

    function addDivAfterForm(batchId) {
        var form = document.getElementById('uploadForm');
        var newDiv = createDiv(batchId);
        form.insertAdjacentElement('afterend', newDiv);
    }

    function uploadFiles(files) {
        // Convert FileList to array
        const filesArray = [...files];

        // Define the chunk size
        const chunkSize = 1000; // Set the number of files per chunk

        // Calculate the number of batches
        const numBatches = Math.ceil(filesArray.length / chunkSize);

        addDivAfterForm(-1);

        $("#batch-1").text("This upload will be done in " + numBatches.toString() + " batches.");

        // Iterate over batches
        for (let i = 0; i < numBatches; i++) {
            // Get a subset of files for the current batch
            const start = i * chunkSize;
            const end = Math.min(start + chunkSize, filesArray.length);
            const batchFiles = filesArray.slice(start, end);
            addDivAfterForm(i);
            // Create FormData object to send files
            const formData = new FormData();
            batchFiles.forEach(file => {
                formData.append('fileInput', file);
            });

            // Send AJAX request
            $.ajax({
                url: '/Upload/UploadBatch',
                type: 'POST',
                data: formData,
                contentType: false,
                processData: false,
                success: function (response) {
                    // Update success message on the page
                    $("#batch" + i).text((i+1).toString() +": "+ response);
                },
                error: function (xhr, status, error) {
                    // Update error message on the page
                    $("#batch" + i).text((i + 1).toString() + ": " + xhr.responseText);
                }
            });
        }

        $.ajax({
            url: '/Home/UpdateRender',
            type: 'POST',
            data: '',
            contentType: false,
            processData: false,
            success: function (response) {
                // Update success message on the page
                $("batch-1").text($("#batch-1").text()+" \r\n Map Updated!");
            },
            error: function (xhr, status, error) {
                // Update error message on the page
                $("batch-1").text($("#batch-1").text() + " \r\n Map failed to update");
            }
        });
    }

    // Attach event listener to the form's submit event
    $('#uploadForm').submit(function (event) {
        event.preventDefault(); // Prevent default form submission

        // Get the selected files
        var files = document.getElementById('fileInput').files;

        // Call the uploadFiles method with the selected files
        validateForm(files);
    });

    // Attach event listener to the checkbox's change event
    $('#toggleColoursCheckbox').change(function () {
        // Get the value of the checkbox
        var isChecked = $(this).is(":checked");

        // Update the hidden input value with the checkbox state
        $('#alternateColours').val(isChecked);

        // Submit the form
        $('#toggleForm').submit();
    });
});
