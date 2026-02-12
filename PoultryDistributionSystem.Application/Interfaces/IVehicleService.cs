using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Vehicle;

namespace PoultryDistributionSystem.Application.Interfaces;

/// <summary>
/// Vehicle service interface
/// </summary>
public interface IVehicleService
{
    Task<VehicleDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<VehicleDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<VehicleDto> CreateAsync(CreateVehicleDto dto, Guid createdBy, CancellationToken cancellationToken = default);
    Task<VehicleDto> UpdateAsync(Guid id, UpdateVehicleDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
