using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Order;

namespace PoultryDistributionSystem.Application.Interfaces;

/// <summary>
/// Order service interface
/// </summary>
public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto, Guid createdBy, CancellationToken cancellationToken = default);
    Task<PagedResult<OrderDto>> GetOrdersAsync(Guid? shopId, string? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<OrderDto>> GetMyOrdersAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<OrderDto> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrderDto> ApproveOrderAsync(Guid id, Guid approvedBy, CancellationToken cancellationToken = default);
    Task<OrderDto> RejectOrderAsync(Guid id, string reason, Guid rejectedBy, CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateFulfillmentAsync(Guid id, UpdateFulfillmentDto dto, CancellationToken cancellationToken = default);
    Task<OrderDto> CancelOrderAsync(Guid id, Guid cancelledBy, CancellationToken cancellationToken = default);
}
