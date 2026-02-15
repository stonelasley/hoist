using Hoist.Domain.Entities;

namespace Hoist.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }

    DbSet<ExerciseTemplate> ExerciseTemplates { get; }

    DbSet<WorkoutTemplate> WorkoutTemplates { get; }

    DbSet<WorkoutTemplateExercise> WorkoutTemplateExercises { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
