using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Delivery;

namespace PoultryDistributionSystem.Application.Interfaces;

/// <summary>
/// Delivery service interface
/// </summary>
public interface IDeliveryService
{
    Task<DeliveryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<DeliveryDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<DeliveryDto>> GetByShopIdAsync(Guid shopId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<DeliveryDto>> GetMyDeliveriesAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<DeliveryDto> UpdateAsync(Guid id, UpdateDeliveryDto dto, CancellationToken cancellationToken = default);
}
