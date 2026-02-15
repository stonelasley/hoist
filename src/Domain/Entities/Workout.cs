namespace Hoist.Domain.Entities;

public class Workout : BaseAuditableEntity
{
    public int? WorkoutTemplateId { get; set; }

    public string TemplateName { get; set; } = string.Empty;

    public WorkoutStatus Status { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset? EndedAt { get; set; }

    public string? Notes { get; set; }

    public int? Rating { get; set; }

    public int? LocationId { get; set; }

    public string? LocationName { get; set; }

    public string UserId { get; set; } = string.Empty;

    public WorkoutTemplate? WorkoutTemplate { get; set; }

    public Location? Location { get; set; }

    public IList<WorkoutExercise> Exercises { get; set; } = new List<WorkoutExercise>();
}
