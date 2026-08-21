using MediatR;
using WebAppAPI.Application.Abstractions.Hubs;

namespace WebAppAPI.Application.Features.Orders.Notifications
{
    public sealed class OrderCreatedNotificationHandler(IOrderHubService orderHubService) : INotificationHandler<OrderCreatedNotification>
    {
        public Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken)
            => orderHubService.OrderAddedMessageAsync("You have a new order!");
    }
}
