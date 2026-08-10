using MediatR;
using WebAppAPI.Application.Abstractions.Storage.Models;

namespace WebAppAPI.Application.Features.Products.Commands.UploadProductImage
{
    public sealed class UploadProductImageCommandRequest : IRequest
    {
        public required string Id { get; init; }
        public required IReadOnlyCollection<StorageUploadFile> Files { get; init; }
    }
}
