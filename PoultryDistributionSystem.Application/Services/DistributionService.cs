using AutoMapper;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Distribution;
using PoultryDistributionSystem.Application.DTOs.Inventory;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Domain.Entities;
using PoultryDistributionSystem.Domain.Interfaces;

namespace PoultryDistributionSystem.Application.Services;

/// <summary>
/// Distribution service implementation
/// </summary>
public class DistributionService : IDistributionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IInventoryService _inventoryService;
    private readonly INotificationService _notificationService;

    public DistributionService(IUnitOfWork unitOfWork, IMapper mapper, IInventoryService inventoryService, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
        _notificationService = notificationService;
    }

    public async Task<DistributionDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var distribution = await _unitOfWork.Distributions.GetByIdAsync(id, cancellationToken);
        if (distribution == null)
        {
            throw new KeyNotFoundException($"Distribution with ID {id} not found");
        }

        var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(distribution.VehicleId, cancellationToken);
        var driver = vehicle != null ? await _unitOfWork.Drivers.GetByIdAsync(vehicle.DriverId, cancellationToken) : null;
        var cleaner = vehicle != null ? await _unitOfWork.Cleaners.GetByIdAsync(vehicle.CleanerId, cancellationToken) : null;
        var items = await _unitOfWork.DistributionItems.FindAsync(di => di.DistributionId == id, cancellationToken);

        var dto = _mapper.Map<DistributionDto>(distribution);
        dto.VehicleNumber = vehicle?.VehicleNumber ?? string.Empty;
        dto.DriverName = driver?.Name ?? string.Empty;
        dto.CleanerName = cleaner?.Name ?? string.Empty;
        dto.TotalItems = items.Count();
        return dto;
    }

    public async Task<PagedResult<DistributionDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allDistributions = await _unitOfWork.Distributions.FindAsync(d => !d.IsDeleted, cancellationToken);
        var distributionsList = allDistributions.OrderByDescending(d => d.ScheduledDate).ToList();

        var totalCount = distributionsList.Count;
        var pagedDistributions = distributionsList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = new List<DistributionDto>();
        foreach (var distribution in pagedDistributions)
        {
            var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(distribution.VehicleId, cancellationToken);
            var driver = vehicle != null ? await _unitOfWork.Drivers.GetByIdAsync(vehicle.DriverId, cancellationToken) : null;
            var cleaner = vehicle != null ? await _unitOfWork.Cleaners.GetByIdAsync(vehicle.CleanerId, cancellationToken) : null;
            var distributionItems = await _unitOfWork.DistributionItems.FindAsync(di => di.DistributionId == distribution.Id, cancellationToken);

            var dto = _mapper.Map<DistributionDto>(distribution);
            dto.VehicleNumber = vehicle?.VehicleNumber ?? string.Empty;
            dto.DriverName = driver?.Name ?? string.Empty;
            dto.CleanerName = cleaner?.Name ?? string.Empty;
            dto.TotalItems = distributionItems.Count();
            items.Add(dto);
        }

        return new PagedResult<DistributionDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<DistributionDto> CreateAsync(CreateDistributionDto dto, Guid createdBy, CancellationToken cancellationToken = default)
    {
        var distribution = new Distribution
        {
            VehicleId = dto.VehicleId,
            ScheduledDate = dto.ScheduledDate,
            Status = Domain.Enums.DistributionStatus.Scheduled,
            CreatedBy = createdBy
        };

        await _unitOfWork.Distributions.AddAsync(distribution, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create distribution items
        foreach (var item in dto.Items)
        {
            var distributionItem = new DistributionItem
            {
                DistributionId = distribution.Id,
                ChickenId = item.ChickenId,
                ShopId = item.ShopId,
                Quantity = item.Quantity,
                DeliveryStatus = Domain.Enums.DistributionItemStatus.Pending,
                CreatedBy = createdBy
            };

            await _unitOfWork.DistributionItems.AddAsync(distributionItem, cancellationToken);

            // Update chicken status
            var chicken = await _unitOfWork.Chickens.GetByIdAsync(item.ChickenId, cancellationToken);
            if (chicken != null)
            {
                chicken.Status = Domain.Enums.ChickenStatus.ReadyForDistribution;
                await _unitOfWork.Chickens.UpdateAsync(chicken, cancellationToken);

                // Create stock movement for distribution
                if (chicken.FarmId.HasValue)
                {
                    try
                    {
                        await _inventoryService.RecordStockMovementAsync(
                            new CreateStockMovementDto
                            {
                                FarmId = chicken.FarmId.Value,
                                ChickenId = chicken.Id,
                                MovementType = Domain.Enums.StockMovementType.Out,
                                Quantity = item.Quantity,
                                Reason = $"Distribution to shop - Distribution #{distribution.Id}",
                                MovementDate = dto.ScheduledDate
                            },
                            createdBy,
                            cancellationToken);
                    }
                    catch
                    {
                        // Log error but don't fail the operation
                    }
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send notifications to shop owners
        try
        {
            var shopIds = dto.Items.Select(i => i.ShopId).Distinct();
            foreach (var shopId in shopIds)
            {
                var shop = await _unitOfWork.Shops.GetByIdAsync(shopId, cancellationToken);
                if (shop != null)
                {
                    var shopUser = await _unitOfWork.Users.FirstOrDefaultAsync(
                        u => u.Email == shop.Email && !u.IsDeleted,
                        cancellationToken);
                    if (shopUser != null)
                    {
                        await _notificationService.SendDeliveryScheduledNotificationAsync(
                            distribution.Id,
                            shopUser.Id,
                            cancellationToken);
                    }
                }
            }
        }
        catch
        {
            // Log error but don't fail the operation
        }

        return await GetByIdAsync(distribution.Id, cancellationToken);
    }

    public async Task<DistributionDto> UpdateStatusAsync(Guid id, Domain.Enums.DistributionStatus status, CancellationToken cancellationToken = default)
    {
        var distribution = await _unitOfWork.Distributions.GetByIdAsync(id, cancellationToken);
        if (distribution == null)
        {
            throw new KeyNotFoundException($"Distribution with ID {id} not found");
        }

        distribution.Status = status;
        distribution.UpdatedAt = DateTime.UtcNow;

        // Update chicken status if distribution is completed
        if (status == Domain.Enums.DistributionStatus.Completed)
        {
            var items = await _unitOfWork.DistributionItems.FindAsync(
                di => di.DistributionId == id, cancellationToken);

            foreach (var item in items)
            {
                var chicken = await _unitOfWork.Chickens.GetByIdAsync(item.ChickenId, cancellationToken);
                if (chicken != null)
                {
                    chicken.Status = Domain.Enums.ChickenStatus.InTransit;
                    await _unitOfWork.Chickens.UpdateAsync(chicken, cancellationToken);
                }
            }
        }

        await _unitOfWork.Distributions.UpdateAsync(distribution, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var distribution = await _unitOfWork.Distributions.GetByIdAsync(id, cancellationToken);
        if (distribution == null)
        {
            return false;
        }

        await _unitOfWork.Distributions.DeleteAsync(distribution, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
