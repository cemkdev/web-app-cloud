namespace WebAppAPI.Application.Features.Products.Queries.GetAllProducts
{
    public class GetAllProductsQueryResponse
    {
        public int TotalProductCount { get; set; }
        public object Products { get; set; }
    }
}
