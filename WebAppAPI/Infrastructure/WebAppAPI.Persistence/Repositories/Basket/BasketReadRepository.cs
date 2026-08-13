using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Features.Baskets.Queries.GetAllBasketItems;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Persistence.Contexts;
using Entities = WebAppAPI.Domain.Entities;

namespace WebAppAPI.Persistence.Repositories
{
    public class BasketReadRepository(WebAppAPIDbContext context)
        : ReadRepository<Entities.Basket>(context), IBasketReadRepository
    {
        public Task<Entities.Basket?> GetActiveBasketByUserIdAsync(string userId, CancellationToken cancellationToken)
            => Query(tracking: false)
                .FirstOrDefaultAsync(
                    basket => basket.UserId == userId && basket.Order == null,
                    cancellationToken);

        public Task<List<BasketItemListDto>> GetItemsByBasketIdAsync(Guid basketId, CancellationToken cancellationToken)
            => Query(false)
                .Where(basket => basket.Id == basketId)
                .SelectMany(basket => basket.BasketItems)
                .OrderBy(basketItem => basketItem.DateCreated)
                .Select(basketItem => new BasketItemListDto
                {
                    BasketItemId = basketItem.Id,
                    ProductId = basketItem.ProductId,
                    Name = basketItem.Product.Name,
                    Description = basketItem.Product.Description,
                    Stock = basketItem.Product.Stock,
                    Price = basketItem.Product.Price,
                    Quantity = basketItem.Quantity,
                    ProductImageFile = basketItem.Product.ProductImageFiles
                        .Where(image => image.CoverImage)
                        .OrderBy(image => image.Id)
                        .Select(image => new BasketItemImageDto
                        {
                            ProductImageFileId = image.Id,
                            FileName = image.FileName,
                            Path = image.Path
                        })
                        .FirstOrDefault()
                })
                .ToListAsync(cancellationToken);
    }
}
