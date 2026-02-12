using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Distribution;

namespace PoultryDistributionSystem.Application.Interfaces;

/// <summary>
/// Distribution service interface
/// </summary>
public interface IDistributionService
{
    Task<DistributionDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<DistributionDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<DistributionDto> CreateAsync(CreateDistributionDto dto, Guid createdBy, CancellationToken cancellationToken = default);
    Task<DistributionDto> UpdateStatusAsync(Guid id, Domain.Enums.DistributionStatus status, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
