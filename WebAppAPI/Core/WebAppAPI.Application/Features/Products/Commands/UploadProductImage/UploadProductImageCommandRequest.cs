using MediatR;
using Microsoft.AspNetCore.Http;

namespace WebAppAPI.Application.Features.Products.Commands.UploadProductImage
{
    public class UploadProductImageCommandRequest : IRequest<UploadProductImageCommandResponse>
    {
        public string Id { get; set; }
        public IFormFileCollection? Files { get; set; }
    }
}
