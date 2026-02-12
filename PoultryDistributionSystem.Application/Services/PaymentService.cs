using AutoMapper;
using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Payment;
using PoultryDistributionSystem.Application.Interfaces;
using PoultryDistributionSystem.Domain.Entities;
using PoultryDistributionSystem.Domain.Interfaces;

namespace PoultryDistributionSystem.Application.Services;

/// <summary>
/// Payment service implementation
/// </summary>
public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPaymentGatewayService _paymentGatewayService;
    private readonly INotificationService? _notificationService;

    public PaymentService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPaymentGatewayService paymentGatewayService,
        INotificationService? notificationService = null)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _paymentGatewayService = paymentGatewayService ?? throw new ArgumentNullException(nameof(paymentGatewayService));
        _notificationService = notificationService;
    }

    public async Task<PaymentDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var payment = await _unitOfWork.Payments.GetByIdAsync(id, cancellationToken);
        if (payment == null)
        {
            throw new KeyNotFoundException($"Payment with ID {id} not found");
        }

        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<PagedResult<PaymentDto>> GetBySaleIdAsync(Guid saleId, CancellationToken cancellationToken = default)
    {
        var allPayments = await _unitOfWork.Payments.FindAsync(p => p.SaleId == saleId, cancellationToken);
        var paymentsList = allPayments.OrderByDescending(p => p.PaymentDate).ToList();

        return new PagedResult<PaymentDto>
        {
            Items = _mapper.Map<List<PaymentDto>>(paymentsList),
            TotalCount = paymentsList.Count,
            PageNumber = 1,
            PageSize = paymentsList.Count
        };
    }

    public async Task<PaymentDto> CreateAsync(CreatePaymentDto dto, Guid createdBy, CancellationToken cancellationToken = default)
    {
        var sale = await _unitOfWork.Sales.GetByIdAsync(dto.SaleId, cancellationToken);
        if (sale == null)
        {
            throw new KeyNotFoundException($"Sale with ID {dto.SaleId} not found");
        }

        // Get existing payments
        var existingPayments = await _unitOfWork.Payments.FindAsync(p => p.SaleId == dto.SaleId, cancellationToken);
        var totalPaid = existingPayments.Sum(p => p.Amount);

        if (totalPaid + dto.Amount > sale.TotalAmount)
        {
            throw new InvalidOperationException("Payment amount exceeds sale total");
        }

        var payment = new Payment
        {
            SaleId = dto.SaleId,
            Amount = dto.Amount,
            PaymentDate = DateTime.UtcNow,
            PaymentMethod = dto.PaymentMethod,
            PaymentGatewayTransactionId = dto.PaymentGatewayTransactionId,
            Notes = dto.Notes,
            CreatedBy = createdBy
        };

        await _unitOfWork.Payments.AddAsync(payment, cancellationToken);

        // Update sale payment status
        var newTotalPaid = totalPaid + dto.Amount;
        if (newTotalPaid >= sale.TotalAmount)
        {
            sale.PaymentStatus = Domain.Enums.PaymentStatus.Paid;
        }
        else if (newTotalPaid > 0)
        {
            sale.PaymentStatus = Domain.Enums.PaymentStatus.Partial;
        }

        await _unitOfWork.Sales.UpdateAsync(sale, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Send payment reminder if still pending
        if (_notificationService != null && sale.PaymentStatus != Domain.Enums.PaymentStatus.Paid)
        {
            try
            {
                // Get shop owner user ID from sale
                var shop = await _unitOfWork.Shops.GetByIdAsync(sale.ShopId, cancellationToken);
                if (shop != null)
                {
                    var shopUser = await _unitOfWork.Users.FirstOrDefaultAsync(
                        u => u.Email == shop.Email && !u.IsDeleted,
                        cancellationToken);
                    if (shopUser != null)
                    {
                        await _notificationService.SendPaymentReminderNotificationAsync(
                            sale.Id,
                            shopUser.Id,
                            cancellationToken);
                    }
                }
            }
            catch
            {
                // Log error but don't fail the operation
            }
        }

        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto> ProcessPaymentGatewayAsync(CreatePaymentDto dto, Guid createdBy, CancellationToken cancellationToken = default)
    {
        var sale = await _unitOfWork.Sales.GetByIdAsync(dto.SaleId, cancellationToken);
        if (sale == null)
        {
            throw new KeyNotFoundException($"Sale with ID {dto.SaleId} not found");
        }

        // Process payment through gateway
        var gatewayResult = await _paymentGatewayService.ProcessPaymentAsync(
            dto.Amount,
            "USD",
            $"Payment for Sale #{sale.Id}",
            cancellationToken);

        if (!gatewayResult.Success)
        {
            throw new InvalidOperationException(gatewayResult.ErrorMessage ?? "Payment gateway processing failed");
        }

        // Create payment with gateway transaction ID
        var paymentDto = new CreatePaymentDto
        {
            SaleId = dto.SaleId,
            Amount = dto.Amount,
            PaymentMethod = Domain.Enums.PaymentMethod.PaymentGateway,
            PaymentGatewayTransactionId = gatewayResult.TransactionId,
            Notes = dto.Notes
        };

        return await CreateAsync(paymentDto, createdBy, cancellationToken);
    }

    public async Task<PagedResult<PaymentDto>> GetMyPaymentsAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
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
            return new PagedResult<PaymentDto>
            {
                Items = new List<PaymentDto>(),
                TotalCount = 0,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        // Get sales for this shop
        var sales = await _unitOfWork.Sales.FindAsync(s => s.ShopId == shop.Id && !s.IsDeleted, cancellationToken);
        var saleIds = sales.Select(s => s.Id).ToList();

        // Get payments for these sales
        var allPayments = await _unitOfWork.Payments.FindAsync(p => saleIds.Contains(p.SaleId), cancellationToken);
        var paymentsList = allPayments.OrderByDescending(p => p.PaymentDate).ToList();

        var totalCount = paymentsList.Count;
        var pagedPayments = paymentsList
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PagedResult<PaymentDto>
        {
            Items = _mapper.Map<List<PaymentDto>>(pagedPayments),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PaymentIntentDto> CreatePaymentIntentAsync(decimal amount, Guid saleId, CancellationToken cancellationToken = default)
    {
        var sale = await _unitOfWork.Sales.GetByIdAsync(saleId, cancellationToken);
        if (sale == null)
        {
            throw new KeyNotFoundException($"Sale with ID {saleId} not found");
        }

        // Get existing payments
        var existingPayments = await _unitOfWork.Payments.FindAsync(p => p.SaleId == saleId, cancellationToken);
        var totalPaid = existingPayments.Sum(p => p.Amount);
        var remainingAmount = sale.TotalAmount - totalPaid;

        if (amount > remainingAmount)
        {
            throw new InvalidOperationException($"Payment amount exceeds remaining balance of ${remainingAmount:F2}");
        }

        // Create payment intent through gateway
        var intentResult = await _paymentGatewayService.CreatePaymentIntentAsync(
            amount,
            "USD",
            $"Payment for Sale #{sale.Id}",
            cancellationToken);

        if (!intentResult.Success || string.IsNullOrEmpty(intentResult.ClientSecret))
        {
            throw new InvalidOperationException(intentResult.ErrorMessage ?? "Failed to create payment intent");
        }

        return new PaymentIntentDto
        {
            ClientSecret = intentResult.ClientSecret,
            PaymentIntentId = intentResult.PaymentIntentId ?? string.Empty
        };
    }

    public async Task<PaymentDto> ConfirmPaymentAsync(string paymentIntentId, Guid saleId, Guid createdBy, CancellationToken cancellationToken = default)
    {
        var sale = await _unitOfWork.Sales.GetByIdAsync(saleId, cancellationToken);
        if (sale == null)
        {
            throw new KeyNotFoundException($"Sale with ID {saleId} not found");
        }

        // Verify payment intent
        var verifyResult = await _paymentGatewayService.ConfirmPaymentIntentAsync(paymentIntentId, cancellationToken);
        if (!verifyResult.Success)
        {
            throw new InvalidOperationException(verifyResult.ErrorMessage ?? "Payment verification failed");
        }

        // Get existing payments to calculate remaining amount
        var existingPayments = await _unitOfWork.Payments.FindAsync(p => p.SaleId == saleId, cancellationToken);
        var totalPaid = existingPayments.Sum(p => p.Amount);
        var remainingAmount = sale.TotalAmount - totalPaid;

        // Create payment record
        var paymentDto = new CreatePaymentDto
        {
            SaleId = saleId,
            Amount = remainingAmount, // Pay the full remaining amount
            PaymentMethod = Domain.Enums.PaymentMethod.PaymentGateway,
            PaymentGatewayTransactionId = paymentIntentId,
            Notes = "Payment processed via Stripe"
        };

        return await CreateAsync(paymentDto, createdBy, cancellationToken);
    }
}
