using AutoMapper;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Supplier;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Domain.Interfaces;

namespace PoultryDistributionSystem.Application.Services;

/// <summary>
/// Supplier service implementation
/// </summary>
public class SupplierService : ISupplierService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SupplierService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<SupplierDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id, cancellationToken);
        if (supplier == null)
        {
            throw new KeyNotFoundException($"Supplier with ID {id} not found");
        }

        return _mapper.Map<SupplierDto>(supplier);
    }

    public async Task<PagedResult<SupplierDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allSuppliers = await _unitOfWork.Suppliers.FindAsync(s => !s.IsDeleted, cancellationToken);
        var suppliersList = allSuppliers.OrderBy(s => s.Name).ToList();

        var totalCount = suppliersList.Count;
        var pagedSuppliers = suppliersList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<SupplierDto>
        {
            Items = _mapper.Map<List<SupplierDto>>(pagedSuppliers),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierDto dto, Guid createdBy, CancellationToken cancellationToken = default)
    {
        var supplier = _mapper.Map<Domain.Entities.Supplier>(dto);
        supplier.CreatedBy = createdBy;

        await _unitOfWork.Suppliers.AddAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SupplierDto>(supplier);
    }

    public async Task<SupplierDto> UpdateAsync(Guid id, UpdateSupplierDto dto, CancellationToken cancellationToken = default)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id, cancellationToken);
        if (supplier == null)
        {
            throw new KeyNotFoundException($"Supplier with ID {id} not found");
        }

        _mapper.Map(dto, supplier);
        supplier.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Suppliers.UpdateAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SupplierDto>(supplier);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id, cancellationToken);
        if (supplier == null)
        {
            return false;
        }

        await _unitOfWork.Suppliers.DeleteAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return true;
    }
}
