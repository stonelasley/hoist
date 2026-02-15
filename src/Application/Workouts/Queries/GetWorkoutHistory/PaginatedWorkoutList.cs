using Hoist.Application.Workouts.Queries.GetRecentWorkouts;

namespace Hoist.Application.Workouts.Queries.GetWorkoutHistory;

public class PaginatedWorkoutList
{
    public List<WorkoutBriefDto> Items { get; init; } = new();
    public string? NextCursor { get; init; }
}
