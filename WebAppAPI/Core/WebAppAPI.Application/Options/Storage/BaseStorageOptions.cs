namespace WebAppAPI.Application.Options.Storage
{
    /// <summary>
    /// Represents the public base URL used when generating file and product image URLs.
    /// </summary>
    public sealed class BaseStorageOptions
    {
        public const string SectionName = "BaseStorage";

        public string Url { get; set; } = string.Empty;
    }
}
