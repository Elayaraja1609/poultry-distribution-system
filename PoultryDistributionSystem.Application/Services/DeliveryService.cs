using AutoMapper;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Delivery;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Domain.Interfaces;

namespace PoultryDistributionSystem.Application.Services;

/// <summary>
/// Delivery service implementation
/// </summary>
public class DeliveryService : IDeliveryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DeliveryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<DeliveryDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var delivery = await _unitOfWork.Deliveries.GetByIdAsync(id, cancellationToken);
        if (delivery == null)
        {
            throw new KeyNotFoundException($"Delivery with ID {id} not found");
        }

        var shop = await _unitOfWork.Shops.GetByIdAsync(delivery.ShopId, cancellationToken);

        var dto = _mapper.Map<DeliveryDto>(delivery);
        dto.ShopName = shop?.Name ?? string.Empty;
        return dto;
    }

    public async Task<PagedResult<DeliveryDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allDeliveries = await _unitOfWork.Deliveries.FindAsync(d => !d.IsDeleted, cancellationToken);
        var deliveriesList = allDeliveries.OrderByDescending(d => d.DeliveryDate).ToList();

        var totalCount = deliveriesList.Count;
        var pagedDeliveries = deliveriesList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = new List<DeliveryDto>();
        foreach (var delivery in pagedDeliveries)
        {
            var shop = await _unitOfWork.Shops.GetByIdAsync(delivery.ShopId, cancellationToken);

            var dto = _mapper.Map<DeliveryDto>(delivery);
            dto.ShopName = shop?.Name ?? string.Empty;
            items.Add(dto);
        }

        return new PagedResult<DeliveryDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<DeliveryDto>> GetByShopIdAsync(Guid shopId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allDeliveries = await _unitOfWork.Deliveries.FindAsync(d => d.ShopId == shopId && !d.IsDeleted, cancellationToken);
        var deliveriesList = allDeliveries.OrderByDescending(d => d.DeliveryDate).ToList();

        var totalCount = deliveriesList.Count;
        var pagedDeliveries = deliveriesList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = new List<DeliveryDto>();
        var shop = await _unitOfWork.Shops.GetByIdAsync(shopId, cancellationToken);

        foreach (var delivery in pagedDeliveries)
        {
            var dto = _mapper.Map<DeliveryDto>(delivery);
            dto.ShopName = shop?.Name ?? string.Empty;
            items.Add(dto);
        }

        return new PagedResult<DeliveryDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<DeliveryDto> UpdateAsync(Guid id, UpdateDeliveryDto dto, CancellationToken cancellationToken = default)
    {
        var delivery = await _unitOfWork.Deliveries.GetByIdAsync(id, cancellationToken);
        if (delivery == null)
        {
            throw new KeyNotFoundException($"Delivery with ID {id} not found");
        }

        delivery.VerifiedQuantity = dto.VerifiedQuantity;
        delivery.DeliveryStatus = dto.DeliveryStatus;
        delivery.Notes = dto.Notes;
        delivery.UpdatedAt = DateTime.UtcNow;

        // Update chicken status if delivery is completed
        if (dto.DeliveryStatus == Domain.Enums.DeliveryStatus.Completed)
        {
            var distributionItems = await _unitOfWork.DistributionItems.FindAsync(
                di => di.DistributionId == delivery.DistributionId && di.ShopId == delivery.ShopId,
                cancellationToken);

            foreach (var item in distributionItems)
            {
                item.DeliveryStatus = Domain.Enums.DistributionItemStatus.Delivered;
                item.DeliveredAt = DateTime.UtcNow;
                await _unitOfWork.DistributionItems.UpdateAsync(item, cancellationToken);

                var chicken = await _unitOfWork.Chickens.GetByIdAsync(item.ChickenId, cancellationToken);
                if (chicken != null)
                {
                    chicken.Status = Domain.Enums.ChickenStatus.Delivered;
                    await _unitOfWork.Chickens.UpdateAsync(chicken, cancellationToken);
                }
            }
        }

        await _unitOfWork.Deliveries.UpdateAsync(delivery, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<PagedResult<DeliveryDto>> GetMyDeliveriesAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        // Get user to find their email
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID {userId} not found");
        }

        // Find shop by user's email
        var shops = await _unitOfWork.Shops.FindAsync(s => s.Email == user.Email && !s.IsDeleted, cancellationToken);
        var shop = shops.FirstOrDefault();
        if (shop == null)
        {
            return new PagedResult<DeliveryDto>
            {
                Items = new List<DeliveryDto>(),
                TotalCount = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // Get deliveries for this shop
        var allDeliveries = await _unitOfWork.Deliveries.FindAsync(d => d.ShopId == shop.Id && !d.IsDeleted, cancellationToken);
        var deliveriesList = allDeliveries.OrderByDescending(d => d.DeliveryDate).ToList();

        var totalCount = deliveriesList.Count;
        var pagedDeliveries = deliveriesList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = new List<DeliveryDto>();
        foreach (var delivery in pagedDeliveries)
        {
            var dto = _mapper.Map<DeliveryDto>(delivery);
            dto.ShopName = shop.Name;
            items.Add(dto);
        }

        return new PagedResult<DeliveryDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
