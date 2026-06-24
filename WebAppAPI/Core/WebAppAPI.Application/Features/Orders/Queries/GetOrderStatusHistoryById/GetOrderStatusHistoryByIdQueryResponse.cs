using WebAppAPI.Application.DTOs.Order;

namespace WebAppAPI.Application.Features.Orders.Queries.GetOrderStatusHistoryById
{
    public class GetOrderStatusHistoryByIdQueryResponse
    {
        public int CurrentStatusId { get; set; }
        public List<StatusChangeEntry> History { get; set; }
    }
}
