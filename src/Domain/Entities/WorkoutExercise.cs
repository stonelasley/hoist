namespace Hoist.Domain.Entities;

public class WorkoutExercise : BaseAuditableEntity
{
    public int WorkoutId { get; set; }

    public int? ExerciseTemplateId { get; set; }

    public string ExerciseName { get; set; } = string.Empty;

    public ImplementType ImplementType { get; set; }

    public ExerciseType ExerciseType { get; set; }

    public int Position { get; set; }

    public Workout Workout { get; set; } = null!;

    public ExerciseTemplate? ExerciseTemplate { get; set; }

    public IList<WorkoutSet> Sets { get; set; } = new List<WorkoutSet>();
}
