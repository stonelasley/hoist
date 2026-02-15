using Hoist.Application.Common.Interfaces;
using Hoist.Domain.Entities;
using Hoist.Domain.Enums;

namespace Hoist.Application.Workouts.Commands.DiscardWorkout;

public record DiscardWorkoutCommand : IRequest
{
    public int Id { get; init; }
}

public class DiscardWorkoutCommandHandler : IRequestHandler<DiscardWorkoutCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public DiscardWorkoutCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(DiscardWorkoutCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id!;

        // Load workout and verify ownership and status
        var workout = await _context.Workouts
            .Where(w => w.Id == request.Id && w.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (workout == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Workout), request.Id);
        }

        if (workout.Status != WorkoutStatus.InProgress)
        {
            throw new Common.Exceptions.ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Status", "Only in-progress workouts can be discarded")
            });
        }

        // Hard delete the workout (cascade deletes exercises and sets)
        _context.Workouts.Remove(workout);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class DiscardWorkoutCommandValidator : AbstractValidator<DiscardWorkoutCommand>
{
    public DiscardWorkoutCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0);
    }
}
