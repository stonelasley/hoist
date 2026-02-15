using Hoist.Domain.Enums;

namespace Hoist.Application.UserPreferences.Commands.UpsertUserPreferences;

public class UpsertUserPreferencesCommandValidator : AbstractValidator<UpsertUserPreferencesCommand>
{
    public UpsertUserPreferencesCommandValidator()
    {
        RuleFor(v => v.WeightUnit)
            .NotEmpty()
            .Must(BeValidWeightUnit)
            .WithMessage("WeightUnit must be either 'Lbs' or 'Kg'.");

        RuleFor(v => v.DistanceUnit)
            .NotEmpty()
            .Must(BeValidDistanceUnit)
            .WithMessage("DistanceUnit must be one of 'Miles', 'Kilometers', 'Meters', or 'Yards'.");

        RuleFor(v => v.Bodyweight)
            .GreaterThan(0)
            .When(v => v.Bodyweight.HasValue)
            .WithMessage("Bodyweight must be a positive number.");
    }

    private bool BeValidWeightUnit(string unit)
    {
        return Enum.TryParse<WeightUnit>(unit, out _);
    }

    private bool BeValidDistanceUnit(string unit)
    {
        return Enum.TryParse<DistanceUnit>(unit, out _);
    }
}
