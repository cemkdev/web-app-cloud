using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Products.Queries.GetProductImages
{
    public class GetProductImagesQueryHandler : IRequestHandler<GetProductImagesQueryRequest, List<GetProductImagesQueryResponse>>
    {
        readonly IProductService _productService;

        public GetProductImagesQueryHandler(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<List<GetProductImagesQueryResponse>> Handle(GetProductImagesQueryRequest request, CancellationToken cancellationToken)
        {
            var images = await _productService.GetProductImagesAsync(request.Id);

            return images.Select(image => new GetProductImagesQueryResponse
            {
                Id = image.Id,
                Path = image.Path,
                FileName = image.FileName,
                CoverImage = image.CoverImage
            }).ToList();
        }
    }
}
