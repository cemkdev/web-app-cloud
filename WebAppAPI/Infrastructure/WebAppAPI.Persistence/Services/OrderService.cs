using Microsoft.EntityFrameworkCore;
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
    public class OrderService : IOrderService
    {
        readonly IOrderWriteRepository _orderWriteRepository;
        readonly IOrderReadRepository _orderReadRepository;
        readonly IOrderStatusHistoryReadRepository _orderStatusHistoryReadRepository;
        readonly IOrderStatusHistoryWriteRepository _orderStatusHistoryWriteRepository;
        readonly IBasketReadRepository _basketReadRepository;
        readonly IBasketWriteRepository _basketWriteRepository;
        readonly IBasketItemReadRepository _basketItemReadRepository;
        readonly IBasketItemWriteRepository _basketItemWriteRepository;
        readonly IMailService _mailService;
        readonly BaseStorageOptions _baseStorageOptions;

        public OrderService(
            IOrderWriteRepository orderWriteRepository,
            IOrderReadRepository orderReadRepository,
            IOrderStatusHistoryReadRepository orderStatusHistoryReadRepository,
            IOrderStatusHistoryWriteRepository orderStatusHistoryWriteRepository,
            IBasketReadRepository basketReadRepository,
            IBasketWriteRepository basketWriteRepository,
            IBasketItemReadRepository basketItemReadRepository,
            IBasketItemWriteRepository basketItemWriteRepository,
            IMailService mailService,
            IOptions<BaseStorageOptions> baseStorageOptions)
        {
            _orderWriteRepository = orderWriteRepository;
            _orderReadRepository = orderReadRepository;
            _orderStatusHistoryReadRepository = orderStatusHistoryReadRepository;
            _orderStatusHistoryWriteRepository = orderStatusHistoryWriteRepository;
            _basketReadRepository = basketReadRepository;
            _basketWriteRepository = basketWriteRepository;
            _basketItemReadRepository = basketItemReadRepository;
            _basketItemWriteRepository = basketItemWriteRepository;
            _mailService = mailService;
            _baseStorageOptions = baseStorageOptions.Value;
        }

        public async Task<string> CreateOrderAsync(CreateOrder createOrder)
        {
            var order = new Order
            {
                Id = Guid.Parse(createOrder.BasketId),
                Address = createOrder.Address,
                Description = createOrder.Description,
                OrderCode = GenerateOrderCode(),
                StatusId = (int)OrderStatusEnum.Pending
            };
            await _orderWriteRepository.AddAsync(order);
            await _orderWriteRepository.SaveAsync();

            return order.Id.ToString();
        }

        public async Task<ListOrder> GetAllOrdersAsync(int page, int size)
        {
            var query = _orderReadRepository.Table
                            .Include(o => o.Basket)
                                .ThenInclude(b => b.BasketItems)
                            .Include(o => o.Basket.User);

            var dataPerPage = query.OrderBy(o => o.DateCreated).Skip(page * size).Take(size);

            return new()
            {
                TotalOrderCount = await query.CountAsync(),
                Orders = await dataPerPage.Select(o => new
                {
                    Id = o.Id.ToString(),
                    OrderCode = o.OrderCode,
                    CustomerName = $"{o.Basket.User.FirstName} {o.Basket.User.LastName}",
                    TotalPrice = o.Basket.BasketItems.Sum(item => item.Product.Price * item.Quantity),
                    DateCreated = o.DateCreated,
                    StatusId = o.StatusId
                }).ToListAsync()
            };
        }

        public async Task<OrderDetail> GetOrderByIdAsync(string id)
        {
            if (!Guid.TryParse(id, out var orderGuid))
                throw new Exception("Invalid order id.");

            var order = await _orderReadRepository.Table
                                .Include(o => o.Basket)
                                    .ThenInclude(b => b.BasketItems)
                                        .ThenInclude(bi => bi.Product)
                                            .ThenInclude(p => p.ProductImageFiles)
                                .FirstOrDefaultAsync(o => o.Id == orderGuid);

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
            Order order = await _orderReadRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new Exception("An Order with the specified ID could not be found.");

            try
            {
                var currentStatus = (OrderStatusEnum)order.StatusId;

                if (!IsValidStatusTransition(currentStatus, newStatus))
                    throw new InvalidOperationException($"Order status can't transition from {currentStatus} to {newStatus}");

                // Create OrderStatusHistory record
                var orderStatusHistory = new OrderStatusHistory
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    PreviousStatusId = (int)currentStatus,
                    NewStatusId = (int)newStatus,
                    ChangedDate = DateTime.UtcNow
                };
                await _orderStatusHistoryWriteRepository.AddAsync(orderStatusHistory);
                var historySaveResult = await _orderStatusHistoryWriteRepository.SaveAsync();

                if (historySaveResult > 0) // If 'OrderStatusHistory' saving process is success.
                {
                    //// Update Order -> StatusId
                    if (newStatus != currentStatus)
                    {
                        order.StatusId = (int)newStatus;
                        _orderWriteRepository.Update(order);
                        await _orderWriteRepository.SaveAsync();
                    }

                    UpdateOrderStatusMailDto mailData = await CreateOrderStatusMailObject(order.Id, newStatus, orderStatusHistory.ChangedDate);
                    await _mailService.SendOrderStatusUpdateMailAsync(mailData.Recipient, mailData.OrderCode, mailData.NewStatus, mailData.StatusChangedDate, mailData.FirstName);
                }
                else
                {
                    throw new Exception("Order status history could not be saved.");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<OrderStatusHistoryDto> GetOrderStatusHistoryByIdAsync(string orderId)
        {
            if (!Guid.TryParse(orderId, out var orderGuid))
                throw new Exception("Invalid order id.");

            Order order = await _orderReadRepository.GetByIdAsync(orderGuid.ToString());
            if (order == null)
                throw new Exception("An Order with the specified ID could not be found.");

            var statusHistoryList = await _orderStatusHistoryReadRepository
                .GetWhere(os => os.OrderId == orderGuid)
                .ToListAsync();

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
            await _orderWriteRepository.SaveAsync();
        }

        public async Task DeleteRangeOrderAsync(IEnumerable<string> ids)
        {
            foreach (var id in ids)
            {
                if (!Guid.TryParse(id, out var orderId))
                    throw new Exception("Invalid order id.");

                await DeleteOrderAggregateAsync(orderId);
            }
            await _orderWriteRepository.SaveAsync();
        }

        #region Helpers - Methods
        private async Task DeleteOrderAggregateAsync(Guid orderId)
        {
            var order = await _orderReadRepository.GetByIdAsync(orderId.ToString());
            if (order == null)
                throw new Exception("An Order with the specified ID could not be found.");

            var basket = await _basketReadRepository.GetByIdAsync(orderId.ToString());
            if (basket == null)
                throw new Exception("A Basket with the specified ID could not be found.");

            var statusHistories = await _orderStatusHistoryReadRepository
                .GetWhere(sh => sh.OrderId == orderId)
                .ToListAsync();

            if (statusHistories.Count > 0)
                _orderStatusHistoryWriteRepository.RemoveRange(statusHistories);

            var basketItems = await _basketItemReadRepository
                .GetWhere(bi => bi.BasketId == orderId)
                .ToListAsync();
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

            // String Interpolation - (It is enough for this app.)
            //return $"ORD_{positive10Digit}_{timestamp}";

            // string.Create() - more memory-friendly string concatenation.(If necessary...)
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
            var orderData = await _orderReadRepository.Table
                                        .Include(o => o.Basket)
                                            .ThenInclude(b => b.User)
                                        .FirstOrDefaultAsync(o => o.Id == orderId);

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
