using MediatR;

namespace WebAppAPI.Application.Features.Products.Notifications
{
    public sealed class ProductCreatedNotification : INotification
    {
        public required Guid ProductId { get; init; }
        public required string ProductName { get; init; }
    }
}
