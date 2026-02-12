namespace PoultryDistributionSystem.Infrastructure.Services.Interfaces;

/// <summary>
/// Payment gateway service interface for payment processing
/// </summary>
public interface IPaymentGatewayService
{
    Task<PaymentGatewayResult> ProcessPaymentAsync(decimal amount, string currency, string description, CancellationToken cancellationToken = default);
    Task<PaymentGatewayResult> VerifyPaymentAsync(string transactionId, CancellationToken cancellationToken = default);
    Task<PaymentIntentResult> CreatePaymentIntentAsync(decimal amount, string currency, string description, CancellationToken cancellationToken = default);
    Task<PaymentGatewayResult> ConfirmPaymentIntentAsync(string paymentIntentId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Payment gateway result
/// </summary>
public class PaymentGatewayResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Payment intent result for Stripe
/// </summary>
public class PaymentIntentResult
{
    public bool Success { get; set; }
    public string? ClientSecret { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? ErrorMessage { get; set; }
}
