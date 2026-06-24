using MediatR;

namespace WebAppAPI.Application.Features.Products.Commands.RemoveProduct
{
    public class RemoveProductCommandRequest : IRequest<RemoveProductCommandResponse>
    {
        public string Id { get; set; }
    }
}
