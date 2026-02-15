using Hoist.Application.Common.Interfaces;
using Hoist.Domain.Entities;
using Hoist.Domain.Enums;

namespace Hoist.Application.Workouts.Commands.StartWorkout;

public record StartWorkoutCommand : IRequest<int>
{
    public int WorkoutTemplateId { get; init; }
}

public class StartWorkoutCommandHandler : IRequestHandler<StartWorkoutCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public StartWorkoutCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<int> Handle(StartWorkoutCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id!;

        // Check if user already has a workout in progress
        var existingInProgressWorkout = await _context.Workouts
            .Where(w => w.UserId == userId && w.Status == WorkoutStatus.InProgress)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingInProgressWorkout != null)
        {
            throw new Common.Exceptions.ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("", "You already have a workout in progress")
            });
        }

        // Load workout template with exercises and exercise templates
        var template = await _context.WorkoutTemplates
            .Include(t => t.Exercises)
                .ThenInclude(e => e.ExerciseTemplate)
            .Include(t => t.Location)
            .Where(t => t.Id == request.WorkoutTemplateId && t.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (template == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(WorkoutTemplate), request.WorkoutTemplateId);
        }

        // Create workout entity
        var workout = new Workout
        {
            TemplateName = template.Name,
            Status = WorkoutStatus.InProgress,
            StartedAt = DateTimeOffset.UtcNow,
            LocationId = template.LocationId,
            LocationName = template.Location?.Name,
            UserId = userId,
            WorkoutTemplateId = template.Id
        };

        // Create workout exercises from template
        foreach (var templateExercise in template.Exercises.OrderBy(e => e.Position))
        {
            var workoutExercise = new WorkoutExercise
            {
                ExerciseName = templateExercise.ExerciseTemplate.Name,
                ImplementType = templateExercise.ExerciseTemplate.ImplementType,
                ExerciseType = templateExercise.ExerciseTemplate.ExerciseType,
                Position = templateExercise.Position,
                ExerciseTemplateId = templateExercise.ExerciseTemplateId
            };

            workout.Exercises.Add(workoutExercise);
        }

        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync(cancellationToken);

        return workout.Id;
    }
}

public class StartWorkoutCommandValidator : AbstractValidator<StartWorkoutCommand>
{
    public StartWorkoutCommandValidator()
    {
        RuleFor(v => v.WorkoutTemplateId)
            .GreaterThan(0);
    }
}
