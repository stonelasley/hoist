namespace Hoist.Application.WorkoutTemplates.Commands.CreateWorkoutTemplate;

public class CreateWorkoutTemplateCommandValidator : AbstractValidator<CreateWorkoutTemplateCommand>
{
    public CreateWorkoutTemplateCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(v => v.Notes)
            .MaximumLength(2000);
    }
}
