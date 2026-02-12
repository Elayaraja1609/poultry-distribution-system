using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Sale;

namespace PoultryDistributionSystem.Application.Interfaces;

/// <summary>
/// Sales service interface
/// </summary>
public interface ISalesService
{
    Task<SaleDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<SaleDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<SaleDto>> GetByShopIdAsync(Guid shopId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<SaleDto>> GetMySalesAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<SaleDto> CreateAsync(CreateSaleDto dto, Guid createdBy, CancellationToken cancellationToken = default);
}
