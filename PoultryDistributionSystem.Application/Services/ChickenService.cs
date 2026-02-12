using AutoMapper;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Chicken;
using PoultryDistributionSystem.Application.DTOs.Inventory;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Domain.Interfaces;

namespace PoultryDistributionSystem.Application.Services;

/// <summary>
/// Chicken service implementation
/// </summary>
public class ChickenService : IChickenService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IInventoryService _inventoryService;

    public ChickenService(IUnitOfWork unitOfWork, IMapper mapper, IInventoryService inventoryService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
    }

    public async Task<ChickenDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var chicken = await _unitOfWork.Chickens.GetByIdAsync(id, cancellationToken);
        if (chicken == null)
        {
            throw new KeyNotFoundException($"Chicken with ID {id} not found");
        }

        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(chicken.SupplierId, cancellationToken);
        var farm = chicken.FarmId.HasValue ? await _unitOfWork.Farms.GetByIdAsync(chicken.FarmId.Value, cancellationToken) : null;

        var dto = _mapper.Map<ChickenDto>(chicken);
        dto.SupplierName = supplier?.Name ?? string.Empty;
        dto.FarmName = farm?.Name;
        return dto;
    }

    public async Task<PagedResult<ChickenDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allChickens = await _unitOfWork.Chickens.FindAsync(c => !c.IsDeleted, cancellationToken);
        var chickensList = allChickens.OrderByDescending(c => c.CreatedAt).ToList();

        var totalCount = chickensList.Count;
        var pagedChickens = chickensList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = new List<ChickenDto>();
        foreach (var chicken in pagedChickens)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(chicken.SupplierId, cancellationToken);
            var farm = chicken.FarmId.HasValue ? await _unitOfWork.Farms.GetByIdAsync(chicken.FarmId.Value, cancellationToken) : null;

            var dto = _mapper.Map<ChickenDto>(chicken);
            dto.SupplierName = supplier?.Name ?? string.Empty;
            dto.FarmName = farm?.Name;
            items.Add(dto);
        }

        return new PagedResult<ChickenDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<ChickenDto>> GetByFarmIdAsync(Guid farmId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allChickens = await _unitOfWork.Chickens.FindAsync(c => c.FarmId == farmId && !c.IsDeleted, cancellationToken);
        var chickensList = allChickens.OrderByDescending(c => c.CreatedAt).ToList();

        var totalCount = chickensList.Count;
        var pagedChickens = chickensList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = new List<ChickenDto>();
        foreach (var chicken in pagedChickens)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(chicken.SupplierId, cancellationToken);
            var farm = await _unitOfWork.Farms.GetByIdAsync(farmId, cancellationToken);

            var dto = _mapper.Map<ChickenDto>(chicken);
            dto.SupplierName = supplier?.Name ?? string.Empty;
            dto.FarmName = farm?.Name;
            items.Add(dto);
        }

        return new PagedResult<ChickenDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<ChickenDto> CreateAsync(CreateChickenDto dto, Guid createdBy, CancellationToken cancellationToken = default)
    {
        var chicken = new Domain.Entities.Chicken
        {
            BatchNumber = dto.BatchNumber,
            SupplierId = dto.SupplierId,
            FarmId = dto.FarmId,
            PurchaseDate = dto.PurchaseDate,
            Quantity = dto.Quantity,
            WeightKg = dto.WeightKg,
            Status = Domain.Enums.ChickenStatus.Purchased,
            HealthStatus = Domain.Enums.HealthStatus.Healthy,
            CreatedBy = createdBy
        };

        await _unitOfWork.Chickens.AddAsync(chicken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Update farm capacity if assigned
        if (dto.FarmId.HasValue)
        {
            var farm = await _unitOfWork.Farms.GetByIdAsync(dto.FarmId.Value, cancellationToken);
            if (farm != null)
            {
                farm.CurrentCount += dto.Quantity;
                await _unitOfWork.Farms.UpdateAsync(farm, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Create stock movement for purchase
                try
                {
                    await _inventoryService.RecordStockMovementAsync(
                        new DTOs.Inventory.CreateStockMovementDto
                        {
                            FarmId = dto.FarmId.Value,
                            ChickenId = chicken.Id,
                            MovementType = Domain.Enums.StockMovementType.In,
                            Quantity = dto.Quantity,
                            Reason = $"Purchase - Batch {chicken.BatchNumber}",
                            MovementDate = dto.PurchaseDate
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

        return await GetByIdAsync(chicken.Id, cancellationToken);
    }

    public async Task<ChickenDto> UpdateAsync(Guid id, UpdateChickenDto dto, CancellationToken cancellationToken = default)
    {
        var chicken = await _unitOfWork.Chickens.GetByIdAsync(id, cancellationToken);
        if (chicken == null)
        {
            throw new KeyNotFoundException($"Chicken with ID {id} not found");
        }

        // Handle farm change
        if (dto.FarmId.HasValue && chicken.FarmId != dto.FarmId.Value)
        {
            // Remove from old farm
            if (chicken.FarmId.HasValue)
            {
                var oldFarm = await _unitOfWork.Farms.GetByIdAsync(chicken.FarmId.Value, cancellationToken);
                if (oldFarm != null)
                {
                    oldFarm.CurrentCount = Math.Max(0, oldFarm.CurrentCount - chicken.Quantity);
                    await _unitOfWork.Farms.UpdateAsync(oldFarm, cancellationToken);
                }
            }

            // Add to new farm
            var newFarm = await _unitOfWork.Farms.GetByIdAsync(dto.FarmId.Value, cancellationToken);
            if (newFarm != null)
            {
                newFarm.CurrentCount += chicken.Quantity;
                await _unitOfWork.Farms.UpdateAsync(newFarm, cancellationToken);
            }
        }

        chicken.FarmId = dto.FarmId;
        chicken.AgeDays = dto.AgeDays;
        chicken.WeightKg = dto.WeightKg;
        chicken.Status = dto.Status;
        chicken.HealthStatus = dto.HealthStatus;
        chicken.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Chickens.UpdateAsync(chicken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var chicken = await _unitOfWork.Chickens.GetByIdAsync(id, cancellationToken);
        if (chicken == null)
        {
            return false;
        }

        await _unitOfWork.Chickens.DeleteAsync(chicken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
