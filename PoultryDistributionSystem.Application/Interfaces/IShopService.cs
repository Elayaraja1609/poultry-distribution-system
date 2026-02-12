using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Shop;

namespace PoultryDistributionSystem.Application.Interfaces;

/// <summary>
/// Shop service interface
/// </summary>
public interface IShopService
{
    Task<ShopDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<ShopDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<ShopDto> CreateAsync(CreateShopDto dto, Guid createdBy, CancellationToken cancellationToken = default);
    Task<ShopDto> UpdateAsync(Guid id, CreateShopDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
