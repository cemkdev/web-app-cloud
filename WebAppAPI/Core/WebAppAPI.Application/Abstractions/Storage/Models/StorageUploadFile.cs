namespace WebAppAPI.Application.Abstractions.Storage.Models
{
    public sealed class StorageUploadFile
    {
        public required string FileName { get; init; }
        public required Stream Content { get; init; }
        public required long Length { get; init; }
        public required string ContentType { get; init; }
    }
}
