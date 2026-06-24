using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
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
using WebAppAPI.Application.Features.Products.Queries.GetByIdProduct;
using WebAppAPI.Application.Features.Products.Queries.GetProductImages;
using WebAppAPI.Application.Features.Products.Queries.GetQrCodeFromProduct;
using WebAppAPI.Domain.Constants;

namespace WebAppAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("get-all-products")]
        public async Task<IActionResult> GetAllProducts([FromQuery] GetAllProductsQueryRequest getAllProductsQueryRequest)
        {
            GetAllProductsQueryResponse response = await _mediator.Send(getAllProductsQueryRequest);
            return Ok(response);
        }

        [HttpGet("get-product-by-id/{Id}")]
        public async Task<IActionResult> GetProductById([FromRoute] GetByIdProductQueryRequest getByIdProductQueryRequest)
        {
            GetByIdProductQueryResponse response = await _mediator.Send(getByIdProductQueryRequest);
            return Ok(response);
        }

        [HttpPost("create-product")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Products, Definition = "Create Product", ActionType = ActionType.Write, AdminOnly = true)]
        public async Task<IActionResult> CreateProduct(CreateProductCommandRequest createProductCommandRequest)
        {
            CreateProductCommandResponse response = await _mediator.Send(createProductCommandRequest);
            return StatusCode((int)HttpStatusCode.Created);
        }

        [HttpPut("update-product")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Products, Definition = "Update Product", ActionType = ActionType.Update, AdminOnly = true)]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductCommandRequest updateProductCommandRequest)
        {
            UpdateProductCommandResponse response = await _mediator.Send(updateProductCommandRequest);
            return Ok();
        }

        [HttpDelete("{Id}")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Products, Definition = "Delete Product", ActionType = ActionType.Delete, AdminOnly = true)]
        public async Task<IActionResult> Delete([FromRoute] RemoveProductCommandRequest removeProductCommandRequest)
        {
            RemoveProductCommandResponse response = await _mediator.Send(removeProductCommandRequest);
            return Ok();
        }

        [HttpPost("delete-range-of-products")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Products, Definition = "Delete Range of Product", ActionType = ActionType.Delete, AdminOnly = true)]
        public async Task<IActionResult> DeleteRange([FromBody] RemoveRangeProductCommandRequest removeRangeProductCommandRequest)
        {
            RemoveRangeProductCommandResponse response = await _mediator.Send(removeRangeProductCommandRequest);
            return Ok();
        }

        // Used for all file upload requests coming from the client. However, it's only used for uploading product images right now.
        [HttpPost("[action]")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Products, Definition = "Upload Files", ActionType = ActionType.Write, AdminOnly = true)]
        public async Task<IActionResult> Upload([FromQuery] UploadProductImageCommandRequest uploadProductImageCommandRequest)
        {
            uploadProductImageCommandRequest.Files = Request.Form.Files;
            UploadProductImageCommandResponse response = await _mediator.Send(uploadProductImageCommandRequest);

            return Ok();
        }

        [HttpGet("get-product-images-by-product-id/{id}")]
        public async Task<IActionResult> GetProductImages([FromRoute] GetProductImagesQueryRequest getProductImagesQueryRequest)
        {
            List<GetProductImagesQueryResponse> response = await _mediator.Send(getProductImagesQueryRequest);
            return Ok(response);
        }

        [HttpDelete("delete-product-image/{Id}")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Products, Definition = "Delete Product Image", ActionType = ActionType.Delete, AdminOnly = true)]
        public async Task<IActionResult> DeleteProductImage([FromRoute] RemoveProductImageCommandRequest removeProductImageCommandRequest, [FromQuery] string imageId)
        {
            removeProductImageCommandRequest.ImageId = imageId;
            RemoveProductImageCommandResponse response = await _mediator.Send(removeProductImageCommandRequest);
            return Ok();
        }

        [HttpPut("change-cover-image")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Products, Definition = "Change Cover Image", ActionType = ActionType.Update, AdminOnly = true)]
        public async Task<IActionResult> ChangeCoverImage([FromQuery] ChangeCoverImageCommandRequest changeCoverImageCommandRequest)
        {
            ChangeCoverImageCommandResponse response = await _mediator.Send(changeCoverImageCommandRequest);
            return Ok(response);
        }

        [HttpGet("qrcode/{productId}")]
        [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Products, Definition = "Get Product QR Code", ActionType = ActionType.Read, AdminOnly = true)]
        public async Task<IActionResult> GetQrCodeFromProduct([FromRoute] GetQrCodeFromProductQueryRequest getQrCodeFromProductQueryRequest)
        {
            GetQrCodeFromProductQueryResponse response = await _mediator.Send(getQrCodeFromProductQueryRequest);
            return File(response.QrCode, "image/png");
        }
    }
}
