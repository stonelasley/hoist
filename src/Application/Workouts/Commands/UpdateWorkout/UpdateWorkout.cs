using Hoist.Application.Common.Interfaces;

namespace Hoist.Application.Workouts.Commands.UpdateWorkout;

public record UpdateWorkoutCommand : IRequest
{
    public int Id { get; init; }
    public int? LocationId { get; init; }
    public string? Notes { get; init; }
    public int? Rating { get; init; }
    public DateTimeOffset? StartedAt { get; init; }
    public DateTimeOffset? EndedAt { get; init; }
}

public class UpdateWorkoutCommandHandler : IRequestHandler<UpdateWorkoutCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateWorkoutCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(UpdateWorkoutCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id!;

        var workout = await _context.Workouts
            .Where(w => w.Id == request.Id && w.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (workout == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Domain.Entities.Workout), request.Id);
        }

        // Update LocationId and snapshot LocationName if changed
        if (request.LocationId.HasValue)
        {
            if (request.LocationId.Value != workout.LocationId)
            {
                var location = await _context.Locations
                    .Where(l => l.Id == request.LocationId.Value && l.UserId == userId && !l.IsDeleted)
                    .FirstOrDefaultAsync(cancellationToken);

                if (location == null)
                {
                    throw new Common.Exceptions.NotFoundException(nameof(Domain.Entities.Location), request.LocationId.Value);
                }

                workout.LocationId = request.LocationId.Value;
                workout.LocationName = location.Name;
            }
        }

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

        if (request.EndedAt.HasValue)
        {
            workout.EndedAt = request.EndedAt.Value;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class UpdateWorkoutCommandValidator : AbstractValidator<UpdateWorkoutCommand>
{
    public UpdateWorkoutCommandValidator()
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
