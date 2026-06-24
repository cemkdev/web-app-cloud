using MediatR;

namespace WebAppAPI.Application.Features.Products.Queries.GetProductImages
{
    public class GetProductImagesQueryRequest : IRequest<List<GetProductImagesQueryResponse>>
    {
        public string Id { get; set; }
    }
}
