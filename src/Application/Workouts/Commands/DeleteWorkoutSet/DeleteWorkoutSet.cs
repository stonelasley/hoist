using Hoist.Application.Common.Interfaces;
using Hoist.Domain.Entities;

namespace Hoist.Application.Workouts.Commands.DeleteWorkoutSet;

public record DeleteWorkoutSetCommand : IRequest
{
    public int WorkoutId { get; init; }

    public int WorkoutExerciseId { get; init; }

    public int SetId { get; init; }
}

public class DeleteWorkoutSetCommandHandler : IRequestHandler<DeleteWorkoutSetCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public DeleteWorkoutSetCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(DeleteWorkoutSetCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id!;

        // Load workout and verify ownership
        var workout = await _context.Workouts
            .Where(w => w.Id == request.WorkoutId && w.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (workout == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Workout), request.WorkoutId);
        }

        // Load workout exercise and verify it belongs to the workout
        var workoutExercise = await _context.WorkoutExercises
            .Include(we => we.Sets)
            .Where(we => we.Id == request.WorkoutExerciseId && we.WorkoutId == request.WorkoutId)
            .FirstOrDefaultAsync(cancellationToken);

        if (workoutExercise == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(WorkoutExercise), request.WorkoutExerciseId);
        }

        // Load workout set and verify it belongs to the exercise
        var workoutSet = await _context.WorkoutSets
            .Where(ws => ws.Id == request.SetId && ws.WorkoutExerciseId == request.WorkoutExerciseId)
            .FirstOrDefaultAsync(cancellationToken);

        if (workoutSet == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(WorkoutSet), request.SetId);
        }

        var deletedPosition = workoutSet.Position;

        // Delete the set
        _context.WorkoutSets.Remove(workoutSet);

        // Resequence remaining sets
        var remainingSets = workoutExercise.Sets
            .Where(s => s.Id != request.SetId && s.Position > deletedPosition)
            .ToList();

        foreach (var set in remainingSets)
        {
            set.Position--;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class DeleteWorkoutSetCommandValidator : AbstractValidator<DeleteWorkoutSetCommand>
{
    public DeleteWorkoutSetCommandValidator()
    {
        RuleFor(v => v.WorkoutId)
            .GreaterThan(0);

        RuleFor(v => v.WorkoutExerciseId)
            .GreaterThan(0);

        RuleFor(v => v.SetId)
            .GreaterThan(0);
    }
}
