using FluentValidation;
using PoultryDistributionSystem.Application.DTOs.Farm;

namespace PoultryDistributionSystem.Application.Validators.Farm;

/// <summary>
/// Validator for UpdateFarmDto
/// </summary>
public class UpdateFarmValidator : AbstractValidator<UpdateFarmDto>
{
    public UpdateFarmValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Farm name is required")
            .MaximumLength(200).WithMessage("Farm name must not exceed 200 characters");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Location is required")
            .MaximumLength(200).WithMessage("Location must not exceed 200 characters");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Capacity must be greater than 0");

        RuleFor(x => x.ManagerName)
            .NotEmpty().WithMessage("Manager name is required")
            .MaximumLength(200).WithMessage("Manager name must not exceed 200 characters");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required")
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters");
    }
}
