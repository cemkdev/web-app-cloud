using MediatR;

namespace WebAppAPI.Application.Features.Products.Queries.GetProductDetail
{
    public sealed class GetProductDetailQueryRequest : IRequest<GetProductDetailQueryResponse>
    {
        public required string Id { get; init; }
    }
}
