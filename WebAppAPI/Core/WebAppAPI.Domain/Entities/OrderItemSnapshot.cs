using WebAppAPI.Domain.Entities.Common;

namespace WebAppAPI.Domain.Entities
{
    public sealed class OrderItemSnapshot : BaseEntity
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }

        public required string Name { get; set; }
        public required string Title { get; set; }
        public required string Description { get; set; }
        public float? Rating { get; set; }

        public float UnitPrice { get; set; }
        public int Quantity { get; set; }

        public bool IsProductDeleted { get; set; }
    }
}
