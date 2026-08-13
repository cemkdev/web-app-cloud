using MediatR;

namespace WebAppAPI.Application.Features.Baskets.Queries.GetAllBasketItems
{
    public sealed class GetAllBasketItemsQueryRequest : IRequest<IReadOnlyCollection<GetAllBasketItemsQueryResponse>>
    {
    }
}
