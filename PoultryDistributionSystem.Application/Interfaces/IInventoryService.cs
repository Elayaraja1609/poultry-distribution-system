using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Inventory;

namespace PoultryDistributionSystem.Application.Interfaces;

/// <summary>
/// Inventory service interface
/// </summary>
public interface IInventoryService
{
    Task<StockMovementDto> RecordStockMovementAsync(CreateStockMovementDto dto, Guid createdBy, CancellationToken cancellationToken = default);
    Task<FarmInventoryDto> GetFarmInventoryAsync(Guid farmId, CancellationToken cancellationToken = default);
    Task<PagedResult<StockMovementDto>> GetStockMovementsAsync(Guid? farmId, Guid? chickenId, DateTime? startDate, DateTime? endDate, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CalculateAvailableStockAsync(Guid farmId, Guid chickenId, CancellationToken cancellationToken = default);
    Task<StockSummaryDto> GetStockSummaryAsync(Guid farmId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default);
}

/// <summary>
/// Stock summary DTO
/// </summary>
public class StockSummaryDto
{
    public int TotalIn { get; set; }
    public int TotalOut { get; set; }
    public int TotalLoss { get; set; }
    public int TotalAdjustments { get; set; }
    public int NetChange { get; set; }
}
