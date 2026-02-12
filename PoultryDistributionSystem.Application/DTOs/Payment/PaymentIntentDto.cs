namespace PoultryDistributionSystem.Application.DTOs.Payment;

/// <summary>
/// Payment intent DTO for Stripe
/// </summary>
public class PaymentIntentDto
{
    public string ClientSecret { get; set; } = string.Empty;
    public string PaymentIntentId { get; set; } = string.Empty;
}
