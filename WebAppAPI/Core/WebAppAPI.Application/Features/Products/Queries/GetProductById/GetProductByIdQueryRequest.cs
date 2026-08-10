using MediatR;

namespace WebAppAPI.Application.Features.Products.Queries.GetProductById
{
    public sealed class GetProductByIdQueryRequest : IRequest<GetProductByIdQueryResponse>
    {
        public required string Id { get; init; }
    }
}
