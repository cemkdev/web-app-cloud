using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppAPI.Application.Abstractions.Storage.Models;
using WebAppAPI.Application.Consts;
using WebAppAPI.Application.CustomAttributes;
using WebAppAPI.Application.Enums;
using WebAppAPI.Application.Features.Products.Commands.ChangeCoverImage;
using WebAppAPI.Application.Features.Products.Commands.CreateProduct;
using WebAppAPI.Application.Features.Products.Commands.RemoveProduct;
using WebAppAPI.Application.Features.Products.Commands.RemoveProductImage;
using WebAppAPI.Application.Features.Products.Commands.RemoveRangeProduct;
using WebAppAPI.Application.Features.Products.Commands.UpdateProduct;
using WebAppAPI.Application.Features.Products.Commands.UploadProductImage;
using WebAppAPI.Application.Features.Products.Queries.GetAllProducts;
using WebAppAPI.Application.Features.Products.Queries.GetProductById;
using WebAppAPI.Application.Features.Products.Queries.GetProductDetail;
using WebAppAPI.Application.Features.Products.Queries.GetProductImages;
using WebAppAPI.Application.Features.Products.Queries.GetQrCodeFromProduct;
using WebAppAPI.Domain.Constants;

namespace WebAppAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet("get-all-products")]
        public async Task<ActionResult<GetAllProductsQueryResponse>> GetAllProducts([FromQuery] GetAllProductsQueryRequest request, CancellationToken cancellationToken)
        {
            GetAllProductsQueryResponse response = await _mediator.Send(request, cancellationToken);

            return Ok(response);
        }

        [HttpGet("get-product-by-id/{id}")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Products, Definition = "Get Product By Id", ActionType = ActionType.Read, AdminOnly = true)]
        public async Task<ActionResult<GetProductByIdQueryResponse>> GetProductById([FromRoute] string id, CancellationToken cancellationToken)
        {
            GetProductByIdQueryResponse response = await _mediator.Send(
                new GetProductByIdQueryRequest
                {
                    Id = id
                },
                cancellationToken);

            return Ok(response);
        }

        [HttpGet("get-product-detail/{id}")]
        public async Task<ActionResult<GetProductDetailQueryResponse>> GetProductDetail([FromRoute] string id, CancellationToken cancellationToken)
        {
            GetProductDetailQueryResponse response = await _mediator.Send(
                new GetProductDetailQueryRequest
                {
                    Id = id
                },
                cancellationToken);

            return Ok(response);
        }

        [HttpGet("get-product-images-by-product-id/{id}")]
        public async Task<ActionResult<IReadOnlyList<GetProductImagesQueryResponse>>> GetProductImages([FromRoute] string id, CancellationToken cancellationToken)
        {
            IReadOnlyList<GetProductImagesQueryResponse> response = await _mediator.Send(
                new GetProductImagesQueryRequest
                {
                    Id = id
                },
                cancellationToken);

            return Ok(response);
        }

        [HttpGet("qrcode/{productId}")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Products, Definition = "Get Product QR Code", ActionType = ActionType.Read, AdminOnly = true)]
        public async Task<ActionResult> GetQrCodeFromProduct([FromRoute] string productId, CancellationToken cancellationToken)
        {
            GetQrCodeFromProductQueryResponse response = await _mediator.Send(new GetQrCodeFromProductQueryRequest
            {
                ProductId = productId
            },
            cancellationToken);

            return File(response.QrCode, "image/png");
        }

        [HttpPost("create-product")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Products, Definition = "Create Product", ActionType = ActionType.Write, AdminOnly = true)]
        public async Task<ActionResult<CreateProductCommandResponse>> CreateProduct([FromBody] CreateProductCommandRequest request, CancellationToken cancellationToken)
        {
            CreateProductCommandResponse response = await _mediator.Send(request, cancellationToken);

            return CreatedAtAction(
                nameof(GetProductDetail),
                new { id = response.Id },
                response);
        }

        [HttpPut("update-product")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Products, Definition = "Update Product", ActionType = ActionType.Update, AdminOnly = true)]
        public async Task<ActionResult> UpdateProduct([FromBody] UpdateProductCommandRequest request, CancellationToken cancellationToken)
        {
            await _mediator.Send(request, cancellationToken);

            return NoContent();
        }

        [HttpPost("upload")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Products, Definition = "Upload Files", ActionType = ActionType.Write, AdminOnly = true)]
        public async Task<ActionResult> Upload([FromQuery] string id, CancellationToken cancellationToken)
        {
            List<StorageUploadFile> files = Request.Form.Files
                .Select(file => new StorageUploadFile
                {
                    FileName = file.FileName,
                    Content = file.OpenReadStream(),
                    Length = file.Length,
                    ContentType = file.ContentType
                })
                .ToList();

            try
            {
                await _mediator.Send(new UploadProductImageCommandRequest
                {
                    Id = id,
                    Files = files
                },
                cancellationToken);

                return NoContent();
            }
            finally
            {
                foreach (StorageUploadFile file in files)
                    await file.Content.DisposeAsync();
            }
        }

        [HttpPut("change-cover-image")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Products, Definition = "Change Cover Image", ActionType = ActionType.Update, AdminOnly = true)]
        public async Task<ActionResult> ChangeCoverImage([FromQuery] string imageId, [FromQuery] string productId, CancellationToken cancellationToken)
        {
            await _mediator.Send(new ChangeCoverImageCommandRequest
            {
                ProductId = productId,
                ImageId = imageId
            },
            cancellationToken);

            return NoContent();
        }

        [HttpDelete("delete-product-image/{productId}")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Products, Definition = "Delete Product Image", ActionType = ActionType.Delete, AdminOnly = true)]
        public async Task<ActionResult> DeleteProductImage([FromRoute] string productId, [FromQuery] string imageId, CancellationToken cancellationToken)
        {
            await _mediator.Send(new RemoveProductImageCommandRequest
            {
                ProductId = productId,
                ImageId = imageId
            },
            cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Products, Definition = "Delete Product", ActionType = ActionType.Delete, AdminOnly = true)]
        public async Task<ActionResult> Delete([FromRoute] string id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new RemoveProductCommandRequest
            {
                Id = id
            },
            cancellationToken);

            return NoContent();
        }

        [HttpPost("delete-range-of-products")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Products, Definition = "Delete Range of Product", ActionType = ActionType.Delete, AdminOnly = true)]
        public async Task<ActionResult> DeleteRange([FromBody] RemoveRangeProductCommandRequest request, CancellationToken cancellationToken)
        {
            await _mediator.Send(request, cancellationToken);

            return NoContent();
        }
    }
}
