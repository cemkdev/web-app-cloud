using MediatR;
using Microsoft.Extensions.Options;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.DTOs;
using WebAppAPI.Application.Options.Storage;

namespace WebAppAPI.Application.Features.Queries.Basket.GetAllBasketItems
{
    public class GetAllBasketItemsQueryHandler : IRequestHandler<GetAllBasketItemsQueryRequest, List<GetAllBasketItemsQueryResponse>>
    {
        readonly IBasketService _basketService;
        readonly BaseStorageOptions _baseStorageOptions;

        public GetAllBasketItemsQueryHandler(IBasketService basketService, IOptions<BaseStorageOptions> baseStorageOptions)
        {
            _basketService = basketService;
            _baseStorageOptions = baseStorageOptions.Value;
        }

        public async Task<List<GetAllBasketItemsQueryResponse>> Handle(GetAllBasketItemsQueryRequest request, CancellationToken cancellationToken)
        {
            var basketItems = await _basketService.GetAllBasketItemsAsync();

            return basketItems.Select(bi => new GetAllBasketItemsQueryResponse()
            {
                BasketItemId = bi.Id.ToString(),
                ProductId = bi.ProductId.ToString(),
                Name = bi.Product.Name,
                Description = bi.Product.Description,
                Price = bi.Product.Price,
                Stock = bi.Product.Stock,
                Quantity = bi.Quantity,
                ProductImageFile = bi.Product.ProductImageFiles?.Where(pif => pif.CoverImage == true).Select(pif => new BasketProductImageFile()
                {
                    ProductImageFileId = pif.Id.ToString(),
                    FileName = pif.FileName,
                    Path = $"{_baseStorageOptions.Url}/{pif.Path}",
                }).FirstOrDefault()
            }).ToList();
        }
    }
}
