namespace Hoist.Application.ExerciseTemplates.Commands.UploadExerciseImage;

public class UploadExerciseImageCommandValidator : AbstractValidator<UploadExerciseImageCommand>
{
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];

    public UploadExerciseImageCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0);

        RuleFor(v => v.FileStream)
            .NotNull();

        RuleFor(v => v.ContentType)
            .NotEmpty()
            .Must(ct => AllowedContentTypes.Contains(ct))
                .WithMessage("Content type must be image/jpeg, image/png, or image/webp.");
    }
}
