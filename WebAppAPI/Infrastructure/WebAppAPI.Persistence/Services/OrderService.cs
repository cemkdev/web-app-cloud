using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using WebAppAPI.Application.Abstractions.CurrentUser;
using WebAppAPI.Application.Abstractions.Messaging;
using WebAppAPI.Application.Abstractions.Messaging.Messages;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.Features.Orders.Commands.CreateOrder;
using WebAppAPI.Application.Features.Orders.Commands.UpdateStatus;
using WebAppAPI.Application.Features.Orders.Queries.GetAllOrders;
using WebAppAPI.Application.Features.Orders.Queries.GetMyOrderById;
using WebAppAPI.Application.Features.Orders.Queries.GetMyOrders;
using WebAppAPI.Application.Features.Orders.Queries.GetMyOrderStatusHistoryById;
using WebAppAPI.Application.Features.Orders.Queries.GetOrderById;
using WebAppAPI.Application.Features.Orders.Queries.GetOrderCustomerById;
using WebAppAPI.Application.Features.Orders.Queries.GetOrderStatusHistoryById;
using WebAppAPI.Application.Options.Storage;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Domain.Entities;
using WebAppAPI.Domain.Enums;

namespace WebAppAPI.Persistence.Services
{
    public sealed class OrderService(
        ICurrentUserContext currentUserContext,
        IOrderWriteRepository orderWriteRepository,
        IOrderReadRepository orderReadRepository,
        IOrderStatusHistoryReadRepository orderStatusHistoryReadRepository,
        IWriteRepository<OrderStatusHistory> orderStatusHistoryWriteRepository,
        IOrderItemSnapshotReadRepository orderItemSnapshotReadRepository,
        IWriteRepository<OrderItemSnapshot> orderItemSnapshotWriteRepository,
        IBasketReadRepository basketReadRepository,
        IWriteRepository<Basket> basketWriteRepository,
        IBasketItemReadRepository basketItemReadRepository,
        IWriteRepository<BasketItem> basketItemWriteRepository,
        IProductWriteRepository productWriteRepository,
        IProductImageFileReadRepository productImageFileReadRepository,
        IOutboxWriter outboxWriter,
        IOptions<BaseStorageOptions> baseStorageOptions,
        IUnitOfWork unitOfWork) : IOrderService
    {
        private readonly ICurrentUserContext _currentUserContext = currentUserContext;
        private readonly IOrderReadRepository _orderReadRepository = orderReadRepository;
        private readonly IOrderWriteRepository _orderWriteRepository = orderWriteRepository;
        private readonly IOrderStatusHistoryReadRepository _orderStatusHistoryReadRepository = orderStatusHistoryReadRepository;
        private readonly IWriteRepository<OrderStatusHistory> _orderStatusHistoryWriteRepository = orderStatusHistoryWriteRepository;
        private readonly IOrderItemSnapshotReadRepository _orderItemSnapshotReadRepository = orderItemSnapshotReadRepository;
        private readonly IWriteRepository<OrderItemSnapshot> _orderItemSnapshotWriteRepository = orderItemSnapshotWriteRepository;
        private readonly IBasketReadRepository _basketReadRepository = basketReadRepository;
        private readonly IWriteRepository<Basket> _basketWriteRepository = basketWriteRepository;
        private readonly IBasketItemReadRepository _basketItemReadRepository = basketItemReadRepository;
        private readonly IWriteRepository<BasketItem> _basketItemWriteRepository = basketItemWriteRepository;
        private readonly IProductWriteRepository _productWriteRepository = productWriteRepository;
        private readonly IProductImageFileReadRepository _productImageFileReadRepository = productImageFileReadRepository;
        private readonly IOutboxWriter _outboxWriter = outboxWriter;
        private readonly BaseStorageOptions _baseStorageOptions = baseStorageOptions.Value;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<GetAllOrdersDto> GetAllOrdersAsync(int page, int size, CancellationToken cancellationToken)
        {
            if (page < 0)
                throw new ArgumentOutOfRangeException(nameof(page), "Page cannot be negative.");

            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size), "Size must be greater than zero.");

            GetAllOrdersDto result = await _orderReadRepository.GetPagedAsync(page, size, cancellationToken);

            if (result.Orders.Count == 0)
                return result;

            Guid[] orderIds = result.Orders
                .Select(order => order.Id)
                .ToArray();

            IReadOnlyDictionary<Guid, float> totalPrices =
                await _orderItemSnapshotReadRepository.GetTotalPricesByOrderIdsAsync(
                    orderIds,
                    cancellationToken);

            foreach (OrderListItemDto order in result.Orders)
                if (totalPrices.TryGetValue(order.Id, out float totalPrice))
                    order.TotalPrice = totalPrice;

            return result;
        }

        public async Task<GetMyOrdersDto> GetMyOrdersAsync(int page, int size, CancellationToken cancellationToken)
        {
            if (page < 0)
                throw new ArgumentOutOfRangeException(nameof(page), "Page cannot be negative.");

            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size), "Size must be greater than zero.");

            GetMyOrdersDto result = await _orderReadRepository.GetPagedByUserIdAsync(
                _currentUserContext.UserId,
                page,
                size,
                cancellationToken);

            if (result.Orders.Count == 0)
                return result;

            Guid[] orderIds = result.Orders
                .Select(order => order.Id)
                .ToArray();

            IReadOnlyDictionary<Guid, float> totalPrices =
                await _orderItemSnapshotReadRepository.GetTotalPricesByOrderIdsAsync(
                    orderIds,
                    cancellationToken);

            foreach (MyOrderListItemDto order in result.Orders)
                if (totalPrices.TryGetValue(order.Id, out float totalPrice))
                    order.TotalPrice = totalPrice;

            return result;
        }

        public async Task<GetOrderByIdDto> GetOrderByIdAsync(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Order id is required.", nameof(id));

            if (!Guid.TryParse(id, out Guid orderGuid))
                throw new ArgumentException("Order id is invalid.", nameof(id));

            GetOrderByIdDto? order = await _orderReadRepository.GetDetailByIdAsync(orderGuid, cancellationToken);

            if (order is null)
                throw new KeyNotFoundException($"Order with id '{id}' was not found.");

            IReadOnlyDictionary<Guid, OrderDetailItemDto> snapshotItems =
                await _orderItemSnapshotReadRepository.GetDetailItemsByOrderIdAsync(orderGuid, cancellationToken);

            Guid[] productIds = snapshotItems
                .Where(item => !item.Value.IsProductDeleted)
                .Select(item => item.Key)
                .ToArray();

            IReadOnlyDictionary<Guid, ProductImageFile> coverImages =
                await _productImageFileReadRepository.GetCoversByProductIdsAsync(productIds, cancellationToken);

            foreach ((Guid productId, OrderDetailItemDto item) in snapshotItems)
                if (!item.IsProductDeleted && coverImages.TryGetValue(productId, out ProductImageFile? image))
                    item.OrderProductImageFile = new OrderProductImageDto
                    {
                        ProductImageFileId = image.Id,
                        FileName = image.FileName,
                        Path = $"{_baseStorageOptions.Url}/{image.Path}"
                    };

            order.OrderBasketItems = snapshotItems.Values.ToList();

            return order;
        }

        public async Task<GetOrderCustomerByIdDto> GetOrderCustomerByIdAsync(string orderId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                throw new ArgumentException("Order id is required.", nameof(orderId));

            if (!Guid.TryParse(orderId, out Guid orderGuid))
                throw new ArgumentException("Order id is invalid.", nameof(orderId));

            GetOrderCustomerByIdDto? customer = await _orderReadRepository.GetCustomerByOrderIdAsync(orderGuid, cancellationToken);

            if (customer is null)
                throw new KeyNotFoundException($"Order with id '{orderId}' was not found.");

            return customer;
        }

        public async Task<GetMyOrderByIdDto> GetMyOrderByIdAsync(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Order id is required.", nameof(id));

            if (!Guid.TryParse(id, out Guid orderGuid))
                throw new ArgumentException("Order id is invalid.", nameof(id));

            GetMyOrderByIdDto? order = await _orderReadRepository.GetDetailByIdAndUserIdAsync(orderGuid, _currentUserContext.UserId, cancellationToken);

            if (order is null)
                throw new KeyNotFoundException($"Order with id '{id}' was not found.");

            IReadOnlyDictionary<Guid, MyOrderDetailItemDto> snapshotItems =
                await _orderItemSnapshotReadRepository.GetMyDetailItemsByOrderIdAsync(orderGuid, cancellationToken);

            Guid[] productIds = snapshotItems
                .Where(item => !item.Value.IsProductDeleted)
                .Select(item => item.Key)
                .ToArray();

            IReadOnlyDictionary<Guid, ProductImageFile> coverImages =
                await _productImageFileReadRepository.GetCoversByProductIdsAsync(
                    productIds,
                    cancellationToken);

            foreach ((Guid productId, MyOrderDetailItemDto item) in snapshotItems)
                if (!item.IsProductDeleted && coverImages.TryGetValue(productId, out ProductImageFile? image))
                    item.OrderProductImageFile = new MyOrderProductImageDto
                    {
                        ProductImageFileId = image.Id,
                        FileName = image.FileName,
                        Path = $"{_baseStorageOptions.Url}/{image.Path}"
                    };

            order.OrderBasketItems = snapshotItems.Values.ToList();

            return order;
        }

        public async Task<OrderStatusHistoryDto> GetOrderStatusHistoryByIdAsync(string orderId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                throw new ArgumentException("Order id is required.", nameof(orderId));

            if (!Guid.TryParse(orderId, out Guid orderGuid))
                throw new ArgumentException("Order id is invalid.", nameof(orderId));

            int? currentStatusId = await _orderReadRepository.GetStatusIdByIdAsync(orderGuid, cancellationToken);

            if (currentStatusId is null)
                throw new KeyNotFoundException($"Order with id '{orderId}' was not found.");

            IReadOnlyList<OrderStatusHistoryEntryDto> history =
                await _orderStatusHistoryReadRepository.GetStatusHistoryByOrderIdAsync(orderGuid, cancellationToken);

            return new OrderStatusHistoryDto
            {
                CurrentStatusId = currentStatusId.Value,
                History = history
            };
        }

        public async Task<MyOrderStatusHistoryDto> GetMyOrderStatusHistoryByIdAsync(string orderId, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                throw new ArgumentException("Order id is required.", nameof(orderId));

            if (!Guid.TryParse(orderId, out Guid orderGuid))
                throw new ArgumentException("Order id is invalid.", nameof(orderId));

            int? currentStatusId = await _orderReadRepository.GetStatusIdByIdAndUserIdAsync(orderGuid, _currentUserContext.UserId, cancellationToken);

            if (currentStatusId is null)
                throw new KeyNotFoundException($"Order with id '{orderId}' was not found.");

            IReadOnlyList<OrderStatusHistoryEntryDto> history = await _orderStatusHistoryReadRepository.GetStatusHistoryByOrderIdAsync(orderGuid, cancellationToken);

            return new MyOrderStatusHistoryDto
            {
                CurrentStatusId = currentStatusId.Value,
                History = history
                    .Select(entry => new MyOrderStatusHistoryEntryDto
                    {
                        NewStatusId = entry.NewStatusId,
                        ChangedDate = entry.ChangedDate
                    })
                    .ToList()
            };
        }

        public async Task CreateOrderAsync(OrderCreateDto createOrder, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(createOrder);

            if (string.IsNullOrWhiteSpace(createOrder.Address))
                throw new ArgumentException("Order address is required.", nameof(createOrder));

            if (string.IsNullOrWhiteSpace(createOrder.Description))
                throw new ArgumentException("Order description is required.", nameof(createOrder));

            await _unitOfWork.ExecuteInTransactionAsync(
                async transactionCancellationToken =>
                {
                    ActiveBasketOrderData? activeBasket = await _basketReadRepository
                        .GetActiveBasketOrderDataAsync(_currentUserContext.UserId, transactionCancellationToken);

                    if (activeBasket is null)
                        throw new InvalidOperationException("Active basket was not found.");

                    if (string.IsNullOrWhiteSpace(activeBasket.Recipient))
                        throw new InvalidOperationException("The order customer does not have a valid email address.");

                    IReadOnlyList<CreateOrderBasketItemData> basketItems = await _basketItemReadRepository.GetOrderItemsByBasketIdAsync(activeBasket.BasketId, transactionCancellationToken);

                    if (basketItems.Count == 0)
                        throw new InvalidOperationException("Cannot create an order from an empty basket.");

                    Order order = new()
                    {
                        Id = activeBasket.BasketId,
                        Address = createOrder.Address,
                        Description = createOrder.Description,
                        OrderCode = GenerateOrderCode(),
                        StatusId = (int)OrderStatusEnum.Pending
                    };

                    await _orderWriteRepository.AddAsync(order);

                    // Create immutable order item snapshots for historical order data.
                    IReadOnlyList<CreateOrderItemSnapshotData> snapshotItems =
                        await _basketItemReadRepository.GetOrderItemSnapshotsByBasketIdAsync(activeBasket.BasketId, transactionCancellationToken);

                    List<OrderItemSnapshot> orderItemSnapshots = snapshotItems
                        .Select(item => new OrderItemSnapshot
                        {
                            Id = Guid.NewGuid(),
                            OrderId = order.Id,
                            ProductId = item.ProductId,
                            Name = item.Name,
                            Title = item.Title,
                            Description = item.Description,
                            Rating = item.Rating,
                            UnitPrice = item.UnitPrice,
                            Quantity = item.Quantity,
                            IsProductDeleted = false
                        })
                        .ToList();

                    await _orderItemSnapshotWriteRepository.AddRangeAsync(orderItemSnapshots);

                    // Persist the Order and its item snapshots inside the explicit transaction before reserving stock.
                    // The outer transaction still owns the commit.
                    await _unitOfWork.SaveChangesAsync(transactionCancellationToken);

                    foreach (CreateOrderBasketItemData basketItem in basketItems)
                    {
                        bool stockDecreased = await _productWriteRepository.TryDecreaseStockAsync(
                                basketItem.ProductId,
                                basketItem.Quantity,
                                transactionCancellationToken);

                        if (!stockDecreased)
                            throw new InvalidOperationException($"Product '{basketItem.ProductId}' is unavailable or does not have sufficient stock.");
                    }

                    await ApplyOrderStatusAsync(
                        order.Id,
                        order.OrderCode,
                        OrderStatusEnum.Pending,
                        OrderStatusEnum.Pending,
                        activeBasket.Recipient,
                        activeBasket.FirstName,
                        transactionCancellationToken);
                },
                cancellationToken);
        }

        public async Task UpdateOrderStatusAsync(string orderId, OrderStatusEnum newStatus, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(orderId, out Guid orderGuid))
                throw new ArgumentException("Order id is invalid.", nameof(orderId));

            if (!Enum.IsDefined(newStatus))
                throw new ArgumentOutOfRangeException(nameof(newStatus), newStatus, "Order status is invalid.");

            await _unitOfWork.ExecuteInTransactionAsync(
                async transactionCancellationToken =>
                {
                    OrderStatusUpdateData? orderStatusUpdateData = await _orderReadRepository
                        .GetOrderStatusUpdateDetailsAsync(orderGuid, transactionCancellationToken);

                    if (orderStatusUpdateData is null)
                        throw new KeyNotFoundException($"Order with id '{orderId}' was not found.");

                    if (string.IsNullOrWhiteSpace(orderStatusUpdateData.Recipient))
                        throw new InvalidOperationException("The order customer does not have a valid email address.");

                    OrderStatusEnum currentStatus = (OrderStatusEnum)orderStatusUpdateData.StatusId;

                    if (newStatus == currentStatus)
                        throw new InvalidOperationException($"Order is already in {currentStatus} status.");

                    bool statusUpdated = await _orderWriteRepository.TryUpdateStatusAsync(
                        orderStatusUpdateData.OrderId,
                        currentStatus,
                        newStatus,
                        transactionCancellationToken);

                    if (!statusUpdated)
                        throw new InvalidOperationException("Order status was changed by another operation. Refresh and try again.");

                    await ApplyOrderStatusAsync(
                        orderStatusUpdateData.OrderId,
                        orderStatusUpdateData.OrderCode,
                        currentStatus,
                        newStatus,
                        orderStatusUpdateData.Recipient,
                        orderStatusUpdateData.FirstName,
                        transactionCancellationToken);
                },
                cancellationToken);
        }

        public async Task DeleteOrderAsync(string id, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Order id is required.", nameof(id));

            if (!Guid.TryParse(id, out Guid orderId))
                throw new ArgumentException("Order id is invalid.", nameof(id));

            await DeleteOrderAggregateAsync([orderId], cancellationToken);
        }

        public async Task DeleteRangeOrderAsync(IEnumerable<string> ids, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(ids);

            List<string> orderIds = ids.ToList();

            if (orderIds.Count == 0)
                throw new ArgumentException("At least one order id is required.", nameof(ids));

            List<Guid> parsedOrderIds = new(orderIds.Count);

            foreach (string id in orderIds)
            {
                if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out Guid orderId))
                    throw new ArgumentException($"Order id '{id}' is invalid.", nameof(ids));

                parsedOrderIds.Add(orderId);
            }

            Guid[] distinctOrderIds = parsedOrderIds.Distinct().ToArray();

            await DeleteOrderAggregateAsync(distinctOrderIds, cancellationToken);
        }

        #region Helpers - Methods
        private async Task ApplyOrderStatusAsync(Guid orderId, string orderCode, OrderStatusEnum currentStatus, OrderStatusEnum newStatus, string recipient, string firstName, CancellationToken cancellationToken)
        {
            if (!Enum.IsDefined(currentStatus))
                throw new InvalidOperationException($"Order '{orderId}' has an invalid current status.");

            if (!IsValidStatusTransition(currentStatus, newStatus))
                throw new InvalidOperationException($"Order status cannot transition from {currentStatus} to {newStatus}.");

            DateTime changedDate = DateTime.UtcNow;

            OrderStatusHistory orderStatusHistory = new()
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                PreviousStatusId = (int)currentStatus,
                NewStatusId = (int)newStatus,
                ChangedDate = changedDate
            };

            await _orderStatusHistoryWriteRepository.AddAsync(orderStatusHistory);

            OrderStatusUpdateMailMessage mailMessage = new()
            {
                Recipient = recipient,
                FirstName = firstName,
                OrderCode = orderCode,
                NewStatus = newStatus,
                StatusChangedDate = changedDate
            };

            // Queue the order status email for background processing.
            await _outboxWriter.EnqueueAsync(
                OutboxMessageTypes.OrderStatusUpdateMail,
                mailMessage,
                $"{OutboxMessageTypes.OrderStatusUpdateMail}:{orderStatusHistory.Id}",
                expiresAt: null,
                cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private static bool IsValidStatusTransition(OrderStatusEnum current, OrderStatusEnum next)
            => current switch
            {
                OrderStatusEnum.Pending =>
                    next is OrderStatusEnum.Pending
                        or OrderStatusEnum.Approved
                        or OrderStatusEnum.Cancelled,

                OrderStatusEnum.Approved =>
                    next == OrderStatusEnum.Shipping,

                OrderStatusEnum.Shipping =>
                    next == OrderStatusEnum.Delivered,

                OrderStatusEnum.Delivered => false,
                OrderStatusEnum.Cancelled => false,

                _ => false
            };

        private static string GenerateOrderCode()
        {
            Span<byte> buffer = stackalloc byte[8];
            RandomNumberGenerator.Fill(buffer);

            long randomNumber = BitConverter.ToInt64(buffer);
            long positive10Digit =
                Math.Abs(randomNumber % 9_000_000_000L) + 1_000_000_000L;

            return $"ORD_{positive10Digit}_{DateTime.UtcNow:yyyyMMddHHmm}";
        }

        private async Task DeleteOrderAggregateAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken cancellationToken)
        {
            await _unitOfWork.ExecuteInTransactionAsync(
                async transactionCancellationToken =>
                {
                    await _orderStatusHistoryWriteRepository.ExecuteDeleteAsync(
                        history => orderIds.Contains(history.OrderId),
                        transactionCancellationToken);

                    await _basketItemWriteRepository.ExecuteDeleteAsync(
                        basketItem => orderIds.Contains(basketItem.BasketId),
                        transactionCancellationToken);

                    int deletedOrderCount = await _orderWriteRepository.ExecuteDeleteAsync(
                        order => orderIds.Contains(order.Id),
                        transactionCancellationToken);

                    if (deletedOrderCount != orderIds.Count)
                        throw new KeyNotFoundException("One or more orders were not found.");

                    int deletedBasketCount = await _basketWriteRepository.ExecuteDeleteAsync(
                        basket => orderIds.Contains(basket.Id),
                        transactionCancellationToken);

                    if (deletedBasketCount != orderIds.Count)
                        throw new InvalidOperationException("One or more order baskets could not be deleted.");
                },
                cancellationToken);
        }
        #endregion
    }
}
