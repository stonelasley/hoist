namespace Hoist.Application.WorkoutTemplates.Commands.UpdateWorkoutTemplateExercises;

public class UpdateWorkoutTemplateExercisesCommandValidator : AbstractValidator<UpdateWorkoutTemplateExercisesCommand>
{
    public UpdateWorkoutTemplateExercisesCommandValidator()
    {
        RuleFor(v => v.WorkoutTemplateId)
            .GreaterThan(0);

        RuleForEach(v => v.Exercises)
            .ChildRules(exercise =>
            {
                exercise.RuleFor(e => e.ExerciseTemplateId)
                    .GreaterThan(0);
            });
    }
}
