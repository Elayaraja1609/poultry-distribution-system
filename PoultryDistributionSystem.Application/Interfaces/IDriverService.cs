using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Driver;

namespace PoultryDistributionSystem.Application.Interfaces;

/// <summary>
/// Driver service interface
/// </summary>
public interface IDriverService
{
    Task<DriverDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<DriverDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<DriverDto> CreateAsync(CreateDriverDto dto, Guid createdBy, CancellationToken cancellationToken = default);
    Task<DriverDto> UpdateAsync(Guid id, CreateDriverDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
