using AutoMapper;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Cleaner;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Domain.Interfaces;

namespace PoultryDistributionSystem.Application.Services;

/// <summary>
/// Cleaner service implementation
/// </summary>
public class CleanerService : ICleanerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CleanerService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<CleanerDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cleaner = await _unitOfWork.Cleaners.GetByIdAsync(id, cancellationToken);
        if (cleaner == null)
        {
            throw new KeyNotFoundException($"Cleaner with ID {id} not found");
        }

        return _mapper.Map<CleanerDto>(cleaner);
    }

    public async Task<PagedResult<CleanerDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allCleaners = await _unitOfWork.Cleaners.FindAsync(c => !c.IsDeleted, cancellationToken);
        var cleanersList = allCleaners.OrderBy(c => c.Name).ToList();

        var totalCount = cleanersList.Count;
        var pagedCleaners = cleanersList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<CleanerDto>
        {
            Items = _mapper.Map<List<CleanerDto>>(pagedCleaners),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<CleanerDto> CreateAsync(CreateCleanerDto dto, Guid createdBy, CancellationToken cancellationToken = default)
    {
        var cleaner = _mapper.Map<Domain.Entities.Cleaner>(dto);
        cleaner.CreatedBy = createdBy;

        await _unitOfWork.Cleaners.AddAsync(cleaner, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CleanerDto>(cleaner);
    }

    public async Task<CleanerDto> UpdateAsync(Guid id, CreateCleanerDto dto, CancellationToken cancellationToken = default)
    {
        var cleaner = await _unitOfWork.Cleaners.GetByIdAsync(id, cancellationToken);
        if (cleaner == null)
        {
            throw new KeyNotFoundException($"Cleaner with ID {id} not found");
        }

        cleaner.Name = dto.Name;
        cleaner.Phone = dto.Phone;
        cleaner.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Cleaners.UpdateAsync(cleaner, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CleanerDto>(cleaner);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cleaner = await _unitOfWork.Cleaners.GetByIdAsync(id, cancellationToken);
        if (cleaner == null)
        {
            return false;
        }

        await _unitOfWork.Cleaners.DeleteAsync(cleaner, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
