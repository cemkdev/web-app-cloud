namespace WebAppAPI.Application.Features.Products.Commands.UploadProductImage.Policies
{
    public static class ProductImageUploadPolicy
    {
        public const long MaxFileSizeBytes = 5 * 1024 * 1024;

        public static IReadOnlySet<string> AllowedExtensions { get; } =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".png",
                ".jpg",
                ".jpeg"
            };

        public static IReadOnlySet<string> AllowedContentTypes { get; } =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "image/png",
                "image/jpeg"
            };
    }
}
