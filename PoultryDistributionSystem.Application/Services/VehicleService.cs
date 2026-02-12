using AutoMapper;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Vehicle;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Domain.Interfaces;

namespace PoultryDistributionSystem.Application.Services;

/// <summary>
/// Vehicle service implementation
/// </summary>
public class VehicleService : IVehicleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public VehicleService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<VehicleDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id, cancellationToken);
        if (vehicle == null)
        {
            throw new KeyNotFoundException($"Vehicle with ID {id} not found");
        }

        var driver = await _unitOfWork.Drivers.GetByIdAsync(vehicle.DriverId, cancellationToken);
        var cleaner = await _unitOfWork.Cleaners.GetByIdAsync(vehicle.CleanerId, cancellationToken);

        var dto = _mapper.Map<VehicleDto>(vehicle);
        dto.DriverName = driver?.Name ?? string.Empty;
        dto.CleanerName = cleaner?.Name ?? string.Empty;
        return dto;
    }

    public async Task<PagedResult<VehicleDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allVehicles = await _unitOfWork.Vehicles.FindAsync(v => !v.IsDeleted, cancellationToken);
        var vehiclesList = allVehicles.OrderBy(v => v.VehicleNumber).ToList();

        var totalCount = vehiclesList.Count;
        var pagedVehicles = vehiclesList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = new List<VehicleDto>();
        foreach (var vehicle in pagedVehicles)
        {
            var driver = await _unitOfWork.Drivers.GetByIdAsync(vehicle.DriverId, cancellationToken);
            var cleaner = await _unitOfWork.Cleaners.GetByIdAsync(vehicle.CleanerId, cancellationToken);

            var dto = _mapper.Map<VehicleDto>(vehicle);
            dto.DriverName = driver?.Name ?? string.Empty;
            dto.CleanerName = cleaner?.Name ?? string.Empty;
            items.Add(dto);
        }

        return new PagedResult<VehicleDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<VehicleDto> CreateAsync(CreateVehicleDto dto, Guid createdBy, CancellationToken cancellationToken = default)
    {
        var vehicle = new Domain.Entities.Vehicle
        {
            VehicleNumber = dto.VehicleNumber,
            Model = dto.Model,
            Capacity = dto.Capacity,
            DriverId = dto.DriverId,
            CleanerId = dto.CleanerId,
            CreatedBy = createdBy
        };

        await _unitOfWork.Vehicles.AddAsync(vehicle, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(vehicle.Id, cancellationToken);
    }

    public async Task<VehicleDto> UpdateAsync(Guid id, UpdateVehicleDto dto, CancellationToken cancellationToken = default)
    {
        var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id, cancellationToken);
        if (vehicle == null)
        {
            throw new KeyNotFoundException($"Vehicle with ID {id} not found");
        }

        vehicle.VehicleNumber = dto.VehicleNumber;
        vehicle.Model = dto.Model;
        vehicle.Capacity = dto.Capacity;
        vehicle.DriverId = dto.DriverId;
        vehicle.CleanerId = dto.CleanerId;
        vehicle.IsActive = dto.IsActive;
        vehicle.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Vehicles.UpdateAsync(vehicle, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(id, cancellationToken);
        if (vehicle == null)
        {
            return false;
        }

        await _unitOfWork.Vehicles.DeleteAsync(vehicle, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
