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
    public class BasketService : IBasketService
    {
        readonly IHttpContextAccessor _httpContextAccessor;
        readonly UserManager<AppUser> _userManager;
        readonly IOrderReadRepository _orderReadRepository;
        readonly IBasketWriteRepository _basketWriteRepository;
        readonly IBasketReadRepository _basketReadRepository;
        readonly IBasketItemReadRepository _basketItemReadRepository;
        readonly IBasketItemWriteRepository _basketItemWriteRepository;

        public BasketService(
            IHttpContextAccessor httpContextAccessor,
            UserManager<AppUser> userManager,
            IOrderReadRepository orderReadRepository,
            IBasketReadRepository basketReadRepository,
            IBasketWriteRepository basketWriteRepository,
            IBasketItemReadRepository basketItemReadRepository,
            IBasketItemWriteRepository basketItemWriteRepository)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _orderReadRepository = orderReadRepository;
            _basketReadRepository = basketReadRepository;
            _basketWriteRepository = basketWriteRepository;
            _basketItemReadRepository = basketItemReadRepository;
            _basketItemWriteRepository = basketItemWriteRepository;
        }

        public async Task AddItemToBasketAsync(VM_Create_BasketItem basketItem)
        {
            Basket? basket = await ContextUser(createIfNotExists: true);
            if (basket != null)
            {
                BasketItem _basketItem = await _basketItemReadRepository.GetSingleAsync(bi => bi.BasketId == basket.Id && bi.ProductId == Guid.Parse(basketItem.ProductId));
                if (_basketItem != null)
                    _basketItem.Quantity++;
                else
                    await _basketItemWriteRepository.AddAsync(new()
                    {
                        BasketId = basket.Id,
                        ProductId = Guid.Parse(basketItem.ProductId),
                        Quantity = basketItem.Quantity
                    });

                await _basketItemWriteRepository.SaveAsync();
            }
        }

        public async Task<List<BasketItem>> GetAllBasketItemsAsync()
        {
            Basket? basket = await ContextUser(createIfNotExists: false);
            if (basket == null)
                return new();

            Basket? result = await _basketReadRepository.Table
                                   .Include(bi => bi.BasketItems)
                                   .ThenInclude(p => p.Product)
                                   .ThenInclude(pi => pi.ProductImageFiles)
                                   .FirstOrDefaultAsync(b => b.Id == basket.Id);

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

            BasketItem? basketItem = await _basketItemReadRepository.GetSingleAsync(bi => bi.Id == basketItemGuid && bi.BasketId == basket.Id);
            if (basketItem != null)
            {
                _basketItemWriteRepository.Remove(basketItem);
                await _basketItemWriteRepository.SaveAsync();
            }
        }

        public async Task UpdateQuantityAsync(VM_Update_BasketItem basketItem)
        {
            if (!Guid.TryParse(basketItem.BasketItemId, out var basketItemGuid)) return;

            Basket? basket = await ContextUser(createIfNotExists: false);

            if (basket == null) return;

            BasketItem? currentBasketItem = await _basketItemReadRepository.Table
                                                    .Include(bi => bi.Product)
                                                    .FirstOrDefaultAsync(bi => bi.Id == basketItemGuid && bi.BasketId == basket.Id);

            if (currentBasketItem == null) return;

            if (currentBasketItem.Product == null)
                throw new Exception("Product not found.");

            if (basketItem.Quantity > currentBasketItem.Product.Stock)
                throw new Exception("Quantity exceeds available stock.");

            currentBasketItem.Quantity = basketItem.Quantity;
            await _basketItemWriteRepository.SaveAsync();
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

                var _basket = from basket in user?.Baskets
                              join order in _orderReadRepository.Table
                              on basket.Id equals order.Id into BasketOrder
                              from order in BasketOrder.DefaultIfEmpty()
                              select new
                              {
                                  Basket = basket,
                                  Order = order
                              };

                Basket? targetBasket = null;
                if (_basket.Any(o => o.Order is null))
                    targetBasket = _basket.FirstOrDefault(o => o.Order is null)?.Basket;
                else if (createIfNotExists)
                {
                    targetBasket = new();
                    user.Baskets.Add(targetBasket);
                    await _basketWriteRepository.SaveAsync();
                }

                return targetBasket;
            }
            throw new Exception("An unexpected error occurred.");
        }
        #endregion
    }
}
