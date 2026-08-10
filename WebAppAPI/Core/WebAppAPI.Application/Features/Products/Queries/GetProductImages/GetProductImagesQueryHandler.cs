using MediatR;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Features.Products.DTOs;

namespace WebAppAPI.Application.Features.Products.Queries.GetProductImages
{
    public sealed class GetProductImagesQueryHandler(IProductService productService) : IRequestHandler<GetProductImagesQueryRequest, IReadOnlyList<GetProductImagesQueryResponse>>
    {
        public async Task<IReadOnlyList<GetProductImagesQueryResponse>> Handle(GetProductImagesQueryRequest request, CancellationToken cancellationToken)
        {
            IReadOnlyList<ProductImageDto> images = await productService.GetProductImagesAsync(
                request.Id,
                cancellationToken);

            return images
                .Select(image => new GetProductImagesQueryResponse
                {
                    Id = image.Id,
                    Path = image.Path,
                    FileName = image.FileName,
                    CoverImage = image.CoverImage
                })
                .ToList();
        }
    }
}
