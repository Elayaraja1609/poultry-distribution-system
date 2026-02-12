using PoultryDistributionSystem.Application.Interfaces;

namespace PoultryDistributionSystem.Infrastructure.Services;

/// <summary>
/// Adapter to bridge Infrastructure.IPaymentGatewayService to Application.IPaymentGatewayService
/// </summary>
public class PaymentGatewayServiceAdapter : Application.Interfaces.IPaymentGatewayService
{
    private readonly Services.Interfaces.IPaymentGatewayService _paymentGatewayService;

    public PaymentGatewayServiceAdapter(Services.Interfaces.IPaymentGatewayService paymentGatewayService)
    {
        _paymentGatewayService = paymentGatewayService ?? throw new ArgumentNullException(nameof(paymentGatewayService));
    }

    public async Task<PaymentGatewayResult> ProcessPaymentAsync(decimal amount, string currency, string description, CancellationToken cancellationToken = default)
    {
        var result = await _paymentGatewayService.ProcessPaymentAsync(amount, currency, description, cancellationToken);
        return new PaymentGatewayResult
        {
            Success = result.Success,
            TransactionId = result.TransactionId,
            ErrorMessage = result.ErrorMessage
        };
    }

    public async Task<PaymentGatewayResult> VerifyPaymentAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        var result = await _paymentGatewayService.VerifyPaymentAsync(transactionId, cancellationToken);
        return new PaymentGatewayResult
        {
            Success = result.Success,
            TransactionId = result.TransactionId,
            ErrorMessage = result.ErrorMessage
        };
    }

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(decimal amount, string currency, string description, CancellationToken cancellationToken = default)
    {
        var result = await _paymentGatewayService.CreatePaymentIntentAsync(amount, currency, description, cancellationToken);
        return new PaymentIntentResult
        {
            Success = result.Success,
            ClientSecret = result.ClientSecret,
            PaymentIntentId = result.PaymentIntentId,
            ErrorMessage = result.ErrorMessage
        };
    }

    public async Task<PaymentGatewayResult> ConfirmPaymentIntentAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        var result = await _paymentGatewayService.ConfirmPaymentIntentAsync(paymentIntentId, cancellationToken);
        return new PaymentGatewayResult
        {
            Success = result.Success,
            TransactionId = result.TransactionId,
            ErrorMessage = result.ErrorMessage
        };
    }
}
