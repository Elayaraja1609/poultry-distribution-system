using FluentValidation;
using PoultryDistributionSystem.Application.DTOs.Chicken;

namespace PoultryDistributionSystem.Application.Validators.Chicken;

/// <summary>
/// Validator for CreateChickenDto
/// </summary>
public class CreateChickenValidator : AbstractValidator<CreateChickenDto>
{
    public CreateChickenValidator()
    {
        RuleFor(x => x.BatchNumber)
            .NotEmpty().WithMessage("Batch number is required")
            .MaximumLength(50).WithMessage("Batch number must not exceed 50 characters");

        RuleFor(x => x.SupplierId)
            .NotEmpty().WithMessage("Supplier is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.WeightKg)
            .GreaterThan(0).WithMessage("Weight must be greater than 0");

        RuleFor(x => x.PurchaseDate)
            .NotEmpty().WithMessage("Purchase date is required");
    }
}
