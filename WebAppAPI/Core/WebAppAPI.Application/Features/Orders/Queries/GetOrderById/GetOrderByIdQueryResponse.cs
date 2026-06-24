using WebAppAPI.Application.DTOs;
using WebAppAPI.Application.DTOs.Order;
using WebAppAPI.Domain.Entities;

namespace WebAppAPI.Application.Features.Orders.Queries.GetOrderById
{
    public class GetOrderByIdQueryResponse
    {
        public string Id { get; set; }
        public string OrderCode { get; set; }
        public string Address { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public int StatusId { get; set; }
        public List<OrderItems> OrderBasketItems { get; set; }
    }
}
