using Microsoft.AspNetCore.Http;
using WebAppAPI.Application.DTOs.Product;

namespace WebAppAPI.Application.Abstractions.Services
{
    public interface IProductService
    {
        Task CreateProductAsync(CreateProductDto product);
        Task UpdateProductAsync(UpdateProductDto product);
        Task RemoveProductAsync(string id);
        Task RemoveRangeProductAsync(IEnumerable<string> productIds);
        Task UploadProductImagesAsync(string productId, IFormFileCollection? files);
        Task ChangeCoverImageAsync(string productId, string imageId);
        Task RemoveProductImageAsync(string productId, string imageId);
        Task<GetAllProductsDto> GetAllProductsAsync(int page, int size);
        Task<GetByIdProductDto> GetProductByIdAsync(string id);
        Task<List<GetProductImagesDto>> GetProductImagesAsync(string productId);
        Task<byte[]> QrCodeFromProductAsync(string productId);
    }
}
