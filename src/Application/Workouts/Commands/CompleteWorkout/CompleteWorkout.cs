using Hoist.Application.Common.Interfaces;
using Hoist.Domain.Enums;

namespace Hoist.Application.Workouts.Commands.CompleteWorkout;

public record CompleteWorkoutCommand : IRequest
{
    public int Id { get; init; }
    public string? Notes { get; init; }
    public int? Rating { get; init; }
    public DateTimeOffset? StartedAt { get; init; }
    public DateTimeOffset? EndedAt { get; init; }
}

public class CompleteWorkoutCommandHandler : IRequestHandler<CompleteWorkoutCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CompleteWorkoutCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(CompleteWorkoutCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id!;

        var workout = await _context.Workouts
            .Where(w => w.Id == request.Id && w.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (workout == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Domain.Entities.Workout), request.Id);
        }

        if (workout.Status != WorkoutStatus.InProgress)
        {
            throw new Common.Exceptions.ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Status", "Workout is not in progress")
            });
        }

        workout.Status = WorkoutStatus.Completed;
        workout.EndedAt = request.EndedAt ?? DateTimeOffset.UtcNow;

        if (request.Notes != null)
        {
            workout.Notes = request.Notes;
        }

        if (request.Rating.HasValue)
        {
            workout.Rating = request.Rating.Value;
        }

        if (request.StartedAt.HasValue)
        {
            workout.StartedAt = request.StartedAt.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class CompleteWorkoutCommandValidator : AbstractValidator<CompleteWorkoutCommand>
{
    public CompleteWorkoutCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0);

        RuleFor(v => v.Rating)
            .InclusiveBetween(1, 5)
            .When(v => v.Rating.HasValue);

        RuleFor(v => v.Notes)
            .MaximumLength(2000);

        RuleFor(v => v)
            .Must(v => !v.StartedAt.HasValue || !v.EndedAt.HasValue || v.EndedAt > v.StartedAt)
            .WithMessage("EndedAt must be after StartedAt")
            .When(v => v.StartedAt.HasValue && v.EndedAt.HasValue);
    }
}
