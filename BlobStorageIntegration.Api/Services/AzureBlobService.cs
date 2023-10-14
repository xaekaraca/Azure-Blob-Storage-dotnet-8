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
        
        // You can set the metadata for the file here
        // This will also be stored in the blob storage
        // You can use it to store information about the file such as file size, file type, etc.
        
        await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders
        {
            ContentType = model.File.ContentType
        }, cancellationToken: cancellationToken);
        
        return blobClient.Uri.AbsoluteUri;
    }
    
    public async Task<FileViewModel> DownloadFileAsync(string uri , CancellationToken cancellationToken)
    {
        // BlobUriBuilder is a built-in class from Azure.Storage.Blobs
        // It is used to parse the blob uri into its components
        var uriBuilder = new BlobUriBuilder(new Uri(uri));
        
        var blobClient = _blobServiceClient.GetBlobContainerClient(uriBuilder.BlobContainerName).GetBlobClient(uriBuilder.BlobName);

        // DownloadAsync method returns a BlobDownloadInfo object
        // BlobDownloadInfo object contains the file stream, content type and other information about the file
        // In this case, we are only interested in the file stream and content type
        
        var response = await blobClient.DownloadAsync(cancellationToken);
        return new FileViewModel(response.Value.Content, response.Value.ContentType, uriBuilder.BlobName);
    }
    
    private static async Task<MemoryStream> CopyFileToMemoryStreamAsync(IFormFile file)
    {
        //MemoryStream is basically a stream stored in the memory instead of a physical file.
        //It is used to store data in memory and then retrieve it later.
        //It is mostly used with libraries that accept streams as input parameters.
        //In this case, we are using it to copy the file to a memory stream and then upload it to Azure Blob Storage.
        
        var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }
}