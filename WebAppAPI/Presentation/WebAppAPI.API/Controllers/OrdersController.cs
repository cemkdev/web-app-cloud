using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppAPI.Application.Consts;
using WebAppAPI.Application.CustomAttributes;
using WebAppAPI.Application.Enums;
using WebAppAPI.Application.Features.Orders.Commands.CreateOrder;
using WebAppAPI.Application.Features.Orders.Commands.RemoveOrder;
using WebAppAPI.Application.Features.Orders.Commands.RemoveRangeOrder;
using WebAppAPI.Application.Features.Orders.Commands.UpdateStatus;
using WebAppAPI.Application.Features.Orders.Queries.GetAllOrders;
using WebAppAPI.Application.Features.Orders.Queries.GetOrderById;
using WebAppAPI.Application.Features.Orders.Queries.GetOrderStatusHistoryById;
using WebAppAPI.Domain.Constants;

namespace WebAppAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
    public class OrdersController : ControllerBase
    {
        readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("get-all-orders")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Orders, Definition = "Get All Orders", ActionType = ActionType.Read, AdminOnly = true)]
        public async Task<ActionResult> GetAllOrders([FromQuery] GetAllOrdersQueryRequest getAllOrdersQueryRequest)
        {
            GetAllOrdersQueryResponse response = await _mediator.Send(getAllOrdersQueryRequest);
            return Ok(response);
        }

        [HttpGet("get-order-by-id/{id}")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Orders, Definition = "Get Order by Id", ActionType = ActionType.Read)]
        public async Task<ActionResult> GetOrderById([FromRoute] GetOrderByIdQueryRequest getOrderByIdQueryRequest)
        {
            GetOrderByIdQueryResponse response = await _mediator.Send(getOrderByIdQueryRequest);
            return Ok(response);
        }

        [HttpPost("create-order")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Orders, Definition = "Create Order", ActionType = ActionType.Write)]
        public async Task<ActionResult> CreateOrder(CreateOrderCommandRequest createOrderCommandRequest)
        {
            CreateOrderCommandResponse response = await _mediator.Send(createOrderCommandRequest);
            return Ok(response);
        }

        [HttpDelete("{id}")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Orders, Definition = "Delete Order", ActionType = ActionType.Delete, AdminOnly = true)]
        public async Task<IActionResult> Delete([FromRoute] RemoveOrderCommandRequest removeOrderCommandRequest)
        {
            RemoveOrderCommandResponse response = await _mediator.Send(removeOrderCommandRequest);
            return Ok();
        }

        [HttpPost("delete-range")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Orders, Definition = "Delete Range of Order", ActionType = ActionType.Delete, AdminOnly = true)]
        public async Task<IActionResult> DeleteRange([FromBody] RemoveRangeOrderCommandRequest removeRangeOrderCommandRequest)
        {
            RemoveRangeOrderCommandResponse response = await _mediator.Send(removeRangeOrderCommandRequest);
            return Ok();
        }

        [HttpPut("update-order-status")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Orders, Definition = "Update Order Status", ActionType = ActionType.Update, AdminOnly = true)]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusCommandRequest updateStatusCommandRequest)
        {
            UpdateStatusCommandResponse response = await _mediator.Send(updateStatusCommandRequest);
            return Ok(response);
        }

        [HttpGet("get-order-status-history-by-id/{orderId}")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Orders, Definition = "Get Order Status History by Id", ActionType = ActionType.Read)]
        public async Task<ActionResult> GetOrderStatusHistoryById([FromRoute] GetOrderStatusHistoryByIdQueryRequest getOrderStatusHistoryByIdQueryRequest)
        {
            GetOrderStatusHistoryByIdQueryResponse response = await _mediator.Send(getOrderStatusHistoryByIdQueryRequest);
            return Ok(response);
        }
    }
}
