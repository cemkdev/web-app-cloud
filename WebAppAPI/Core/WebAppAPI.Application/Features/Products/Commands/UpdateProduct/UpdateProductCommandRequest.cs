using MediatR;

namespace WebAppAPI.Application.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandRequest : IRequest<UpdateProductCommandResponse>
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public int? Stock { get; set; }
        public float? Price { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}
