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
using WebAppAPI.Application.Features.Orders.Queries.GetMyOrderById;
using WebAppAPI.Application.Features.Orders.Queries.GetMyOrders;
using WebAppAPI.Application.Features.Orders.Queries.GetMyOrderStatusHistoryById;
using WebAppAPI.Application.Features.Orders.Queries.GetOrderById;
using WebAppAPI.Application.Features.Orders.Queries.GetOrderCustomerById;
using WebAppAPI.Application.Features.Orders.Queries.GetOrderStatusHistoryById;
using WebAppAPI.Domain.Constants;

namespace WebAppAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = AuthSchemes.Authenticated)]
    public class OrdersController(IMediator mediator) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;

        [HttpGet("get-all-orders")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Orders, Definition = "Get All Orders", ActionType = ActionType.Read, AdminOnly = true)]
        public async Task<ActionResult<GetAllOrdersQueryResponse>> GetAllOrders([FromQuery] GetAllOrdersQueryRequest request, CancellationToken cancellationToken)
        {
            GetAllOrdersQueryResponse response = await _mediator.Send(request, cancellationToken);

            return Ok(response);
        }

        [HttpGet("get-my-orders")]
        public async Task<ActionResult<GetMyOrdersQueryResponse>> GetMyOrders([FromQuery] GetMyOrdersQueryRequest request, CancellationToken cancellationToken)
        {
            GetMyOrdersQueryResponse response = await _mediator.Send(request, cancellationToken);

            return Ok(response);
        }

        [HttpGet("get-order-by-id/{id}")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Orders, Definition = "Get Order by Id", ActionType = ActionType.Read, AdminOnly = true)]
        public async Task<ActionResult<GetOrderByIdQueryResponse>> GetOrderById([FromRoute] string id, CancellationToken cancellationToken)
        {
            GetOrderByIdQueryResponse response = await _mediator.Send(
                new GetOrderByIdQueryRequest
                {
                    Id = id
                },
                cancellationToken);

            return Ok(response);
        }

        [HttpGet("get-order-customer-by-id/{id}")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Orders, Definition = "Get Order Customer by Id", ActionType = ActionType.Read, AdminOnly = true)]
        public async Task<ActionResult<GetOrderCustomerByIdQueryResponse>> GetOrderCustomerById([FromRoute] string id, CancellationToken cancellationToken)
        {
            GetOrderCustomerByIdQueryResponse response = await _mediator.Send(
                new GetOrderCustomerByIdQueryRequest
                {
                    Id = id
                },
                cancellationToken);

            return Ok(response);
        }

        [HttpGet("get-my-order-by-id/{id}")]
        public async Task<ActionResult<GetMyOrderByIdQueryResponse>> GetMyOrderById([FromRoute] string id, CancellationToken cancellationToken)
        {
            GetMyOrderByIdQueryResponse response = await _mediator.Send(
                new GetMyOrderByIdQueryRequest
                {
                    Id = id
                },
                cancellationToken);

            return Ok(response);
        }

        [HttpGet("get-order-status-history-by-id/{orderId}")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Orders, Definition = "Get Order Status History by Id", ActionType = ActionType.Read, AdminOnly = true)]
        public async Task<ActionResult<GetOrderStatusHistoryByIdQueryResponse>> GetOrderStatusHistoryById([FromRoute] string orderId, CancellationToken cancellationToken)
        {
            GetOrderStatusHistoryByIdQueryResponse response = await _mediator.Send(
                new GetOrderStatusHistoryByIdQueryRequest
                {
                    OrderId = orderId
                },
                cancellationToken);

            return Ok(response);
        }

        [HttpGet("get-my-order-status-history-by-id/{orderId}")]
        public async Task<ActionResult<GetMyOrderStatusHistoryByIdQueryResponse>> GetMyOrderStatusHistoryById([FromRoute] string orderId, CancellationToken cancellationToken)
        {
            GetMyOrderStatusHistoryByIdQueryResponse response = await _mediator.Send(
                new GetMyOrderStatusHistoryByIdQueryRequest
                {
                    OrderId = orderId
                },
                cancellationToken);

            return Ok(response);
        }

        [HttpPost("create-order")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Orders, Definition = "Create Order", ActionType = ActionType.Write)]
        public async Task<ActionResult> CreateOrder([FromBody] CreateOrderCommandRequest request, CancellationToken cancellationToken)
        {
            await _mediator.Send(request, cancellationToken);

            return NoContent();
        }

        [HttpPut("update-order-status")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Orders, Definition = "Update Order Status", ActionType = ActionType.Update, AdminOnly = true)]
        public async Task<ActionResult> UpdateStatus([FromBody] UpdateStatusCommandRequest request, CancellationToken cancellationToken)
        {
            await _mediator.Send(request, cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Orders, Definition = "Delete Order", ActionType = ActionType.Delete, AdminOnly = true)]
        public async Task<ActionResult> Delete([FromRoute] string id, CancellationToken cancellationToken)
        {
            await _mediator.Send(
                new RemoveOrderCommandRequest
                {
                    Id = id
                },
                cancellationToken);

            return NoContent();
        }

        [HttpPost("delete-range")]
        [AuthorizeDefinition(Menu = AuthorizeDefinitionConstants.Orders, Definition = "Delete Range of Order", ActionType = ActionType.Delete, AdminOnly = true)]
        public async Task<ActionResult> DeleteRange([FromBody] RemoveRangeOrderCommandRequest request, CancellationToken cancellationToken)
        {
            await _mediator.Send(request, cancellationToken);

            return NoContent();
        }
    }
}
