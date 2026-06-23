using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using WebAppAPI.Application.Abstractions.Services;
using WebAppAPI.Application.DTOs;
using WebAppAPI.Application.DTOs.Order;
using WebAppAPI.Application.Options.Storage;
using WebAppAPI.Application.Repositories;
using WebAppAPI.Domain.Entities;
using WebAppAPI.Domain.Enums;

namespace WebAppAPI.Persistence.Services
{
    public class OrderService(
        IWriteRepository<Order> orderWriteRepository,
        IOrderReadRepository orderReadRepository,
        IOrderStatusHistoryReadRepository orderStatusHistoryReadRepository,
        IWriteRepository<OrderStatusHistory> orderStatusHistoryWriteRepository,
        IBasketReadRepository basketReadRepository,
        IWriteRepository<Basket> basketWriteRepository,
        IBasketItemReadRepository basketItemReadRepository,
        IWriteRepository<BasketItem> basketItemWriteRepository,
        IMailService mailService,
        IOptions<BaseStorageOptions> baseStorageOptions,
        IBasketService basketService,
        IUnitOfWork unitOfWork) : IOrderService
    {
        private readonly IWriteRepository<Order> _orderWriteRepository = orderWriteRepository;
        private readonly IOrderReadRepository _orderReadRepository = orderReadRepository;
        private readonly IOrderStatusHistoryReadRepository _orderStatusHistoryReadRepository = orderStatusHistoryReadRepository;
        private readonly IWriteRepository<OrderStatusHistory> _orderStatusHistoryWriteRepository = orderStatusHistoryWriteRepository;
        private readonly IBasketReadRepository _basketReadRepository = basketReadRepository;
        private readonly IWriteRepository<Basket> _basketWriteRepository = basketWriteRepository;
        private readonly IBasketItemReadRepository _basketItemReadRepository = basketItemReadRepository;
        private readonly IWriteRepository<BasketItem> _basketItemWriteRepository = basketItemWriteRepository;
        private readonly IMailService _mailService = mailService;
        private readonly BaseStorageOptions _baseStorageOptions = baseStorageOptions.Value;
        private readonly IBasketService _basketService = basketService;
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<string> CreateOrderFromActiveBasketAsync(CreateOrder createOrder)
        {
            var basket = await _basketService.GetUserActiveBasketAsync(createIfNotExists: false)
                ?? throw new Exception("Active basket not found.");

            await EnsureBasketHasItemsAsync(basket.Id);

            var order = new Order
            {
                Id = basket.Id,
                Address = createOrder.Address,
                Description = createOrder.Description,
                OrderCode = GenerateOrderCode(),
                StatusId = (int)OrderStatusEnum.Pending
            };

            await _orderWriteRepository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            string orderId = order.Id.ToString();
            await UpdateOrderStatusAsync(orderId, OrderStatusEnum.Pending);

            return orderId;
        }

        public async Task<ListOrder> GetAllOrdersAsync(int page, int size)
        {
            (List<Order> orders, int totalCount) = await _orderReadRepository.GetPagedWithBasketSummaryAsync(page, size);

            return new()
            {
                TotalOrderCount = totalCount,
                Orders = orders.Select(o => new
                {
                    Id = o.Id.ToString(),
                    OrderCode = o.OrderCode,
                    CustomerName = $"{o.Basket.User.FirstName} {o.Basket.User.LastName}",
                    TotalPrice = o.Basket.BasketItems.Sum(item => item.Product.Price * item.Quantity),
                    DateCreated = o.DateCreated,
                    StatusId = o.StatusId
                }).ToList()
            };
        }

        public async Task<OrderDetail> GetOrderByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out var orderGuid))
                throw new Exception("Invalid order id.");

            Order? order = await _orderReadRepository.GetDetailByIdAsync(orderGuid);

            if (order == null)
                throw new Exception("An Order with the specified ID could not be found.");

            var orderDetail = new OrderDetail()
            {
                Id = order.Id.ToString(),
                OrderCode = order.OrderCode,
                Description = order.Description,
                Address = order.Address,
                DateCreated = order.DateCreated,
                StatusId = order.StatusId,
                OrderBasketItems = order.Basket.BasketItems.Select(bi => new OrderItems()
                {
                    Name = bi.Product.Name,
                    Description = bi.Product.Description,
                    Price = bi.Product.Price,
                    Quantity = bi.Quantity,
                    Rating = bi.Product.Rating,
                    OrderProductImageFile = bi.Product.ProductImageFiles.Where(pif => pif.CoverImage == true).Select(pif => new BasketProductImageFile
                    {
                        ProductImageFileId = pif.Id.ToString(),
                        FileName = pif.FileName,
                        Path = $"{_baseStorageOptions.Url}/{pif.Path}"
                    }).FirstOrDefault()
                }).ToList()
            };

            return orderDetail;
        }

        public async Task UpdateOrderStatusAsync(string orderId, OrderStatusEnum newStatus)
        {
            if (!Guid.TryParse(orderId, out Guid orderGuid))
                throw new Exception("Invalid order id.");

            Order? order = await _orderReadRepository.GetByIdAsync(orderGuid, tracking: true);
            if (order == null)
                throw new Exception("An Order with the specified ID could not be found.");

            try
            {
                var currentStatus = (OrderStatusEnum)order.StatusId;

                if (!IsValidStatusTransition(currentStatus, newStatus))
                    throw new InvalidOperationException($"Order status can't transition from {currentStatus} to {newStatus}");

                // Save the status history before applying the order status update.
                var orderStatusHistory = new OrderStatusHistory
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    PreviousStatusId = (int)currentStatus,
                    NewStatusId = (int)newStatus,
                    ChangedDate = DateTime.UtcNow
                };
                await _orderStatusHistoryWriteRepository.AddAsync(orderStatusHistory);
                var historySaveResult = await _unitOfWork.SaveChangesAsync();

                // Continue only if the history record was persisted successfully.
                if (historySaveResult > 0)
                {
                    if (newStatus != currentStatus)
                    {
                        order.StatusId = (int)newStatus;
                        _orderWriteRepository.Update(order);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    UpdateOrderStatusMailDto mailData = await CreateOrderStatusMailObject(order.Id, newStatus, orderStatusHistory.ChangedDate);
                    await _mailService.SendOrderStatusUpdateMailAsync(mailData.Recipient, mailData.OrderCode, mailData.NewStatus, mailData.StatusChangedDate, mailData.FirstName);
                }
                else
                {
                    throw new Exception("Order status history could not be saved.");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<OrderStatusHistoryDto> GetOrderStatusHistoryByIdAsync(string orderId)
        {
            if (!Guid.TryParse(orderId, out var orderGuid))
                throw new Exception("Invalid order id.");

            Order? order = await _orderReadRepository.GetByIdAsync(orderGuid, tracking: false);
            if (order == null)
                throw new Exception("An Order with the specified ID could not be found.");

            List<OrderStatusHistory> statusHistoryList = await _orderStatusHistoryReadRepository.GetByOrderIdAsync(orderGuid);

            return new OrderStatusHistoryDto
            {
                CurrentStatusId = order.StatusId,
                History = statusHistoryList.Select(sh => new StatusChangeEntry
                {
                    NewStatusId = sh.NewStatusId,
                    ChangedDate = sh.ChangedDate
                }).ToList()
            };
        }

        public async Task DeleteOrderAsync(string id)
        {
            if (!Guid.TryParse(id, out var orderId))
                throw new Exception("Invalid order id.");

            await DeleteOrderAggregateAsync(orderId);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteRangeOrderAsync(IEnumerable<string> ids)
        {
            foreach (var id in ids)
            {
                if (!Guid.TryParse(id, out var orderId))
                    throw new Exception("Invalid order id.");

                await DeleteOrderAggregateAsync(orderId);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        #region Helpers - Methods
        private async Task EnsureBasketHasItemsAsync(Guid basketId)
        {
            bool hasBasketItems = await _basketItemReadRepository.AnyByBasketIdAsync(basketId);

            if (!hasBasketItems)
                throw new Exception("Cannot create an order from an empty basket.");
        }

        private async Task DeleteOrderAggregateAsync(Guid orderId)
        {
            Order? order = await _orderReadRepository.GetByIdAsync(orderId, tracking: true);
            if (order == null)
                throw new Exception("An Order with the specified ID could not be found.");

            Basket? basket = await _basketReadRepository.GetByIdAsync(orderId, tracking: true);
            if (basket == null)
                throw new Exception("A Basket with the specified ID could not be found.");

            List<OrderStatusHistory> statusHistories = await _orderStatusHistoryReadRepository.GetByOrderIdAsync(orderId, tracking: true);

            if (statusHistories.Count > 0)
                _orderStatusHistoryWriteRepository.RemoveRange(statusHistories);

            List<BasketItem> basketItems = await _basketItemReadRepository.GetByBasketIdAsync(orderId, tracking: true);
            if (basketItems.Count > 0)
                _basketItemWriteRepository.RemoveRange(basketItems);

            _orderWriteRepository.Remove(order);
            _basketWriteRepository.Remove(basket);
        }

        private string GenerateOrderCode()
        {
            Span<byte> buffer = stackalloc byte[8];
            RandomNumberGenerator.Fill(buffer);
            long randomNumber = BitConverter.ToInt64(buffer);

            long positive10Digit = Math.Abs(randomNumber % 9_000_000_000L) + 1_000_000_000L;
            string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmm");

            return string.Create(
                4 + 10 + 1 + 12, // "ORD_" + 10 digits + "_" + timestamp (yyyyMMddHHmm)
                (positive10Digit, timestamp),
                static (span, state) =>
                {
                    var (number, ts) = state;

                    // "ORD_"
                    "ORD_".AsSpan().CopyTo(span.Slice(0, 4));

                    // 10 digits number
                    number.TryFormat(span.Slice(4, 10), out _);

                    // "_"
                    span[14] = '_';

                    // timestamp
                    ts.AsSpan().CopyTo(span.Slice(15));
                }
            );
        }

        private bool IsValidStatusTransition(OrderStatusEnum current, OrderStatusEnum next)
        {
            var validTransitions = new Dictionary<OrderStatusEnum, List<OrderStatusEnum>>
            {
                { OrderStatusEnum.Pending, new() { OrderStatusEnum.Pending, OrderStatusEnum.Approved, OrderStatusEnum.Cancelled } },
                { OrderStatusEnum.Approved, new() { OrderStatusEnum.Shipping } },
                { OrderStatusEnum.Shipping, new() { OrderStatusEnum.Delivered } },
                { OrderStatusEnum.Delivered, new() },
                { OrderStatusEnum.Cancelled, new() }
            };
            return validTransitions.TryGetValue(current, out var nextStates) && nextStates.Contains(next);
        }

        private async Task<UpdateOrderStatusMailDto> CreateOrderStatusMailObject(Guid orderId, OrderStatusEnum newStatus, DateTime changedDate)
        {
            Order? orderData = await _orderReadRepository.GetWithBasketUserAsync(orderId);

            if (orderData?.Basket?.User == null)
                throw new Exception("Order's Basket or User data could not be loaded.");

            UpdateOrderStatusMailDto mailData = new()
            {
                Recipient = orderData.Basket.User.Email,
                FirstName = orderData.Basket.User.FirstName,
                OrderCode = orderData.OrderCode,
                NewStatus = newStatus,
                StatusChangedDate = changedDate
            };
            return mailData;
        }
        #endregion
    }
}
