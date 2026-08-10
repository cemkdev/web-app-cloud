using MediatR;

namespace WebAppAPI.Application.Features.Products.Queries.GetAllProducts
{
    public sealed class GetAllProductsQueryRequest : IRequest<GetAllProductsQueryResponse>
    {
        public int Page { get; init; } = 0;
        public int Size { get; init; } = 5;
    }
}
