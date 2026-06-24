using MediatR;

namespace WebAppAPI.Application.Features.Products.Commands.ChangeCoverImage
{
    public class ChangeCoverImageCommandRequest : IRequest<ChangeCoverImageCommandResponse>
    {
        public string ImageId { get; set; }
        public string ProductId { get; set; }
    }
}
