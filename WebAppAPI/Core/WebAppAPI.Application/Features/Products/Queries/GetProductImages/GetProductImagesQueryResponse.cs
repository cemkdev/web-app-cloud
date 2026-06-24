namespace WebAppAPI.Application.Features.Products.Queries.GetProductImages
{
    public class GetProductImagesQueryResponse
    {
        public Guid Id { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public bool CoverImage { get; set; }
    }
}
