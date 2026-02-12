using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Supplier;

namespace PoultryDistributionSystem.Application.Interfaces;

/// <summary>
/// Supplier service interface
/// </summary>
public interface ISupplierService
{
    Task<SupplierDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<SupplierDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<SupplierDto> CreateAsync(CreateSupplierDto dto, Guid createdBy, CancellationToken cancellationToken = default);
    Task<SupplierDto> UpdateAsync(Guid id, UpdateSupplierDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
