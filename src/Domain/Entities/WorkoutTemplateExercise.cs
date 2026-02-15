namespace Hoist.Domain.Entities;

public class WorkoutTemplateExercise : BaseEntity
{
    public int WorkoutTemplateId { get; set; }

    public int ExerciseTemplateId { get; set; }

    public int Position { get; set; }

    public WorkoutTemplate WorkoutTemplate { get; set; } = null!;

    public ExerciseTemplate ExerciseTemplate { get; set; } = null!;
}
