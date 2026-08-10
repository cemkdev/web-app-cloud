using WebAppAPI.Application.Abstractions.Storage.Models;
using WebAppAPI.Application.Features.Products.Commands.CreateProduct.DTOs;
using WebAppAPI.Application.Features.Products.Commands.UpdateProduct.DTOs;
using WebAppAPI.Application.Features.Products.DTOs;
using WebAppAPI.Application.Features.Products.Queries.GetAllProducts.DTOs;
using WebAppAPI.Application.Features.Products.Queries.GetProductById.DTOs;
using WebAppAPI.Application.Features.Products.Queries.GetProductDetail.DTOs;
namespace WebAppAPI.Application.Abstractions.Services
{
    public interface IProductService
    {
        Task<GetAllProductsDto> GetAllProductsAsync(int page, int size, CancellationToken cancellationToken);
        Task<ProductByIdDto> GetProductByIdAsync(string id, CancellationToken cancellationToken);
        Task<ProductDetailDto> GetProductDetailAsync(string id, CancellationToken cancellationToken);
        Task<IReadOnlyList<ProductImageDto>> GetProductImagesAsync(string productId, CancellationToken cancellationToken);
        Task<byte[]> QrCodeFromProductAsync(string productId, CancellationToken cancellationToken);
        Task<Guid> CreateProductAsync(CreateProductDto product, CancellationToken cancellationToken);
        Task UpdateProductAsync(UpdateProductDto product, CancellationToken cancellationToken);
        Task UploadProductImagesAsync(string productId, IReadOnlyCollection<StorageUploadFile> files, CancellationToken cancellationToken);
        Task ChangeCoverImageAsync(string productId, string imageId, CancellationToken cancellationToken);
        Task RemoveProductImageAsync(string productId, string imageId, CancellationToken cancellationToken);
        Task RemoveProductAsync(string id, CancellationToken cancellationToken);
        Task RemoveRangeProductAsync(IEnumerable<string> productIds, CancellationToken cancellationToken);
    }
}
