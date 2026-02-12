using AutoMapper;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Sale;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Domain.Entities;
using PoultryDistributionSystem.Domain.Interfaces;

namespace PoultryDistributionSystem.Application.Services;

/// <summary>
/// Sales service implementation
/// </summary>
public class SalesService : ISalesService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SalesService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<SaleDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await _unitOfWork.Sales.GetByIdAsync(id, cancellationToken);
        if (sale == null)
        {
            throw new KeyNotFoundException($"Sale with ID {id} not found");
        }

        var shop = await _unitOfWork.Shops.GetByIdAsync(sale.ShopId, cancellationToken);
        var payments = await _unitOfWork.Payments.FindAsync(p => p.SaleId == id, cancellationToken);

        var dto = _mapper.Map<SaleDto>(sale);
        dto.ShopName = shop?.Name ?? string.Empty;
        dto.PaidAmount = payments.Sum(p => p.Amount);
        dto.RemainingAmount = sale.TotalAmount - dto.PaidAmount;
        return dto;
    }

    public async Task<PagedResult<SaleDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allSales = await _unitOfWork.Sales.FindAsync(s => !s.IsDeleted, cancellationToken);
        var salesList = allSales.OrderByDescending(s => s.SaleDate).ToList();

        var totalCount = salesList.Count;
        var pagedSales = salesList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = new List<SaleDto>();
        foreach (var sale in pagedSales)
        {
            var shop = await _unitOfWork.Shops.GetByIdAsync(sale.ShopId, cancellationToken);
            var payments = await _unitOfWork.Payments.FindAsync(p => p.SaleId == sale.Id, cancellationToken);

            var dto = _mapper.Map<SaleDto>(sale);
            dto.ShopName = shop?.Name ?? string.Empty;
            dto.PaidAmount = payments.Sum(p => p.Amount);
            dto.RemainingAmount = sale.TotalAmount - dto.PaidAmount;
            items.Add(dto);
        }

        return new PagedResult<SaleDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PagedResult<SaleDto>> GetByShopIdAsync(Guid shopId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var allSales = await _unitOfWork.Sales.FindAsync(s => s.ShopId == shopId && !s.IsDeleted, cancellationToken);
        var salesList = allSales.OrderByDescending(s => s.SaleDate).ToList();

        var totalCount = salesList.Count;
        var pagedSales = salesList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = new List<SaleDto>();
        var shop = await _unitOfWork.Shops.GetByIdAsync(shopId, cancellationToken);

        foreach (var sale in pagedSales)
        {
            var payments = await _unitOfWork.Payments.FindAsync(p => p.SaleId == sale.Id, cancellationToken);

            var dto = _mapper.Map<SaleDto>(sale);
            dto.ShopName = shop?.Name ?? string.Empty;
            dto.PaidAmount = payments.Sum(p => p.Amount);
            dto.RemainingAmount = sale.TotalAmount - dto.PaidAmount;
            items.Add(dto);
        }

        return new PagedResult<SaleDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<SaleDto> CreateAsync(CreateSaleDto dto, Guid createdBy, CancellationToken cancellationToken = default)
    {
        var sale = new Sale
        {
            DeliveryId = dto.DeliveryId,
            ShopId = dto.ShopId,
            SaleDate = dto.SaleDate,
            TotalAmount = dto.TotalAmount,
            PaymentStatus = Domain.Enums.PaymentStatus.Pending,
            PaymentMethod = dto.PaymentMethod,
            CreatedBy = createdBy
        };

        await _unitOfWork.Sales.AddAsync(sale, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(sale.Id, cancellationToken);
    }

    public async Task<PagedResult<SaleDto>> GetMySalesAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
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
            return new PagedResult<SaleDto>
            {
                Items = new List<SaleDto>(),
                TotalCount = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // Get sales for this shop
        var allSales = await _unitOfWork.Sales.FindAsync(s => s.ShopId == shop.Id && !s.IsDeleted, cancellationToken);
        var salesList = allSales.OrderByDescending(s => s.SaleDate).ToList();

        var totalCount = salesList.Count;
        var pagedSales = salesList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var items = new List<SaleDto>();
        foreach (var sale in pagedSales)
        {
            var payments = await _unitOfWork.Payments.FindAsync(p => p.SaleId == sale.Id, cancellationToken);

            var dto = _mapper.Map<SaleDto>(sale);
            dto.ShopName = shop.Name;
            dto.PaidAmount = payments.Sum(p => p.Amount);
            dto.RemainingAmount = sale.TotalAmount - dto.PaidAmount;
            items.Add(dto);
        }

        return new PagedResult<SaleDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
