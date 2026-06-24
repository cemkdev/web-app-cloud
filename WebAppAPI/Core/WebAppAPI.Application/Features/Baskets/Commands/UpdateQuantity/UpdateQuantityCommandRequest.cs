using MediatR;

namespace WebAppAPI.Application.Features.Baskets.Commands.UpdateQuantity
{
    public class UpdateQuantityCommandRequest : IRequest<UpdateQuantityCommandResponse>
    {
        public string BasketItemId { get; set; }
        public string ProductId { get; set; } // TODO Section 3B / 6.6: ProductId artık UpdateQuantity flow'unda kullanılmayacak. request/DTO contract'larını standardize ederken kaldır.
        public int Quantity { get; set; }
    }
}
