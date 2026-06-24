using MediatR;

namespace WebAppAPI.Application.Features.Orders.Queries.GetOrderStatusHistoryById
{
    public class GetOrderStatusHistoryByIdQueryRequest : IRequest<GetOrderStatusHistoryByIdQueryResponse>
    {
        public string OrderId { get; set; }
    }
}
