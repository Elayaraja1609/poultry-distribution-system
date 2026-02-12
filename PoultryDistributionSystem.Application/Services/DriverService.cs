using AutoMapper;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Driver;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Domain.Interfaces;

namespace PoultryDistributionSystem.Application.Services;

/// <summary>
/// Driver service implementation
/// </summary>
public class DriverService : IDriverService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DriverService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<DriverDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var driver = await _unitOfWork.Drivers.GetByIdAsync(id, cancellationToken);
        if (driver == null)
        {
            throw new KeyNotFoundException($"Driver with ID {id} not found");
        }

        return _mapper.Map<DriverDto>(driver);
    }

    public async Task<PagedResult<DriverDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allDrivers = await _unitOfWork.Drivers.FindAsync(d => !d.IsDeleted, cancellationToken);
        var driversList = allDrivers.OrderBy(d => d.Name).ToList();

        var totalCount = driversList.Count;
        var pagedDrivers = driversList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<DriverDto>
        {
            Items = _mapper.Map<List<DriverDto>>(pagedDrivers),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<DriverDto> CreateAsync(CreateDriverDto dto, Guid createdBy, CancellationToken cancellationToken = default)
    {
        var driver = _mapper.Map<Domain.Entities.Driver>(dto);
        driver.CreatedBy = createdBy;

        await _unitOfWork.Drivers.AddAsync(driver, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<DriverDto>(driver);
    }

    public async Task<DriverDto> UpdateAsync(Guid id, CreateDriverDto dto, CancellationToken cancellationToken = default)
    {
        var driver = await _unitOfWork.Drivers.GetByIdAsync(id, cancellationToken);
        if (driver == null)
        {
            throw new KeyNotFoundException($"Driver with ID {id} not found");
        }

        driver.Name = dto.Name;
        driver.Phone = dto.Phone;
        driver.LicenseNumber = dto.LicenseNumber;
        driver.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Drivers.UpdateAsync(driver, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<DriverDto>(driver);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var driver = await _unitOfWork.Drivers.GetByIdAsync(id, cancellationToken);
        if (driver == null)
        {
            return false;
        }

        await _unitOfWork.Drivers.DeleteAsync(driver, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
