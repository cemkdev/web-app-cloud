namespace WebAppAPI.Application.DTOs.Product
{
    public class CreateProductDto
    {
        public string Name { get; set; }
        public int Stock { get; set; }
        public float Price { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
