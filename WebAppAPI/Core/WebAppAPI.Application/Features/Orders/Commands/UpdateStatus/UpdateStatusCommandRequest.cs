using MediatR;
using WebAppAPI.Domain.Enums;

namespace WebAppAPI.Application.Features.Orders.Commands.UpdateStatus
{
    public sealed class UpdateStatusCommandRequest : IRequest
    {
        public required string OrderId { get; init; }
        public OrderStatusEnum NewStatus { get; init; }
    }
}
