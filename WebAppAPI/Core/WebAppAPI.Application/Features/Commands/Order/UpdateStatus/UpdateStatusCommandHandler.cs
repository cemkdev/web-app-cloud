using MediatR;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Domain.Enums;

namespace WebAppAPI.Application.Features.Commands.Order.UpdateStatus
{
    public class UpdateStatusCommandHandler : IRequestHandler<UpdateStatusCommandRequest, UpdateStatusCommandResponse>
    {
        readonly IOrderService _orderService;

        public UpdateStatusCommandHandler(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<UpdateStatusCommandResponse> Handle(UpdateStatusCommandRequest request, CancellationToken cancellationToken)
        {
            await _orderService.UpdateOrderStatusAsync(request.OrderId, (OrderStatusEnum)request.NewStatus);
            return new();
        }
    }
}
