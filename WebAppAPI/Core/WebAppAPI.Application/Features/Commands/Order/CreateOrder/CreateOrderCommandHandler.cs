using MediatR;
using WebAppAPI.Application.Abstractions.Hubs;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Domain.Enums;

namespace WebAppAPI.Application.Features.Commands.Order.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommandRequest, CreateOrderCommandResponse>
    {
        readonly IOrderService _orderService;
        readonly IBasketService _basketService;
        readonly IOrderHubService _orderHubService;

        public CreateOrderCommandHandler(IOrderService orderService, IBasketService basketService, IOrderHubService orderHubService)
        {
            _orderService = orderService;
            _basketService = basketService;
            _orderHubService = orderHubService;
        }

        public async Task<CreateOrderCommandResponse> Handle(CreateOrderCommandRequest request, CancellationToken cancellationToken)
        {
            var basket = await _basketService.GetUserActiveBasketAsync(createIfNotExists: false);
            if (basket == null)
                throw new Exception("Cannot create order from an empty basket.");

            var basketItems = await _basketService.GetAllBasketItemsAsync();
            if (!basketItems.Any())
                throw new Exception("Cannot create order from an empty basket.");

            var orderId = await _orderService.CreateOrderAsync(new()
            {
                BasketId = basket.Id.ToString(),
                Description = request.Description,
                Address = request.Address
            });
            await _orderService.UpdateOrderStatusAsync(orderId, OrderStatusEnum.Pending);

            await _orderHubService.OrderAddedMessageAsync("You have a new order!");

            return new();
        }
    }
}
