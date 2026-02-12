using AutoMapper;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Tenant;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Domain.Entities;
using PoultryDistributionSystem.Domain.Interfaces;

namespace PoultryDistributionSystem.Application.Services;

/// <summary>
/// Tenant service implementation
/// </summary>
public class TenantService : ITenantService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TenantService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<TenantDto> CreateTenantAsync(CreateTenantDto dto, CancellationToken cancellationToken = default)
    {
        // Check if subdomain already exists
        var existing = await _unitOfWork.Tenants.FindAsync(
            t => t.Subdomain == dto.Subdomain && !t.IsDeleted,
            cancellationToken);
        if (existing.Any())
        {
            throw new InvalidOperationException($"Subdomain '{dto.Subdomain}' is already taken");
        }

        var tenant = new Tenant
        {
            Name = dto.Name,
            Subdomain = dto.Subdomain,
            Domain = dto.Domain,
            SubscriptionPlan = dto.SubscriptionPlan,
            IsActive = true,
            MaxUsers = dto.SubscriptionPlan == "Enterprise" ? 100 : dto.SubscriptionPlan == "Professional" ? 50 : 10,
            MaxShops = dto.SubscriptionPlan == "Enterprise" ? 50 : dto.SubscriptionPlan == "Professional" ? 20 : 5,
            MaxFarms = dto.SubscriptionPlan == "Enterprise" ? 20 : dto.SubscriptionPlan == "Professional" ? 10 : 3
        };

        await _unitOfWork.Tenants.AddAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TenantDto>(tenant);
    }

    public async Task<TenantDto> GetTenantByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(id, cancellationToken);
        if (tenant == null)
        {
            throw new KeyNotFoundException($"Tenant with ID {id} not found");
        }

        return _mapper.Map<TenantDto>(tenant);
    }

    public async Task<TenantDto> GetTenantBySubdomainAsync(string subdomain, CancellationToken cancellationToken = default)
    {
        var tenants = await _unitOfWork.Tenants.FindAsync(
            t => t.Subdomain == subdomain && !t.IsDeleted,
            cancellationToken);
        var tenant = tenants.FirstOrDefault();

        if (tenant == null)
        {
            throw new KeyNotFoundException($"Tenant with subdomain '{subdomain}' not found");
        }

        return _mapper.Map<TenantDto>(tenant);
    }

    public async Task<PagedResult<TenantDto>> GetAllTenantsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allTenants = await _unitOfWork.Tenants.FindAsync(t => !t.IsDeleted, cancellationToken);
        var tenantsList = allTenants.OrderBy(t => t.Name).ToList();
        var totalCount = tenantsList.Count;
        var pagedTenants = tenantsList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<TenantDto>
        {
            Items = _mapper.Map<List<TenantDto>>(pagedTenants),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<TenantDto> UpdateTenantAsync(Guid id, CreateTenantDto dto, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(id, cancellationToken);
        if (tenant == null)
        {
            throw new KeyNotFoundException($"Tenant with ID {id} not found");
        }

        tenant.Name = dto.Name;
        tenant.Domain = dto.Domain;
        tenant.SubscriptionPlan = dto.SubscriptionPlan;
        tenant.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Tenants.UpdateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TenantDto>(tenant);
    }

    public async Task<bool> DeleteTenantAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(id, cancellationToken);
        if (tenant == null)
        {
            return false;
        }

        await _unitOfWork.Tenants.DeleteAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
