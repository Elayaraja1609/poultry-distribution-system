using AutoMapper;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Shop;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Domain.Interfaces;

namespace PoultryDistributionSystem.Application.Services;

/// <summary>
/// Shop service implementation
/// </summary>
public class ShopService : IShopService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ShopService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<ShopDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var shop = await _unitOfWork.Shops.GetByIdAsync(id, cancellationToken);
        if (shop == null)
        {
            throw new KeyNotFoundException($"Shop with ID {id} not found");
        }

        return _mapper.Map<ShopDto>(shop);
    }

    public async Task<PagedResult<ShopDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allShops = await _unitOfWork.Shops.FindAsync(s => !s.IsDeleted, cancellationToken);
        var shopsList = allShops.OrderBy(s => s.Name).ToList();

        var totalCount = shopsList.Count;
        var pagedShops = shopsList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<ShopDto>
        {
            Items = _mapper.Map<List<ShopDto>>(pagedShops),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<ShopDto> CreateAsync(CreateShopDto dto, Guid createdBy, CancellationToken cancellationToken = default)
    {
        var shop = _mapper.Map<Domain.Entities.Shop>(dto);
        shop.CreatedBy = createdBy;

        await _unitOfWork.Shops.AddAsync(shop, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ShopDto>(shop);
    }

    public async Task<ShopDto> UpdateAsync(Guid id, CreateShopDto dto, CancellationToken cancellationToken = default)
    {
        var shop = await _unitOfWork.Shops.GetByIdAsync(id, cancellationToken);
        if (shop == null)
        {
            throw new KeyNotFoundException($"Shop with ID {id} not found");
        }

        shop.Name = dto.Name;
        shop.OwnerName = dto.OwnerName;
        shop.Phone = dto.Phone;
        shop.Email = dto.Email;
        shop.Address = dto.Address;
        shop.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Shops.UpdateAsync(shop, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ShopDto>(shop);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var shop = await _unitOfWork.Shops.GetByIdAsync(id, cancellationToken);
        if (shop == null)
        {
            return false;
        }

        await _unitOfWork.Shops.DeleteAsync(shop, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
