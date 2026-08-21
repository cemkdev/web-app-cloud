using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Features.Products.DTOs;
using WebAppAPI.Application.Features.Products.Queries.GetAllProducts.DTOs;
using WebAppAPI.Application.Features.Products.Queries.GetProductById.DTOs;
using WebAppAPI.Application.Features.Products.Queries.GetProductDetail.DTOs;
using WebAppAPI.Application.Features.Products.Queries.GetQrCodeFromProduct.DTOs;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Persistence.Contexts;
using Entities = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Persistence.Repositories
{
    public class ProductReadRepository(WebAppAPIDbContext context)
        : ReadRepository<Entities.Product>(context), IProductReadRepository
    {
        public async Task<GetAllProductsDto> GetPagedAsync(int page, int size, CancellationToken cancellationToken)
        {
            IQueryable<Entities.Product> query = Query(tracking: false);

            int totalProductCount = await query.CountAsync(cancellationToken);

            List<ProductListItemDto> products = await query
                .OrderByDescending(product => product.DateCreated)
                .ThenBy(product => product.Id)
                .Skip(page * size)
                .Take(size)
                .Select(product => new ProductListItemDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Stock = product.Stock,
                    Price = product.Price,
                    DateCreated = product.DateCreated,
                    DateUpdated = product.DateUpdated,
                    Title = product.Title,
                    Description = product.Description,
                    Rating = product.Rating,
                    ProductImageFiles = product.ProductImageFiles
                        .Where(image => image.CoverImage)
                        .OrderBy(image => image.Id)
                        .Take(1)
                        .Select(image => new ProductImageDto
                        {
                            Id = image.Id,
                            Path = image.Path,
                            FileName = image.FileName,
                            CoverImage = image.CoverImage
                        })
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            return new GetAllProductsDto
            {
                TotalProductCount = totalProductCount,
                Products = products
            };
        }

        public Task<ProductByIdDto?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken)
            => Query(false)
                .Where(product => product.Id == id)
                .Select(product => new ProductByIdDto
                {
                    Name = product.Name,
                    Stock = product.Stock,
                    Price = product.Price,
                    Title = product.Title,
                    Description = product.Description,
                    Rating = product.Rating
                })
                .FirstOrDefaultAsync(cancellationToken);

        public Task<ProductDetailDto?> GetProductDetailAsync(Guid id, CancellationToken cancellationToken)
            => Query(false)
                .Where(product => product.Id == id)
                .Select(product => new ProductDetailDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Stock = product.Stock,
                    Price = product.Price,
                    Title = product.Title,
                    Description = product.Description,
                    Rating = product.Rating,
                    DateCreated = product.DateCreated,
                    DateUpdated = product.DateUpdated,
                    ProductImageFiles = product.ProductImageFiles
                        .OrderByDescending(image => image.CoverImage)
                        .ThenBy(image => image.Id)
                        .Select(image => new ProductImageDto
                        {
                            Id = image.Id,
                            Path = image.Path,
                            FileName = image.FileName,
                            CoverImage = image.CoverImage
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

        public Task<Entities.Product?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken)
            => Query(tracking: true)
                .FirstOrDefaultAsync(
                    product => product.Id == id,
                    cancellationToken);

        public Task<Entities.Product?> GetByIdWithImagesAsync(Guid id, CancellationToken cancellationToken)
            => Query(tracking: false)
                .Include(product => product.ProductImageFiles)
                .FirstOrDefaultAsync(
                    product => product.Id == id,
                    cancellationToken);

        public Task<List<Entities.Product>> GetByIdsWithImagesAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
            => Query(tracking: false)
                .Include(product => product.ProductImageFiles)
                .Where(product => ids.Contains(product.Id))
                .ToListAsync(cancellationToken);

        public Task<ProductQrCodeDto?> GetQrCodeDataAsync(Guid id, CancellationToken cancellationToken)
            => Query(tracking: false)
                .Where(product => product.Id == id)
                .Select(product => new ProductQrCodeDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    Stock = product.Stock,
                    DateCreated = product.DateCreated
                })
                .FirstOrDefaultAsync(cancellationToken);
    }
}
