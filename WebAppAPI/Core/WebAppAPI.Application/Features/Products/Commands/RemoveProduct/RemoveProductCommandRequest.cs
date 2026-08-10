using MediatR;

namespace WebAppAPI.Application.Features.Products.Commands.RemoveProduct
{
    public sealed class RemoveProductCommandRequest : IRequest
    {
        public required string Id { get; init; }
    }
}
