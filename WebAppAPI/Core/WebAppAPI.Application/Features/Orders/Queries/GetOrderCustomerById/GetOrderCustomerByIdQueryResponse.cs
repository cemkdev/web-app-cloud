namespace WebAppAPI.Application.Features.Orders.Queries.GetOrderCustomerById
{
    public sealed class GetOrderCustomerByIdQueryResponse
    {
        public required string FullName { get; init; }
        public string? Email { get; init; }
        public string? PhoneNumber { get; init; }
    }
}
