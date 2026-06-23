using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Application.ViewModels.Baskets;
using WebAppAPI.Domain.Entities;
using WebAppAPI.Domain.Entities.Identity;

namespace WebAppAPI.Persistence.Services
{
    public class BasketService(
        IHttpContextAccessor httpContextAccessor,
        UserManager<AppUser> userManager,
        IOrderReadRepository orderReadRepository,
        IBasketReadRepository basketReadRepository,
        IBasketItemReadRepository basketItemReadRepository,
        IWriteRepository<BasketItem> basketItemWriteRepository,
        IUnitOfWork unitOfWork) : IBasketService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly UserManager<AppUser> _userManager = userManager;
        private readonly IOrderReadRepository _orderReadRepository = orderReadRepository;
        private readonly IBasketReadRepository _basketReadRepository = basketReadRepository;
        private readonly IBasketItemReadRepository _basketItemReadRepository = basketItemReadRepository;
        private readonly IWriteRepository<BasketItem> _basketItemWriteRepository = basketItemWriteRepository;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task AddItemToBasketAsync(VM_Create_BasketItem basketItem)
        {
            Basket? basket = await ContextUser(createIfNotExists: true);
            if (basket != null)
            {
                if (string.IsNullOrWhiteSpace(basketItem.ProductId) || !Guid.TryParse(basketItem.ProductId, out Guid productGuid))
                    throw new Exception("Product id is not valid.");

                BasketItem? existingBasketItem = await _basketItemReadRepository.GetByBasketAndProductAsync(basket.Id, productGuid, tracking: true);
                if (existingBasketItem != null)
                    existingBasketItem.Quantity++;
                else
                    await _basketItemWriteRepository.AddAsync(new()
                    {
                        BasketId = basket.Id,
                        ProductId = productGuid,
                        Quantity = basketItem.Quantity
                    });

                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<List<BasketItem>> GetAllBasketItemsAsync()
        {
            Basket? basket = await ContextUser(createIfNotExists: false);
            if (basket == null)
                return new();

            Basket? result = await _basketReadRepository.GetWithItemsAndProductImagesAsync(basket.Id);

            return result?.BasketItems.OrderBy(bi => bi.DateCreated).ToList() ?? new();
        }

        public async Task<Basket?> GetUserActiveBasketAsync(bool createIfNotExists = false)
        {
            return await ContextUser(createIfNotExists);
        }

        public async Task RemoveBasketItemAsync(string basketItemId)
        {
            if (!Guid.TryParse(basketItemId, out var basketItemGuid))
                return;

            Basket? basket = await ContextUser(createIfNotExists: false);
            if (basket == null)
                return;

            BasketItem? basketItem = await _basketItemReadRepository.GetByIdAndBasketAsync(basketItemGuid, basket.Id, tracking: true);
            if (basketItem != null)
            {
                _basketItemWriteRepository.Remove(basketItem);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task UpdateQuantityAsync(VM_Update_BasketItem basketItem)
        {
            if (!Guid.TryParse(basketItem.BasketItemId, out var basketItemGuid)) return;

            Basket? basket = await ContextUser(createIfNotExists: false);

            if (basket == null) return;

            BasketItem? currentBasketItem = await _basketItemReadRepository.GetByIdAndBasketAsync(basketItemGuid, basket.Id, tracking: true);

            if (currentBasketItem == null) return;

            if (currentBasketItem.Product == null)
                throw new Exception("Product not found.");

            if (basketItem.Quantity > currentBasketItem.Product.Stock)
                throw new Exception("Quantity exceeds available stock.");

            currentBasketItem.Quantity = basketItem.Quantity;
            await _unitOfWork.SaveChangesAsync();
        }

        #region Helpers
        private async Task<Basket?> ContextUser(bool createIfNotExists)
        {
            var username = _httpContextAccessor?.HttpContext?.User?.Identity?.Name;

            if (!string.IsNullOrEmpty(username))
            {
                AppUser? user = await _userManager.Users
                                        .Include(b => b.Baskets)
                                        .FirstOrDefaultAsync(u => u.UserName == username);

                Basket? targetBasket = null;
                if (user?.Baskets != null)
                {
                    foreach (Basket basket in user.Baskets)
                    {
                        if (!await _orderReadRepository.HasOrderForBasketAsync(basket.Id))
                        {
                            targetBasket = basket;
                            break;
                        }
                    }
                }

                if (targetBasket == null && createIfNotExists)
                {
                    targetBasket = new();
                    user!.Baskets.Add(targetBasket);
                    await _unitOfWork.SaveChangesAsync();
                }

                return targetBasket;
            }
            throw new Exception("An unexpected error occurred.");
        }
        #endregion
    }
}
