using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Farm;

namespace PoultryDistributionSystem.Application.Interfaces;

/// <summary>
/// Farm service interface
/// </summary>
public interface IFarmService
{
    Task<FarmDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<FarmDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<FarmDto> CreateAsync(CreateFarmDto dto, Guid createdBy, CancellationToken cancellationToken = default);
    Task<FarmDto> UpdateAsync(Guid id, UpdateFarmDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateCapacityAsync(Guid id, int currentCount, CancellationToken cancellationToken = default);
}
