using MediatR;

namespace WebAppAPI.Application.Features.Orders.Commands.RemoveOrder
{
    public class RemoveOrderCommandRequest : IRequest<RemoveOrderCommandResponse>
    {
        public string Id { get; set; }
    }
}
