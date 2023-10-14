using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BlobStorageIntegration.Api.Models;
using BlobStorageIntegration.Api.Settings;
using Microsoft.Extensions.Options;

namespace BlobStorageIntegration.Api.Services;

public class AzureBlobService
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobService(IOptions<BlobSettings> blobServiceClient)
    {
        var blobSettingsValue = blobServiceClient.Value;
        _blobServiceClient = new BlobServiceClient(blobSettingsValue.ConnectionString);
    }
    
    /// <summary>
    ///  Upload file to Azure Blob Storage
    /// </summary>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<string> UploadFileAsync(FileCreateModel model, CancellationToken cancellationToken = default)
    {
        using var memoryStream = await CopyFileToMemoryStreamAsync(model.File);

        // Container name can be either static or dynamic depending on your needs
        // In this case, I am using a dynamic container name
        // If you want to use a static container name, you can bring the container name from the appsettings.json file with the IOptions interface
        
        var container = _blobServiceClient.GetBlobContainerClient(model.ContainerName);
        await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        var blobClient = container.GetBlobClient(model.BlobName);
        
        await blobClient.UploadAsync(memoryStream, true, cancellationToken);
        
        await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders
        {
            ContentType = model.File.ContentType
        }, cancellationToken: cancellationToken);
        
        return blobClient.Uri.AbsoluteUri;
    }

    /// <summary>
    /// Download file from Azure Blob Storage
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<FileViewModel> DownloadFileAsync(string uri , CancellationToken cancellationToken)
    {
        var uriBuilder = new BlobUriBuilder(new Uri(uri));
        
        var blobClient = _blobServiceClient.GetBlobContainerClient(uriBuilder.BlobContainerName).GetBlobClient(uriBuilder.BlobName);

        var response = await blobClient.DownloadAsync(cancellationToken);
        return new FileViewModel(response.Value.Content, response.Value.ContentType, uriBuilder.BlobName);
    }

    /// <summary>
    /// Copy file to memory stream
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    private static async Task<MemoryStream> CopyFileToMemoryStreamAsync(IFormFile file)
    {
        var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }
}