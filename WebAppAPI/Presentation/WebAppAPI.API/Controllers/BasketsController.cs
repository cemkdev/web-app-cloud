using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppAPI.Application.Consts;
using WebAppAPI.Application.CustomAttributes;
using WebAppAPI.Application.Enums;
using WebAppAPI.Application.Features.Baskets.Commands.AddItemToBasket;
using WebAppAPI.Application.Features.Baskets.Commands.RemoveBasketItem;
using WebAppAPI.Application.Features.Baskets.Commands.UpdateQuantity;
using WebAppAPI.Application.Features.Baskets.Queries.GetAllBasketItems;
using WebAppAPI.Domain.Constants;

namespace WebAppAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
    public class BasketsController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpPost("add-item-to-basket")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Baskets, ActionType = ActionType.Write, Definition = "Add Item to Basket")]
        public async Task<ActionResult> AddItemToBasket(AddItemToBasketCommandRequest request, CancellationToken cancellationToken)
        {
            await _mediator.Send(request, cancellationToken);

            return NoContent();
        }

        [HttpGet("get-all-basket-items")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Baskets, ActionType = ActionType.Read, Definition = "Get All Basket Items")]
        public async Task<ActionResult<IReadOnlyCollection<GetAllBasketItemsQueryResponse>>> GetAllBasketItems(CancellationToken cancellationToken)
        {
            IReadOnlyCollection<GetAllBasketItemsQueryResponse> response = await _mediator.Send(new GetAllBasketItemsQueryRequest(), cancellationToken);

            return Ok(response);
        }

        [HttpPut("update-basket-item-quantity")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Baskets, ActionType = ActionType.Update, Definition = "Update Basket Item Quantity")]
        public async Task<ActionResult> UpdateQuantity(UpdateQuantityCommandRequest request, CancellationToken cancellationToken)
        {
            await _mediator.Send(request, cancellationToken);

            return NoContent();
        }

        [HttpDelete("remove-basket-item-by-id/{basketItemId}")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Baskets, ActionType = ActionType.Delete, Definition = "Remove Basket Item")]
        public async Task<ActionResult> RemoveBasketItem([FromRoute] string basketItemId, CancellationToken cancellationToken)
        {
            await _mediator.Send(
                new RemoveBasketItemCommandRequest
                {
                    BasketItemId = basketItemId,
                },
                cancellationToken);

            return NoContent();
        }
    }
}
