using MediatR;

namespace WebAppAPI.Application.Features.Baskets.Queries.GetAllBasketItems
{
    public class GetAllBasketItemsQueryRequest : IRequest<List<GetAllBasketItemsQueryResponse>>
    {
    }
}
