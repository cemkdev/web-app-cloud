using MediatR;
using WebAppAPI.Application.Abstractions.Hubs;

namespace WebAppAPI.Application.Features.Products.Notifications
{
    public sealed class ProductCreatedNotificationHandler(IProductHubService productHubService)
        : INotificationHandler<ProductCreatedNotification>
    {
        public Task Handle(ProductCreatedNotification notification, CancellationToken cancellationToken)
            => productHubService.ProductAddedMessageAsync($"'{notification.ProductName}' has been added.");
    }
}
