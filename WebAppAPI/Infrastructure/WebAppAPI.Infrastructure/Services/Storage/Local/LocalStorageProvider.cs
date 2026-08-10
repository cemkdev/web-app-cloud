using Microsoft.AspNetCore.Hosting;
using WebAppAPI.Application.Abstractions.Storage;
using WebAppAPI.Application.Abstractions.Storage.Models;
using WebAppAPI.Application.Options.Storage;

namespace WebAppAPI.Infrastructure.Services.Storage.Local
{
    public sealed class LocalStorageProvider(IWebHostEnvironment webHostEnvironment, StorageFileNameGenerator fileNameGenerator) : IStorageService
    {
        public StorageProvider Provider => StorageProvider.LocalStorage;

        public async Task<IReadOnlyList<StoredFile>> UploadAsync(
            string pathOrContainerName,
            IReadOnlyCollection<StorageUploadFile> files,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(files);

            string uploadPath = GetStoragePath(pathOrContainerName);

            Directory.CreateDirectory(uploadPath);

            List<StoredFile> uploadedFiles = [];

            foreach (StorageUploadFile file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (string candidateFileName in fileNameGenerator.GenerateCandidates(file.FileName))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    string fullPath = Path.Combine(uploadPath, candidateFileName);

                    FileStream fileStream;

                    try
                    {
                        fileStream = new FileStream(
                            fullPath,
                            FileMode.CreateNew,
                            FileAccess.Write,
                            FileShare.None,
                            bufferSize: 81920,
                            useAsync: true);
                    }
                    catch (IOException) when (File.Exists(fullPath))
                    {
                        continue;
                    }

                    try
                    {
                        await using (fileStream)
                        {
                            await file.Content.CopyToAsync(fileStream, cancellationToken);
                            await fileStream.FlushAsync(cancellationToken);
                        }
                    }
                    catch
                    {
                        if (File.Exists(fullPath))
                            File.Delete(fullPath);

                        throw;
                    }

                    uploadedFiles.Add(new StoredFile
                    {
                        FileName = candidateFileName,
                        Path = Path.Combine(pathOrContainerName, candidateFileName).Replace('\\', '/')
                    });

                    break;
                }
            }

            return uploadedFiles;
        }

        public Task DeleteAsync(string pathOrContainerName, string fileName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string filePath = Path.Combine(GetStoragePath(pathOrContainerName), fileName);

            if (File.Exists(filePath))
                File.Delete(filePath);

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<string>> GetFilesAsync(string pathOrContainerName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string directoryPath = GetStoragePath(pathOrContainerName);

            if (!Directory.Exists(directoryPath))
                return Task.FromResult<IReadOnlyList<string>>([]);

            IReadOnlyList<string> files = Directory
                .EnumerateFiles(directoryPath)
                .Select(Path.GetFileName)
                .OfType<string>()
                .ToList();

            return Task.FromResult(files);
        }

        public Task<bool> ExistsAsync(string pathOrContainerName, string fileName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            bool exists = File.Exists(Path.Combine(
                    GetStoragePath(pathOrContainerName),
                    fileName));

            return Task.FromResult(exists);
        }

        private string GetStoragePath(string pathOrContainerName)
        {
            string? webRoot = webHostEnvironment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRoot))
                webRoot = Path.Combine(webHostEnvironment.ContentRootPath, "wwwroot");

            return Path.Combine(webRoot, pathOrContainerName);
        }
    }
}
