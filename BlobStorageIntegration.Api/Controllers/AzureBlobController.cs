using System.ComponentModel.DataAnnotations;
using BlobStorageIntegration.Api.Models;
using BlobStorageIntegration.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlobStorageIntegration.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AzureBlobController : ControllerBase
{
    private readonly AzureBlobService _azureBlobService;

    public AzureBlobController(AzureBlobService azureBlobService)
    {
        _azureBlobService = azureBlobService;
    }
    
    [HttpGet("download")]
    public async Task<IActionResult> DownloadFileAsync([Required]string uri , CancellationToken cancellationToken = default)
    {
        var file = await _azureBlobService.DownloadFileAsync(uri , cancellationToken);

        // Explicitly setting Content-Disposition to force download for other file types.
        // Content-Disposition means that the file should be downloaded and not displayed in the browser.
        // In this api, we are using it to force the user download the file no matter what the file type is.
        
        var contentDisposition = new System.Net.Mime.ContentDisposition
        {
            FileName = file.FileName,
            Inline = false  // false = prompt the user for download
        };
        Response.Headers.ContentDisposition = contentDisposition.ToString();
        
        return File(file.FileStream, file.ContentType);
    }
    
    [HttpGet("read")]
    public async Task<IActionResult> ReadFileAsync(string uri, CancellationToken cancellationToken = default)
    {
        var model = await _azureBlobService.DownloadFileAsync(uri, cancellationToken);

        if (model.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
        {
            //This will simply open the PDF in the current browser window/tab.
            //To be able to open it in a new window/tab, you can use the target="_blank" attribute in the anchor tag.
            //Which should be done by frontend developer :) .
            
            return File(model.FileStream, model.ContentType);
        }
    
        if (model.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            //This will simply open the image in the current browser window/tab.
            //Same as pdf file, to be able to open it in a new window/tab, you can use the target="_blank" attribute in the anchor tag.
            //Which should be done by frontend developer :) .
            
            return File(model.FileStream, model.ContentType);
        }

        // Explicitly setting Content-Disposition to force download for other file types.
        // Content-Disposition means that the file should be downloaded and not displayed in the browser.
        // In this scenario, we are using it for all file types except images and pdf which can be shown in the browser.
        
        var contentDisposition = new System.Net.Mime.ContentDisposition
        {
            FileName = model.FileName,
            Inline = false  // false = prompt the user for download
        };
        Response.Headers.ContentDisposition = contentDisposition.ToString();
    
        return File(model.FileStream, model.ContentType);
    }
    
    [HttpPost]
     public async Task<IActionResult> UploadFileAsync([FromForm]FileCreateModel model , CancellationToken cancellationToken = default)
     {
         var response= await _azureBlobService.UploadFileAsync(model,cancellationToken);
         
         // Response will return the blob storage uri of the uploaded file.
         // In real cases, uploading file will be done in the background and saved inside a database and user will not be able to reach the blob storage uri.
         // Even if user gets the api, in many cases blob storage uri will be useless for the user since it will be private.
         // In this api, we are returning the blob storage uri for demonstration purposes.
         
         return Ok(response);
     }
}