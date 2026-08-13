using Microsoft.Extensions.Options;
using WebAppAPI.Application.Abstractions.CurrentUser;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Features.Baskets.Commands.AddItemToBasket;
using WebAppAPI.Application.Features.Baskets.Commands.UpdateQuantity;
using WebAppAPI.Application.Features.Baskets.Queries.GetAllBasketItems;
using WebAppAPI.Application.Options.Storage;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Domain.Entities;

namespace WebAppAPI.Persistence.Services
{
    public sealed class BasketService(
        ICurrentUserContext currentUserContext,
        IProductReadRepository productReadRepository,
        IBasketReadRepository basketReadRepository,
        IWriteRepository<Basket> basketWriteRepository,
        IBasketItemReadRepository basketItemReadRepository,
        IWriteRepository<BasketItem> basketItemWriteRepository,
        IOptions<BaseStorageOptions> baseStorageOptions,
        IUnitOfWork unitOfWork) : IBasketService
    {
        private readonly ICurrentUserContext _currentUserContext = currentUserContext;
        private readonly IProductReadRepository _productReadRepository = productReadRepository;
        private readonly IBasketReadRepository _basketReadRepository = basketReadRepository;
        private readonly IWriteRepository<Basket> _basketWriteRepository = basketWriteRepository;
        private readonly IBasketItemReadRepository _basketItemReadRepository = basketItemReadRepository;
        private readonly IWriteRepository<BasketItem> _basketItemWriteRepository = basketItemWriteRepository;
        private readonly BaseStorageOptions _baseStorageOptions = baseStorageOptions.Value;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task AddItemToBasketAsync(AddBasketItemDto basketItem, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(basketItem);

            if (string.IsNullOrWhiteSpace(basketItem.ProductId))
                throw new ArgumentException("Product id is required.", nameof(basketItem));

            if (!Guid.TryParse(basketItem.ProductId, out Guid productId))
                throw new ArgumentException("Product id is not valid.", nameof(basketItem));

            if (basketItem.Quantity < 1)
                throw new ArgumentOutOfRangeException(nameof(basketItem), "Quantity must be greater than zero.");

            Product? product = await _productReadRepository.GetByIdAsync(productId, cancellationToken);

            if (product is null)
                throw new KeyNotFoundException("Product not found.");

            Basket basket = await GetOrCreateActiveBasketAsync(cancellationToken);

            BasketItem? existingBasketItem = await _basketItemReadRepository.GetByBasketIdAndProductIdAsync(
                    basket.Id,
                    productId,
                    cancellationToken);

            int newQuantity = (existingBasketItem?.Quantity ?? 0) + basketItem.Quantity;

            if (newQuantity > product.Stock)
                throw new InvalidOperationException("Quantity exceeds available stock.");

            if (existingBasketItem is not null)
                existingBasketItem.Quantity = newQuantity;
            else
                await _basketItemWriteRepository.AddAsync(new BasketItem
                {
                    BasketId = basket.Id,
                    ProductId = productId,
                    Quantity = basketItem.Quantity
                });

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<BasketItemListDto>> GetAllBasketItemsAsync(CancellationToken cancellationToken)
        {
            Basket? basket = await GetActiveBasketAsync(cancellationToken);

            if (basket is null)
                return [];

            List<BasketItemListDto> items = await _basketReadRepository.GetItemsByBasketIdAsync(basket.Id, cancellationToken);

            string baseStorageUrl = _baseStorageOptions.Url.TrimEnd('/');

            return items
                .Select(item => new BasketItemListDto
                {
                    BasketItemId = item.BasketItemId,
                    ProductId = item.ProductId,
                    Name = item.Name,
                    Description = item.Description,
                    Stock = item.Stock,
                    Price = item.Price,
                    Quantity = item.Quantity,
                    ProductImageFile = item.ProductImageFile is null
                        ? null
                        : new BasketItemImageDto
                        {
                            ProductImageFileId = item.ProductImageFile.ProductImageFileId,
                            FileName = item.ProductImageFile.FileName,
                            Path = $"{baseStorageUrl}/{item.ProductImageFile.Path.TrimStart('/')}"
                        }
                })
                .ToList();
        }

        public async Task UpdateQuantityAsync(BasketItemQuantityUpdateDto basketItem, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(basketItem);

            if (string.IsNullOrWhiteSpace(basketItem.BasketItemId))
                throw new ArgumentException("Basket item id is required.", nameof(basketItem));

            if (!Guid.TryParse(basketItem.BasketItemId, out Guid basketItemId))
                throw new ArgumentException("Basket item id is not valid.", nameof(basketItem));

            if (basketItem.Quantity < 1)
                throw new ArgumentOutOfRangeException(nameof(basketItem), "Quantity must be greater than zero.");

            Basket? basket = await GetActiveBasketAsync(cancellationToken);

            if (basket is null)
                throw new KeyNotFoundException("Active basket not found.");

            BasketItem? currentBasketItem = await _basketItemReadRepository.GetForUpdateAsync(basketItemId, basket.Id, cancellationToken);

            if (currentBasketItem is null)
                throw new KeyNotFoundException("Basket item not found.");

            if (basketItem.Quantity > currentBasketItem.Product.Stock)
                throw new InvalidOperationException("Quantity exceeds available stock.");

            currentBasketItem.Quantity = basketItem.Quantity;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task RemoveBasketItemAsync(string basketItemId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(basketItemId))
                throw new ArgumentException("Basket item id is required.", nameof(basketItemId));

            if (!Guid.TryParse(basketItemId, out Guid basketItemGuid))
                throw new ArgumentException("Basket item id is not valid.", nameof(basketItemId));

            Basket? basket = await GetActiveBasketAsync(cancellationToken);

            if (basket is null)
                throw new KeyNotFoundException("Active basket not found.");

            BasketItem? basketItem = await _basketItemReadRepository.GetForDeleteAsync(
                    basketItemGuid,
                    basket.Id,
                    cancellationToken);

            if (basketItem is null)
                throw new KeyNotFoundException("Basket item not found.");

            _basketItemWriteRepository.Remove(basketItem);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        public async Task<Guid?> GetActiveBasketIdAsync(CancellationToken cancellationToken)
        {
            Basket? basket = await GetActiveBasketAsync(cancellationToken);

            return basket?.Id;
        }

        #region Helpers
        private Task<Basket?> GetActiveBasketAsync(CancellationToken cancellationToken)
            => _basketReadRepository.GetActiveBasketByUserIdAsync(
                    _currentUserContext.UserId,
                    cancellationToken);

        private async Task<Basket> GetOrCreateActiveBasketAsync(CancellationToken cancellationToken)
        {
            Basket? basket = await GetActiveBasketAsync(cancellationToken);

            if (basket is not null)
                return basket;

            basket = new Basket
            {
                UserId = _currentUserContext.UserId
            };

            await _basketWriteRepository.AddAsync(basket);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return basket;
        }
        #endregion
    }
}
