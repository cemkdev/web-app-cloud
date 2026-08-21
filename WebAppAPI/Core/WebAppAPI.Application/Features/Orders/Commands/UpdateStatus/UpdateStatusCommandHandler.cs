using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Orders.Commands.UpdateStatus
{
    public sealed class UpdateStatusCommandHandler(IOrderService orderService) : IRequestHandler<UpdateStatusCommandRequest>
    {
        public Task Handle(UpdateStatusCommandRequest request, CancellationToken cancellationToken)
            => orderService.UpdateOrderStatusAsync(
                request.OrderId,
                request.NewStatus,
                cancellationToken);
    }
}
