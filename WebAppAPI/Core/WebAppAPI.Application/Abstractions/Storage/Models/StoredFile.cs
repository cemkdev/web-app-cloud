namespace WebAppAPI.Application.Abstractions.Storage.Models
{
    public sealed class StoredFile
    {
        public required string FileName { get; init; }
        public required string Path { get; init; }
    }
}
