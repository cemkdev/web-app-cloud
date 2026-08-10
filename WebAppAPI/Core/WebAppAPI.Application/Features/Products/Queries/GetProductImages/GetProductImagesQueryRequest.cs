using MediatR;

namespace WebAppAPI.Application.Features.Products.Queries.GetProductImages
{
    public sealed class GetProductImagesQueryRequest : IRequest<IReadOnlyList<GetProductImagesQueryResponse>>
    {
        public required string Id { get; init; }
    }
}
