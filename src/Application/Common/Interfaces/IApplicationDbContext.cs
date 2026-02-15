using Hoist.Domain.Entities;

namespace Hoist.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }

    DbSet<ExerciseTemplate> ExerciseTemplates { get; }

    DbSet<WorkoutTemplate> WorkoutTemplates { get; }

    DbSet<WorkoutTemplateExercise> WorkoutTemplateExercises { get; }

    DbSet<Location> Locations { get; }

    DbSet<Workout> Workouts { get; }

    DbSet<WorkoutExercise> WorkoutExercises { get; }

    DbSet<WorkoutSet> WorkoutSets { get; }

    DbSet<Domain.Entities.UserPreferences> UserPreferences { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
