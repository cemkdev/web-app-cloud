using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Orders.Commands.RemoveRangeOrder
{
    public class RemoveRangeOrderCommandHandler : IRequestHandler<RemoveRangeOrderCommandRequest, RemoveRangeOrderCommandResponse>
    {
        readonly IOrderService _orderService;

        public RemoveRangeOrderCommandHandler(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<RemoveRangeOrderCommandResponse> Handle(RemoveRangeOrderCommandRequest request, CancellationToken cancellationToken)
        {
            await _orderService.DeleteRangeOrderAsync(request.OrderIds);

            return new();
        }
    }
}
