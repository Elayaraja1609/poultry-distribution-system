using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Chicken;

namespace PoultryDistributionSystem.Application.Interfaces;

/// <summary>
/// Chicken service interface
/// </summary>
public interface IChickenService
{
    Task<ChickenDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<ChickenDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<PagedResult<ChickenDto>> GetByFarmIdAsync(Guid farmId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<ChickenDto> CreateAsync(CreateChickenDto dto, Guid createdBy, CancellationToken cancellationToken = default);
    Task<ChickenDto> UpdateAsync(Guid id, UpdateChickenDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
