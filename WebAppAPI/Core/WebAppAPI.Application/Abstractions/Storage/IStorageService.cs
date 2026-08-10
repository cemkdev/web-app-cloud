using WebAppAPI.Application.Abstractions.Storage.Models;
using WebAppAPI.Application.Options.Storage;

namespace WebAppAPI.Application.Abstractions.Storage
{
    public interface IStorageService
    {
        StorageProvider Provider { get; }

        Task<IReadOnlyList<StoredFile>> UploadAsync(
            string pathOrContainerName,
            IReadOnlyCollection<StorageUploadFile> files,
            CancellationToken cancellationToken);

        Task DeleteAsync(
            string pathOrContainerName,
            string fileName,
            CancellationToken cancellationToken);

        Task<IReadOnlyList<string>> GetFilesAsync(
            string pathOrContainerName,
            CancellationToken cancellationToken);

        Task<bool> ExistsAsync(
            string pathOrContainerName,
            string fileName,
            CancellationToken cancellationToken);
    }
}
