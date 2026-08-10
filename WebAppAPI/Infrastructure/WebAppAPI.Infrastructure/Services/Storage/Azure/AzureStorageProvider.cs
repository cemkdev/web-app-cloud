using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;
using System.Net;
using WebAppAPI.Application.Abstractions.Storage;
using WebAppAPI.Application.Abstractions.Storage.Models;
using WebAppAPI.Application.Options.Storage;

namespace WebAppAPI.Infrastructure.Services.Storage.Azure
{
    public sealed class AzureStorageProvider(IOptions<StorageOptions> storageOptions, StorageFileNameGenerator fileNameGenerator) : IStorageService
    {
        private readonly BlobServiceClient _blobServiceClient = new(storageOptions.Value.Azure.ConnectionString);

        public StorageProvider Provider => StorageProvider.AzureStorage;

        public async Task<IReadOnlyList<StoredFile>> UploadAsync(
            string pathOrContainerName,
            IReadOnlyCollection<StorageUploadFile> files,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(files);

            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(pathOrContainerName);

            await containerClient.CreateIfNotExistsAsync(
                PublicAccessType.Blob,
                cancellationToken: cancellationToken);

            List<StoredFile> uploadedFiles = [];

            foreach (StorageUploadFile file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Stream uploadStream = file.Content;
                MemoryStream? bufferedStream = null;

                if (!uploadStream.CanSeek)
                {
                    bufferedStream = new MemoryStream();

                    await uploadStream.CopyToAsync(bufferedStream, cancellationToken);

                    bufferedStream.Position = 0;
                    uploadStream = bufferedStream;
                }

                long initialPosition = uploadStream.Position;

                try
                {
                    foreach (string candidateFileName in fileNameGenerator.GenerateCandidates(file.FileName))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        uploadStream.Position = initialPosition;

                        BlobClient blobClient = containerClient.GetBlobClient(candidateFileName);

                        BlobUploadOptions uploadOptions = new()
                        {
                            HttpHeaders = new BlobHttpHeaders
                            {
                                ContentType = file.ContentType
                            },
                            Conditions = new BlobRequestConditions
                            {
                                IfNoneMatch = ETag.All
                            }
                        };

                        try
                        {
                            await blobClient.UploadAsync(uploadStream, uploadOptions, cancellationToken);
                        }
                        catch (RequestFailedException exception) when (exception.Status == (int)HttpStatusCode.PreconditionFailed)
                        {
                            continue;
                        }

                        uploadedFiles.Add(new StoredFile
                        {
                            FileName = candidateFileName,
                            Path = $"{pathOrContainerName}/{candidateFileName}"
                        });

                        break;
                    }
                }
                finally
                {
                    if (bufferedStream is not null)
                        await bufferedStream.DisposeAsync();
                }
            }

            return uploadedFiles;
        }

        public async Task DeleteAsync(string pathOrContainerName, string fileName, CancellationToken cancellationToken)
        {
            BlobClient blobClient = _blobServiceClient
                .GetBlobContainerClient(pathOrContainerName)
                .GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync(
                DeleteSnapshotsOption.IncludeSnapshots,
                cancellationToken: cancellationToken);
        }

        public async Task<IReadOnlyList<string>> GetFilesAsync(string pathOrContainerName, CancellationToken cancellationToken)
        {
            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(pathOrContainerName);

            List<string> files = [];

            await foreach (BlobItem blob in containerClient.GetBlobsAsync(cancellationToken: cancellationToken))
                files.Add(blob.Name);

            return files;
        }

        public async Task<bool> ExistsAsync(string pathOrContainerName, string fileName, CancellationToken cancellationToken)
        {
            BlobClient blobClient = _blobServiceClient
                .GetBlobContainerClient(pathOrContainerName)
                .GetBlobClient(fileName);

            return (await blobClient.ExistsAsync(cancellationToken)).Value;
        }
    }
}
