using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Orders.Commands.RemoveOrder
{
    public sealed class RemoveOrderCommandHandler(IOrderService orderService) : IRequestHandler<RemoveOrderCommandRequest>
    {
        public Task Handle(RemoveOrderCommandRequest request, CancellationToken cancellationToken)
            => orderService.DeleteOrderAsync(
                request.Id,
                cancellationToken);
    }
}
