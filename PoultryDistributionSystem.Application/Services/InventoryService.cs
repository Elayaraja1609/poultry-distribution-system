using AutoMapper;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Inventory;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Domain.Entities;
using PoultryDistributionSystem.Domain.Enums;
using PoultryDistributionSystem.Domain.Interfaces;

namespace PoultryDistributionSystem.Application.Services;

/// <summary>
/// Inventory service implementation
/// </summary>
public class InventoryService : IInventoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public InventoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<StockMovementDto> RecordStockMovementAsync(CreateStockMovementDto dto, Guid createdBy, CancellationToken cancellationToken = default)
    {
        var farm = await _unitOfWork.Farms.GetByIdAsync(dto.FarmId, cancellationToken);
        if (farm == null)
        {
            throw new KeyNotFoundException($"Farm with ID {dto.FarmId} not found");
        }

        var chicken = await _unitOfWork.Chickens.GetByIdAsync(dto.ChickenId, cancellationToken);
        if (chicken == null)
        {
            throw new KeyNotFoundException($"Chicken with ID {dto.ChickenId} not found");
        }

        // Calculate current stock
        var currentStock = await CalculateAvailableStockAsync(dto.FarmId, dto.ChickenId, cancellationToken);
        var newStock = dto.MovementType switch
        {
            StockMovementType.In => currentStock + dto.Quantity,
            StockMovementType.Out => currentStock - dto.Quantity,
            StockMovementType.Loss => currentStock - dto.Quantity,
            StockMovementType.Adjustment => dto.Quantity, // For adjustments, quantity is the new total
            _ => currentStock
        };

        if (newStock < 0)
        {
            throw new InvalidOperationException("Stock cannot be negative");
        }

        var movement = new StockMovement
        {
            FarmId = dto.FarmId,
            ChickenId = dto.ChickenId,
            MovementType = dto.MovementType,
            Quantity = dto.Quantity,
            PreviousQuantity = currentStock,
            NewQuantity = newStock,
            Reason = dto.Reason,
            MovementDate = dto.MovementDate ?? DateTime.UtcNow,
            CreatedBy = createdBy
        };

        await _unitOfWork.StockMovements.AddAsync(movement, cancellationToken);

        // Update farm current count if needed
        if (chicken.FarmId == dto.FarmId)
        {
            var farmChickens = await _unitOfWork.Chickens.FindAsync(c => c.FarmId == dto.FarmId && !c.IsDeleted, cancellationToken);
            farm.CurrentCount = farmChickens.Sum(c => c.Quantity);
            await _unitOfWork.Farms.UpdateAsync(farm, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = _mapper.Map<StockMovementDto>(movement);
        result.FarmName = farm.Name;
        result.BatchNumber = chicken.BatchNumber;
        return result;
    }

    public async Task<FarmInventoryDto> GetFarmInventoryAsync(Guid farmId, CancellationToken cancellationToken = default)
    {
        var farm = await _unitOfWork.Farms.GetByIdAsync(farmId, cancellationToken);
        if (farm == null)
        {
            throw new KeyNotFoundException($"Farm with ID {farmId} not found");
        }

        var chickens = await _unitOfWork.Chickens.FindAsync(c => c.FarmId == farmId && !c.IsDeleted, cancellationToken);
        var chickenStocks = new List<ChickenStockDto>();

        foreach (var chicken in chickens)
        {
            var availableStock = await CalculateAvailableStockAsync(farmId, chicken.Id, cancellationToken);
            chickenStocks.Add(new ChickenStockDto
            {
                ChickenId = chicken.Id,
                BatchNumber = chicken.BatchNumber,
                Quantity = chicken.Quantity,
                AvailableQuantity = availableStock,
                Status = chicken.Status.ToString()
            });
        }

        var movements = await _unitOfWork.StockMovements.FindAsync(
            m => m.FarmId == farmId && !m.IsDeleted,
            cancellationToken);

        var stockIn = movements.Where(m => m.MovementType == StockMovementType.In).Sum(m => m.Quantity);
        var stockOut = movements.Where(m => m.MovementType == StockMovementType.Out).Sum(m => m.Quantity);
        var stockLoss = movements.Where(m => m.MovementType == StockMovementType.Loss).Sum(m => m.Quantity);
        var currentStock = chickenStocks.Sum(c => c.AvailableQuantity);

        return new FarmInventoryDto
        {
            FarmId = farm.Id,
            FarmName = farm.Name,
            Capacity = farm.Capacity,
            CurrentStock = currentStock,
            AvailableStock = currentStock,
            StockIn = stockIn,
            StockOut = stockOut,
            StockLoss = stockLoss,
            ChickenStocks = chickenStocks
        };
    }

    public async Task<PagedResult<StockMovementDto>> GetStockMovementsAsync(
        Guid? farmId,
        Guid? chickenId,
        DateTime? startDate,
        DateTime? endDate,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var allMovements = await _unitOfWork.StockMovements.FindAsync(m => !m.IsDeleted, cancellationToken);

        if (farmId.HasValue)
        {
            allMovements = allMovements.Where(m => m.FarmId == farmId.Value);
        }

        if (chickenId.HasValue)
        {
            allMovements = allMovements.Where(m => m.ChickenId == chickenId.Value);
        }

        if (startDate.HasValue)
        {
            allMovements = allMovements.Where(m => m.MovementDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            allMovements = allMovements.Where(m => m.MovementDate <= endDate.Value);
        }

        var movementsList = allMovements.OrderByDescending(m => m.MovementDate).ToList();
        var totalCount = movementsList.Count;
        var pagedMovements = movementsList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = new List<StockMovementDto>();
        foreach (var movement in pagedMovements)
        {
            var farm = await _unitOfWork.Farms.GetByIdAsync(movement.FarmId, cancellationToken);
            var chicken = await _unitOfWork.Chickens.GetByIdAsync(movement.ChickenId, cancellationToken);

            var dto = _mapper.Map<StockMovementDto>(movement);
            dto.FarmName = farm?.Name ?? string.Empty;
            dto.BatchNumber = chicken?.BatchNumber ?? string.Empty;
            items.Add(dto);
        }

        return new PagedResult<StockMovementDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<int> CalculateAvailableStockAsync(Guid farmId, Guid chickenId, CancellationToken cancellationToken = default)
    {
        var movements = await _unitOfWork.StockMovements.FindAsync(
            m => m.FarmId == farmId && m.ChickenId == chickenId && !m.IsDeleted,
            cancellationToken);

        var stockIn = movements.Where(m => m.MovementType == StockMovementType.In).Sum(m => m.Quantity);
        var stockOut = movements.Where(m => m.MovementType == StockMovementType.Out).Sum(m => m.Quantity);
        var stockLoss = movements.Where(m => m.MovementType == StockMovementType.Loss).Sum(m => m.Quantity);

        // For adjustments, use the latest adjustment value
        var latestAdjustment = movements
            .Where(m => m.MovementType == StockMovementType.Adjustment)
            .OrderByDescending(m => m.MovementDate)
            .FirstOrDefault();

        if (latestAdjustment != null)
        {
            return latestAdjustment.NewQuantity;
        }

        return stockIn - stockOut - stockLoss;
    }

    public async Task<StockSummaryDto> GetStockSummaryAsync(Guid farmId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken = default)
    {
        var movements = await _unitOfWork.StockMovements.FindAsync(
            m => m.FarmId == farmId && !m.IsDeleted,
            cancellationToken);

        if (startDate.HasValue)
        {
            movements = movements.Where(m => m.MovementDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            movements = movements.Where(m => m.MovementDate <= endDate.Value);
        }

        return new StockSummaryDto
        {
            TotalIn = movements.Where(m => m.MovementType == StockMovementType.In).Sum(m => m.Quantity),
            TotalOut = movements.Where(m => m.MovementType == StockMovementType.Out).Sum(m => m.Quantity),
            TotalLoss = movements.Where(m => m.MovementType == StockMovementType.Loss).Sum(m => m.Quantity),
            TotalAdjustments = movements.Where(m => m.MovementType == StockMovementType.Adjustment).Count(),
            NetChange = movements.Where(m => m.MovementType == StockMovementType.In).Sum(m => m.Quantity) -
                       movements.Where(m => m.MovementType == StockMovementType.Out).Sum(m => m.Quantity) -
                       movements.Where(m => m.MovementType == StockMovementType.Loss).Sum(m => m.Quantity)
        };
    }
}
