namespace WebAppAPI.Application.DTOs.Product
{
    public class GetProductImagesDto
    {
        public Guid Id { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public bool CoverImage { get; set; }
    }
}
