using PoultryDistributionSystem.Application.Common;
using PoultryDistributionSystem.Application.DTOs.Payment;

namespace PoultryDistributionSystem.Application.Interfaces;

/// <summary>
/// Payment service interface
/// </summary>
public interface IPaymentService
{
    Task<PaymentDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<PaymentDto>> GetBySaleIdAsync(Guid saleId, CancellationToken cancellationToken = default);
    Task<PagedResult<PaymentDto>> GetMyPaymentsAsync(Guid userId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<PaymentDto> CreateAsync(CreatePaymentDto dto, Guid createdBy, CancellationToken cancellationToken = default);
    Task<PaymentDto> ProcessPaymentGatewayAsync(CreatePaymentDto dto, Guid createdBy, CancellationToken cancellationToken = default);
    Task<PaymentIntentDto> CreatePaymentIntentAsync(decimal amount, Guid saleId, CancellationToken cancellationToken = default);
    Task<PaymentDto> ConfirmPaymentAsync(string paymentIntentId, Guid saleId, Guid createdBy, CancellationToken cancellationToken = default);
}
