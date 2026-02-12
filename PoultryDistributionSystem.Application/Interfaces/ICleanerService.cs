using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Cleaner;

namespace PoultryDistributionSystem.Application.Interfaces;

/// <summary>
/// Cleaner service interface
/// </summary>
public interface ICleanerService
{
    Task<CleanerDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<CleanerDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<CleanerDto> CreateAsync(CreateCleanerDto dto, Guid createdBy, CancellationToken cancellationToken = default);
    Task<CleanerDto> UpdateAsync(Guid id, CreateCleanerDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
