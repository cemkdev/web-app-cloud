using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppAPI.Application.Consts;
using WebAppAPI.Application.CustomAttributes;
using WebAppAPI.Application.Enums;
using WebAppAPI.Domain.Constants;
using WebAppAPI.Application.Features.Baskets.Commands.RemoveBasketItem;
using WebAppAPI.Application.Features.Baskets.Commands.UpdateQuantity;
using WebAppAPI.Application.Features.Baskets.Commands.AddItemToBasket;
using WebAppAPI.Application.Features.Baskets.Queries.GetAllBasketItems;

namespace WebAppAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
    public class BasketsController : ControllerBase
    {
        readonly IMediator _mediator;

        public BasketsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("get-all-basket-items")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Baskets, ActionType = ActionType.Read, Definition = "Get All Basket Items")]
        public async Task<IActionResult> GetAllBasketItems([FromQuery] GetAllBasketItemsQueryRequest getAllBasketItemsQueryRequest)
        {
            List<GetAllBasketItemsQueryResponse> response = await _mediator.Send(getAllBasketItemsQueryRequest);
            return Ok(response);
        }

        [HttpPost("add-item-to-basket")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Baskets, ActionType = ActionType.Write, Definition = "Add Item to Basket")]
        public async Task<IActionResult> AddItemToBasket(AddItemToBasketCommandRequest addItemToBasketCommandRequest)
        {
            AddItemToBasketCommandResponse response = await _mediator.Send(addItemToBasketCommandRequest);
            return Ok(response);
        }

        [HttpPut("update-basket-item-quantity")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Baskets, ActionType = ActionType.Update, Definition = "Update Basket Item Quantity")]
        public async Task<IActionResult> UpdateQuantity(UpdateQuantityCommandRequest updateQuantityCommandRequest)
        {
            if (updateQuantityCommandRequest.Quantity < 1)
                throw new BadHttpRequestException("Invalid Request!");

            UpdateQuantityCommandResponse response = await _mediator.Send(updateQuantityCommandRequest);
            return Ok(response);
        }

        [HttpDelete("remove-basket-item-by-id/{BasketItemId}")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Baskets, ActionType = ActionType.Delete, Definition = "Remove Basket Item")]
        public async Task<IActionResult> RemoveBasketItem([FromRoute] RemoveBasketItemCommandRequest removeBasketItemCommandRequest)
        {
            RemoveBasketItemCommandResponse response = await _mediator.Send(removeBasketItemCommandRequest);
            return Ok(response);
        }
    }
}
