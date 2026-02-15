namespace Hoist.Domain.Entities;

public class WorkoutTemplate : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Notes { get; set; }

    public string? Location { get; set; }

    public string UserId { get; set; } = string.Empty;

    public IList<WorkoutTemplateExercise> Exercises { get; set; } = new List<WorkoutTemplateExercise>();
}
