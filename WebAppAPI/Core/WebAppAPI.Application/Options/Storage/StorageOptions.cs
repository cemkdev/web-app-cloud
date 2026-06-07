namespace WebAppAPI.Application.Options.Storage
{
    public sealed class StorageOptions
    {
        public const string SectionName = "Storage";

        public StorageProvider Provider { get; set; } = StorageProvider.Local;

        public AzureStorageOptions Azure { get; set; } = new();
    }
}
