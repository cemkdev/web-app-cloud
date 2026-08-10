using WebAppAPI.Application.Features.Products.Queries.GetAllProducts.DTOs;
using WebAppAPI.Application.Features.Products.Queries.GetProductById.DTOs;
using WebAppAPI.Application.Features.Products.Queries.GetProductDetail.DTOs;
using WebAppAPI.Application.Features.Products.Queries.GetQrCodeFromProduct.DTOs;
using Entities = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Application.Repositories
{
    public interface IProductReadRepository : IReadRepository<Entities.Product>
    {
        Task<GetAllProductsDto> GetPagedAsync(int page, int size, CancellationToken cancellationToken);
        Task<ProductByIdDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<ProductDetailDto?> GetProductDetailAsync(Guid id, CancellationToken cancellationToken);
        Task<Entities.Product?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);
        Task<Entities.Product?> GetByIdWithImagesAsync(Guid id, CancellationToken cancellationToken);
        Task<List<Entities.Product>> GetByIdsWithImagesAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken);
        Task<ProductQrCodeDto?> GetQrCodeDataAsync(Guid id, CancellationToken cancellationToken);
    }
}
