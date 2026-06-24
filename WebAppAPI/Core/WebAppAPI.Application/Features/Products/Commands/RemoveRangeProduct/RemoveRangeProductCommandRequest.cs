using MediatR;

namespace WebAppAPI.Application.Features.Products.Commands.RemoveRangeProduct
{
    public class RemoveRangeProductCommandRequest : IRequest<RemoveRangeProductCommandResponse>
    {
        public List<string> ProductIds { get; set; }
    }
}
