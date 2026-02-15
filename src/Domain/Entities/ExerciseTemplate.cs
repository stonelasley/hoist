namespace Hoist.Domain.Entities;

public class ExerciseTemplate : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public ImplementType ImplementType { get; set; }

    public ExerciseType ExerciseType { get; set; }

    public string? ImagePath { get; set; }

    public string? Model { get; set; }

    public bool IsDeleted { get; set; }

    public DateTimeOffset? DeletedAt { get; set; }

    public string UserId { get; set; } = string.Empty;

    public IList<WorkoutTemplateExercise> WorkoutTemplateExercises { get; set; } = new List<WorkoutTemplateExercise>();
}
