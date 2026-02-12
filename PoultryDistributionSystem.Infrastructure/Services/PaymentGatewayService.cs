using Microsoft.Extensions.Configuration;
using PoultryDistributionSystem.Infrastructure.Services.Interfaces;
using Stripe;

namespace PoultryDistributionSystem.Infrastructure.Services;

/// <summary>
/// Payment gateway service implementation using Stripe
/// </summary>
public class PaymentGatewayService : IPaymentGatewayService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;

    public PaymentGatewayService(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _secretKey = _configuration["Stripe:SecretKey"] ?? throw new InvalidOperationException("Stripe SecretKey not configured");
        StripeConfiguration.ApiKey = _secretKey;
    }

    public async Task<PaymentGatewayResult> ProcessPaymentAsync(decimal amount, string currency, string description, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Convert to cents
                Currency = currency.ToLower(),
                Description = description,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options, cancellationToken: cancellationToken);

            if (paymentIntent.Status == "succeeded")
            {
                return new PaymentGatewayResult
                {
                    Success = true,
                    TransactionId = paymentIntent.Id,
                    ErrorMessage = null
                };
            }

            return new PaymentGatewayResult
            {
                Success = false,
                TransactionId = paymentIntent.Id,
                ErrorMessage = $"Payment intent created but not succeeded. Status: {paymentIntent.Status}"
            };
        }
        catch (StripeException ex)
        {
            return new PaymentGatewayResult
            {
                Success = false,
                TransactionId = null,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<PaymentGatewayResult> VerifyPaymentAsync(string transactionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(transactionId, cancellationToken: cancellationToken);

            return new PaymentGatewayResult
            {
                Success = paymentIntent.Status == "succeeded",
                TransactionId = paymentIntent.Id,
                ErrorMessage = paymentIntent.Status == "succeeded" ? null : $"Payment status: {paymentIntent.Status}"
            };
        }
        catch (StripeException ex)
        {
            return new PaymentGatewayResult
            {
                Success = false,
                TransactionId = transactionId,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<PaymentIntentResult> CreatePaymentIntentAsync(decimal amount, string currency, string description, CancellationToken cancellationToken = default)
    {
        try
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Convert to cents
                Currency = currency.ToLower(),
                Description = description,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options, cancellationToken: cancellationToken);

            return new PaymentIntentResult
            {
                Success = true,
                ClientSecret = paymentIntent.ClientSecret,
                PaymentIntentId = paymentIntent.Id,
                ErrorMessage = null
            };
        }
        catch (StripeException ex)
        {
            return new PaymentIntentResult
            {
                Success = false,
                ClientSecret = null,
                PaymentIntentId = null,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<PaymentGatewayResult> ConfirmPaymentIntentAsync(string paymentIntentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var service = new PaymentIntentService();
            var paymentIntent = await service.GetAsync(paymentIntentId, cancellationToken: cancellationToken);

            if (paymentIntent.Status == "succeeded")
            {
                return new PaymentGatewayResult
                {
                    Success = true,
                    TransactionId = paymentIntent.Id,
                    ErrorMessage = null
                };
            }

            return new PaymentGatewayResult
            {
                Success = false,
                TransactionId = paymentIntent.Id,
                ErrorMessage = $"Payment not succeeded. Status: {paymentIntent.Status}"
            };
        }
        catch (StripeException ex)
        {
            return new PaymentGatewayResult
            {
                Success = false,
                TransactionId = paymentIntentId,
                ErrorMessage = ex.Message
            };
        }
    }
}
