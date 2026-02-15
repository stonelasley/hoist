namespace Hoist.Domain.Entities;

public class WorkoutSet : BaseAuditableEntity
{
    public int WorkoutExerciseId { get; set; }

    public int Position { get; set; }

    public decimal? Weight { get; set; }

    public int? Reps { get; set; }

    public int? Duration { get; set; }

    public decimal? Distance { get; set; }

    public decimal? Bodyweight { get; set; }

    public string? BandColor { get; set; }

    public WeightUnit? WeightUnit { get; set; }

    public DistanceUnit? DistanceUnit { get; set; }

    public WorkoutExercise WorkoutExercise { get; set; } = null!;
}
