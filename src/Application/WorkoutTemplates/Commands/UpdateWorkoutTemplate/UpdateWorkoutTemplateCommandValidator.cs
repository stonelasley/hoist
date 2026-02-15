namespace Hoist.Application.WorkoutTemplates.Commands.UpdateWorkoutTemplate;

public class UpdateWorkoutTemplateCommandValidator : AbstractValidator<UpdateWorkoutTemplateCommand>
{
    public UpdateWorkoutTemplateCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(v => v.Notes)
            .MaximumLength(2000);

        RuleFor(v => v.Location)
            .MaximumLength(200);
    }
}
