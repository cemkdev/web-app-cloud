using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Baskets.Queries.GetAllBasketItems
{
    public sealed class GetAllBasketItemsQueryHandler(IBasketService basketService) : IRequestHandler<GetAllBasketItemsQueryRequest, IReadOnlyCollection<GetAllBasketItemsQueryResponse>>
    {
        public async Task<IReadOnlyCollection<GetAllBasketItemsQueryResponse>> Handle(GetAllBasketItemsQueryRequest request, CancellationToken cancellationToken)
        {
            IReadOnlyList<BasketItemListDto> basketItems = await basketService.GetAllBasketItemsAsync(cancellationToken);

            return basketItems.Select(item => new GetAllBasketItemsQueryResponse
            {
                BasketItemId = item.BasketItemId.ToString(),
                ProductId = item.ProductId.ToString(),
                Name = item.Name,
                Description = item.Description,
                Stock = item.Stock,
                Price = item.Price,
                Quantity = item.Quantity,
                ProductImageFile = item.ProductImageFile
            }).ToList();
        }
    }
}
