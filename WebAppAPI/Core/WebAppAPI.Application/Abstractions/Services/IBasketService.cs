using WebAppAPI.Application.ViewModels.Baskets;
using WebAppAPI.Domain.Entities;

namespace WebAppAPI.Application.Abstractions.Services
{
    public interface IBasketService
    {
        public Task<List<BasketItem>> GetAllBasketItemsAsync();
        public Task AddItemToBasketAsync(VM_Create_BasketItem basketItem);
        public Task UpdateQuantityAsync(VM_Update_BasketItem basketItem);
        public Task RemoveBasketItemAsync(string basketItemId);
        public Task<Basket?> GetUserActiveBasketAsync(bool createIfNotExists = false);
    }
}
