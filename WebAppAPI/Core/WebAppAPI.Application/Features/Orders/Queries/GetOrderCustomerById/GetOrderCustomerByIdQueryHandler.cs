using MediatR;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Application.Features.Orders.Queries.GetOrderCustomerById
{
    public sealed class GetOrderCustomerByIdQueryHandler(IOrderService orderService)
        : IRequestHandler<GetOrderCustomerByIdQueryRequest, GetOrderCustomerByIdQueryResponse>
    {
        public async Task<GetOrderCustomerByIdQueryResponse> Handle(GetOrderCustomerByIdQueryRequest request, CancellationToken cancellationToken)
        {
            GetOrderCustomerByIdDto customer = await orderService.GetOrderCustomerByIdAsync(request.Id, cancellationToken);

            return new GetOrderCustomerByIdQueryResponse
            {
                FullName = customer.FullName,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber
            };
        }
    }
}
