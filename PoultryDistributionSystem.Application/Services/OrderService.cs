using AutoMapper;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Order;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Domain.Entities;
using PoultryDistributionSystem.Domain.Enums;
using PoultryDistributionSystem.Domain.Interfaces;

namespace PoultryDistributionSystem.Application.Services;

/// <summary>
/// Order service implementation
/// </summary>
public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, Guid createdBy, CancellationToken cancellationToken = default)
    {
        // Get user to find their shop
        var user = await _unitOfWork.Users.GetByIdAsync(createdBy, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {createdBy} not found");
        }

        // Find shop by user's email
        var shops = await _unitOfWork.Shops.FindAsync(s => s.Email == user.Email && !s.IsDeleted, cancellationToken);
        var shop = shops.FirstOrDefault();
        if (shop == null)
        {
            throw new InvalidOperationException("User is not associated with a shop");
        }

        var order = new Order
        {
            ShopId = shop.Id,
            OrderDate = DateTime.UtcNow,
            RequestedDeliveryDate = dto.RequestedDeliveryDate,
            Status = OrderStatus.Pending,
            FulfillmentStatus = FulfillmentStatus.None,
            CreatedBy = createdBy
        };

        decimal totalAmount = 0;
        foreach (var itemDto in dto.Items)
        {
            var chicken = await _unitOfWork.Chickens.GetByIdAsync(itemDto.ChickenId, cancellationToken);
            if (chicken == null)
            {
                throw new KeyNotFoundException($"Chicken with ID {itemDto.ChickenId} not found");
            }

            if (chicken.Status != ChickenStatus.ReadyForDistribution)
            {
                throw new InvalidOperationException($"Chicken batch {chicken.BatchNumber} is not ready for distribution");
            }

            // Calculate unit price (you may want to get this from a pricing service)
            var unitPrice = 10.0m; // Default price, should come from chicken or pricing table
            var itemTotal = unitPrice * itemDto.Quantity;
            totalAmount += itemTotal;

            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                ChickenId = itemDto.ChickenId,
                RequestedQuantity = itemDto.Quantity,
                FulfilledQuantity = 0,
                UnitPrice = unitPrice,
                TotalPrice = itemTotal,
                CreatedBy = createdBy
            };

            order.OrderItems.Add(orderItem);
        }

        order.TotalAmount = totalAmount;

        await _unitOfWork.Orders.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetOrderByIdAsync(order.Id, cancellationToken);
    }

    public async Task<PagedResult<OrderDto>> GetOrdersAsync(Guid? shopId, string? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allOrders = await _unitOfWork.Orders.FindAsync(o => !o.IsDeleted, cancellationToken);

        if (shopId.HasValue)
        {
            allOrders = allOrders.Where(o => o.ShopId == shopId.Value);
        }

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<OrderStatus>(status, out var statusEnum))
        {
            allOrders = allOrders.Where(o => o.Status == statusEnum);
        }

        var ordersList = allOrders.OrderByDescending(o => o.OrderDate).ToList();
        var totalCount = ordersList.Count;
        var pagedOrders = ordersList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = new List<OrderDto>();
        foreach (var order in pagedOrders)
        {
            var shop = await _unitOfWork.Shops.GetByIdAsync(order.ShopId, cancellationToken);
            var dto = _mapper.Map<OrderDto>(order);
            dto.ShopName = shop?.Name ?? string.Empty;

            // Map order items
            foreach (var item in order.OrderItems)
            {
                var chicken = await _unitOfWork.Chickens.GetByIdAsync(item.ChickenId, cancellationToken);
                var itemDto = _mapper.Map<OrderItemDto>(item);
                itemDto.BatchNumber = chicken?.BatchNumber ?? string.Empty;
                dto.Items.Add(itemDto);
            }

            items.Add(dto);
        }

        return new PagedResult<OrderDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<OrderDto> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {id} not found");
        }

        var shop = await _unitOfWork.Shops.GetByIdAsync(order.ShopId, cancellationToken);
        var dto = _mapper.Map<OrderDto>(order);
        dto.ShopName = shop?.Name ?? string.Empty;

        // Map order items
        foreach (var item in order.OrderItems)
        {
            var chicken = await _unitOfWork.Chickens.GetByIdAsync(item.ChickenId, cancellationToken);
            var itemDto = _mapper.Map<OrderItemDto>(item);
            itemDto.BatchNumber = chicken?.BatchNumber ?? string.Empty;
            dto.Items.Add(itemDto);
        }

        return dto;
    }

    public async Task<OrderDto> ApproveOrderAsync(Guid id, Guid approvedBy, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {id} not found");
        }

        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException($"Order cannot be approved. Current status: {order.Status}");
        }

        order.Status = OrderStatus.Approved;
        order.ApprovedBy = approvedBy;
        order.ApprovedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send notification to shop owner
        try
        {
            await _notificationService.SendOrderStatusNotificationAsync(id, order.CreatedBy ?? Guid.Empty, OrderStatus.Approved, null, cancellationToken);
        }
        catch
        {
            // Log error but don't fail the operation
        }

        return await GetOrderByIdAsync(id, cancellationToken);
    }

    public async Task<OrderDto> RejectOrderAsync(Guid id, string reason, Guid rejectedBy, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {id} not found");
        }

        if (order.Status != OrderStatus.Pending)
        {
            throw new InvalidOperationException($"Order cannot be rejected. Current status: {order.Status}");
        }

        order.Status = OrderStatus.Rejected;
        order.RejectedReason = reason;
        order.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send notification to shop owner
        try
        {
            await _notificationService.SendOrderStatusNotificationAsync(id, order.CreatedBy ?? Guid.Empty, OrderStatus.Rejected, reason, cancellationToken);
        }
        catch
        {
            // Log error but don't fail the operation
        }

        return await GetOrderByIdAsync(id, cancellationToken);
    }

    public async Task<OrderDto> UpdateFulfillmentAsync(Guid id, UpdateFulfillmentDto dto, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {id} not found");
        }

        if (order.Status != OrderStatus.Approved && order.Status != OrderStatus.Processing)
        {
            throw new InvalidOperationException($"Order fulfillment can only be updated for approved or processing orders");
        }

        bool allFulfilled = true;
        bool anyFulfilled = false;

        foreach (var itemDto in dto.Items)
        {
            var orderItem = order.OrderItems.FirstOrDefault(oi => oi.Id == itemDto.OrderItemId);
            if (orderItem == null)
            {
                throw new KeyNotFoundException($"Order item with ID {itemDto.OrderItemId} not found");
            }

            if (itemDto.FulfilledQuantity > orderItem.RequestedQuantity)
            {
                throw new InvalidOperationException($"Fulfilled quantity cannot exceed requested quantity");
            }

            orderItem.FulfilledQuantity = itemDto.FulfilledQuantity;
            orderItem.TotalPrice = orderItem.UnitPrice * itemDto.FulfilledQuantity;
            orderItem.UpdatedAt = DateTime.UtcNow;

            if (orderItem.FulfilledQuantity > 0)
            {
                anyFulfilled = true;
            }

            if (orderItem.FulfilledQuantity < orderItem.RequestedQuantity)
            {
                allFulfilled = false;
            }
        }

        // Update order status and fulfillment status
        if (allFulfilled)
        {
            order.Status = OrderStatus.Fulfilled;
            order.FulfillmentStatus = FulfillmentStatus.Complete;
        }
        else if (anyFulfilled)
        {
            order.Status = OrderStatus.PartiallyFulfilled;
            order.FulfillmentStatus = FulfillmentStatus.Partial;
        }
        else
        {
            order.Status = OrderStatus.Processing;
            order.FulfillmentStatus = FulfillmentStatus.None;
        }

        order.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send notification if order is fulfilled
        if (order.Status == OrderStatus.Fulfilled)
        {
            try
            {
                await _notificationService.SendOrderStatusNotificationAsync(id, order.CreatedBy ?? Guid.Empty, OrderStatus.Fulfilled, null, cancellationToken);
            }
            catch
            {
                // Log error but don't fail the operation
            }
        }

        return await GetOrderByIdAsync(id, cancellationToken);
    }

    public async Task<OrderDto> CancelOrderAsync(Guid id, Guid cancelledBy, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(id, cancellationToken);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {id} not found");
        }

        if (order.Status == OrderStatus.Fulfilled || order.Status == OrderStatus.Cancelled)
        {
            throw new InvalidOperationException($"Order cannot be cancelled. Current status: {order.Status}");
        }

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetOrderByIdAsync(id, cancellationToken);
    }

    public async Task<PagedResult<OrderDto>> GetMyOrdersAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        // Get user to find their shop
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        // Find shop by user's email
        var shops = await _unitOfWork.Shops.FindAsync(s => s.Email == user.Email && !s.IsDeleted, cancellationToken);
        var shop = shops.FirstOrDefault();
        if (shop == null)
        {
            return new PagedResult<OrderDto>
            {
                Items = new List<OrderDto>(),
                TotalCount = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        return await GetOrdersAsync(shop.Id, null, pageNumber, pageSize, cancellationToken);
    }
}
