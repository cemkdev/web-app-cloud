using MediatR;
using WebAppAPI.Application.Abstractions.Hubs;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Commands.Order.CreateOrder
{
    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommandRequest, CreateOrderCommandResponse>
    {
        readonly IOrderService _orderService;
        readonly IOrderHubService _orderHubService;

        public CreateOrderCommandHandler(IOrderService orderService, IOrderHubService orderHubService)
        {
            _orderService = orderService;
            _orderHubService = orderHubService;
        }

        public async Task<CreateOrderCommandResponse> Handle(CreateOrderCommandRequest request, CancellationToken cancellationToken)
        {
            await _orderService.CreateOrderFromActiveBasketAsync(new()
            {
                Description = request.Description,
                Address = request.Address
            });

            await _orderHubService.OrderAddedMessageAsync("You have a new order!");

            return new();
        }
    }
}
