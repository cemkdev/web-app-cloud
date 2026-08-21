namespace WebAppAPI.Application.Features.Orders.Commands.CreateOrder
{
    public sealed class OrderCreateDto
    {
        public required string Address { get; init; }
        public required string Description { get; init; }
    }
}
