using Hoist.Domain.Entities;

namespace Hoist.Application.Workouts.Queries.GetRecentWorkouts;

public class WorkoutBriefDto
{
    public int Id { get; init; }

    public string TemplateName { get; init; } = string.Empty;

    public DateTimeOffset StartedAt { get; init; }

    public DateTimeOffset? EndedAt { get; init; }

    public int? Rating { get; init; }

    public string? LocationName { get; init; }

    private class Mapping : Profile
    {
        public Mapping()
        {
            CreateMap<Workout, WorkoutBriefDto>();
        }
    }
}
