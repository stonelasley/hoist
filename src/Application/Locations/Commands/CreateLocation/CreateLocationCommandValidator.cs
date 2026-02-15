namespace Hoist.Application.Locations.Commands.CreateLocation;

public class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(v => v.InstagramHandle)
            .MaximumLength(30);

        RuleFor(v => v.Latitude)
            .InclusiveBetween(-90, 90)
            .When(v => v.Latitude.HasValue);

        RuleFor(v => v.Longitude)
            .InclusiveBetween(-180, 180)
            .When(v => v.Longitude.HasValue);

        RuleFor(v => v.Latitude)
            .NotNull()
            .When(v => v.Longitude.HasValue)
            .WithMessage("Latitude must be provided when Longitude is specified.");

        RuleFor(v => v.Longitude)
            .NotNull()
            .When(v => v.Latitude.HasValue)
            .WithMessage("Longitude must be provided when Latitude is specified.");

        RuleFor(v => v.Notes)
            .MaximumLength(2000);

        RuleFor(v => v.Address)
            .MaximumLength(500);
    }
}
