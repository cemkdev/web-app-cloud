using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Orders.Commands.RemoveRangeOrder
{
    public sealed class RemoveRangeOrderCommandHandler(IOrderService orderService) : IRequestHandler<RemoveRangeOrderCommandRequest>
    {
        public Task Handle(RemoveRangeOrderCommandRequest request, CancellationToken cancellationToken)
            => orderService.DeleteRangeOrderAsync(
                request.OrderIds,
                cancellationToken);
    }
}
