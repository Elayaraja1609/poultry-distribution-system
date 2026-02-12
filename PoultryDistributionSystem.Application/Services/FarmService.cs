using AutoMapper;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Farm;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Domain.Interfaces;

namespace PoultryDistributionSystem.Application.Services;

/// <summary>
/// Farm service implementation
/// </summary>
public class FarmService : IFarmService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public FarmService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<FarmDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var farm = await _unitOfWork.Farms.GetByIdAsync(id, cancellationToken);
        if (farm == null)
        {
            throw new KeyNotFoundException($"Farm with ID {id} not found");
        }

        return _mapper.Map<FarmDto>(farm);
    }

    public async Task<PagedResult<FarmDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allFarms = await _unitOfWork.Farms.FindAsync(f => !f.IsDeleted, cancellationToken);
        var farmsList = allFarms.OrderBy(f => f.Name).ToList();

        var totalCount = farmsList.Count;
        var pagedFarms = farmsList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<FarmDto>
        {
            Items = _mapper.Map<List<FarmDto>>(pagedFarms),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<FarmDto> CreateAsync(CreateFarmDto dto, Guid createdBy, CancellationToken cancellationToken = default)
    {
        var farm = _mapper.Map<Domain.Entities.Farm>(dto);
        farm.CreatedBy = createdBy;

        await _unitOfWork.Farms.AddAsync(farm, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<FarmDto>(farm);
    }

    public async Task<FarmDto> UpdateAsync(Guid id, UpdateFarmDto dto, CancellationToken cancellationToken = default)
    {
        var farm = await _unitOfWork.Farms.GetByIdAsync(id, cancellationToken);
        if (farm == null)
        {
            throw new KeyNotFoundException($"Farm with ID {id} not found");
        }

        _mapper.Map(dto, farm);
        farm.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Farms.UpdateAsync(farm, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<FarmDto>(farm);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var farm = await _unitOfWork.Farms.GetByIdAsync(id, cancellationToken);
        if (farm == null)
        {
            return false;
        }

        await _unitOfWork.Farms.DeleteAsync(farm, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task UpdateCapacityAsync(Guid id, int currentCount, CancellationToken cancellationToken = default)
    {
        var farm = await _unitOfWork.Farms.GetByIdAsync(id, cancellationToken);
        if (farm == null)
        {
            throw new KeyNotFoundException($"Farm with ID {id} not found");
        }

        if (currentCount > farm.Capacity)
        {
            throw new InvalidOperationException("Current count cannot exceed farm capacity");
        }

        farm.CurrentCount = currentCount;
        farm.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Farms.UpdateAsync(farm, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
