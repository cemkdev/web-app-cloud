using MediatR;

namespace WebAppAPI.Application.Features.Orders.Queries.GetOrderCustomerById
{
    public sealed class GetOrderCustomerByIdQueryRequest : IRequest<GetOrderCustomerByIdQueryResponse>
    {
        public required string Id { get; init; }
    }
}
