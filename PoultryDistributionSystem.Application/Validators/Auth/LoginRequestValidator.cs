using FluentValidation;
using PoultryDistributionSystem.Application.DTOs.Auth;

namespace PoultryDistributionSystem.Application.Validators.Auth;

/// <summary>
/// Validator for LoginRequestDto
/// </summary>
public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UsernameOrEmail)
            .NotEmpty().WithMessage("Username or email is required")
            .MaximumLength(100).WithMessage("Username or email must not exceed 100 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");
    }
}
