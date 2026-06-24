using MediatR;

namespace WebAppAPI.Application.Features.Orders.Commands.RemoveRangeOrder
{
    public class RemoveRangeOrderCommandRequest : IRequest<RemoveRangeOrderCommandResponse>
    {
        public List<string> OrderIds { get; set; }
    }
}
