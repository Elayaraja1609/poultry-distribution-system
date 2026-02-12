using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Tenant;

namespace PoultryDistributionSystem.Application.Interfaces;

/// <summary>
/// Tenant service interface
/// </summary>
public interface ITenantService
{
    Task<TenantDto> CreateTenantAsync(CreateTenantDto dto, CancellationToken cancellationToken = default);
    Task<TenantDto> GetTenantByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TenantDto> GetTenantBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default);
    Task<PagedResult<TenantDto>> GetAllTenantsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<TenantDto> UpdateTenantAsync(Guid id, CreateTenantDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteTenantAsync(Guid id, CancellationToken cancellationToken = default);
}
