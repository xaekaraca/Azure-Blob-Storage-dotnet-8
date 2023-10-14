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
    public async Task<IActionResult> DownloadFileAsync(string uri , CancellationToken cancellationToken = default)
    {
        var file = await _azureBlobService.DownloadFileAsync(uri , cancellationToken);

        return File(file.FileStream, file.ContentType, file.FileName);
    }
    
    [HttpGet("read")]
    public async Task<IActionResult> ReadFileAsync(string uri , CancellationToken cancellationToken = default)
    {
        var model = await _azureBlobService.DownloadFileAsync(uri , cancellationToken);
        
        return model.ContentType.Contains("image") 
            ?
            File(model.FileStream, model.ContentType) 
            : 
            File(model.FileStream, model.ContentType, model.FileName);
    }
    
    [HttpPost]
     public async Task<IActionResult> UploadFileAsync([FromForm]FileCreateModel model , CancellationToken cancellationToken = default)
     {
         var response= await _azureBlobService.UploadFileAsync(model,cancellationToken);
         
         return Ok(response);
     }
}