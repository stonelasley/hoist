using Hoist.Domain.Entities;

namespace Hoist.Application.Workouts.Queries.GetWorkout;

public class WorkoutDetailDto
{
    public int Id { get; init; }

    public string TemplateName { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public DateTimeOffset StartedAt { get; init; }

    public DateTimeOffset? EndedAt { get; init; }

    public string? Notes { get; init; }

    public int? Rating { get; init; }

    public int? LocationId { get; init; }

    public string? LocationName { get; init; }

    public List<WorkoutExerciseDto> Exercises { get; init; } = new();

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Workout, WorkoutDetailDto>()
                .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.Exercises, opt => opt.MapFrom(s => s.Exercises.OrderBy(e => e.Position)));
        }
    }
}
