using MediatR;

namespace WebAppAPI.Application.Features.Orders.Commands.UpdateStatus
{
    public class UpdateStatusCommandRequest : IRequest<UpdateStatusCommandResponse>
    {
        public string OrderId { get; set; }
        public int NewStatus { get; set; }
    }
}
