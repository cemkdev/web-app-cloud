using MediatR;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Features.Orders.Notifications;

namespace WebAppAPI.Application.Features.Orders.Commands.CreateOrder
{
    public sealed class CreateOrderCommandHandler(IOrderService orderService, IMediator mediator) : IRequestHandler<CreateOrderCommandRequest>
    {
        public async Task Handle(CreateOrderCommandRequest request, CancellationToken cancellationToken)
        {
            await orderService.CreateOrderAsync(
                new OrderCreateDto
                {
                    Address = request.Address,
                    Description = request.Description
                },
                cancellationToken);

            await mediator.Publish(new OrderCreatedNotification(), cancellationToken);
        }
    }
}
