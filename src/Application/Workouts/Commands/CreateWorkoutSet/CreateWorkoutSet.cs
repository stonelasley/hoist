using Hoist.Application.Common.Interfaces;
using Hoist.Domain.Entities;
using Hoist.Domain.Enums;

namespace Hoist.Application.Workouts.Commands.CreateWorkoutSet;

public record CreateWorkoutSetCommand : IRequest<int>
{
    public int WorkoutId { get; init; }

    public int WorkoutExerciseId { get; init; }

    public decimal? Weight { get; init; }

    public int? Reps { get; init; }

    public int? Duration { get; init; }

    public decimal? Distance { get; init; }

    public decimal? Bodyweight { get; init; }

    public string? BandColor { get; init; }

    public string? WeightUnit { get; init; }

    public string? DistanceUnit { get; init; }
}

public class CreateWorkoutSetCommandHandler : IRequestHandler<CreateWorkoutSetCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public CreateWorkoutSetCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task<int> Handle(CreateWorkoutSetCommand request, CancellationToken cancellationToken)
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

        // Auto-assign position
        var maxPosition = workoutExercise.Sets.Any()
            ? workoutExercise.Sets.Max(s => s.Position)
            : 0;

        // Parse enum values
        WeightUnit? weightUnit = null;
        if (!string.IsNullOrEmpty(request.WeightUnit) && Enum.TryParse<WeightUnit>(request.WeightUnit, out var parsedWeightUnit))
        {
            weightUnit = parsedWeightUnit;
        }

        DistanceUnit? distanceUnit = null;
        if (!string.IsNullOrEmpty(request.DistanceUnit) && Enum.TryParse<DistanceUnit>(request.DistanceUnit, out var parsedDistanceUnit))
        {
            distanceUnit = parsedDistanceUnit;
        }

        // Create workout set
        var workoutSet = new WorkoutSet
        {
            WorkoutExerciseId = request.WorkoutExerciseId,
            Position = maxPosition + 1,
            Weight = request.Weight,
            Reps = request.Reps,
            Duration = request.Duration,
            Distance = request.Distance,
            Bodyweight = request.Bodyweight,
            BandColor = request.BandColor,
            WeightUnit = weightUnit,
            DistanceUnit = distanceUnit
        };

        _context.WorkoutSets.Add(workoutSet);
        await _context.SaveChangesAsync(cancellationToken);

        return workoutSet.Id;
    }
}

public class CreateWorkoutSetCommandValidator : AbstractValidator<CreateWorkoutSetCommand>
{
    public CreateWorkoutSetCommandValidator()
    {
        RuleFor(v => v.WorkoutId)
            .GreaterThan(0);

        RuleFor(v => v.WorkoutExerciseId)
            .GreaterThan(0);

        RuleFor(v => v.Weight)
            .GreaterThanOrEqualTo(0)
            .When(v => v.Weight.HasValue);

        RuleFor(v => v.Reps)
            .GreaterThanOrEqualTo(0)
            .When(v => v.Reps.HasValue);

        RuleFor(v => v.Duration)
            .GreaterThanOrEqualTo(0)
            .When(v => v.Duration.HasValue);

        RuleFor(v => v.Distance)
            .GreaterThanOrEqualTo(0)
            .When(v => v.Distance.HasValue);

        RuleFor(v => v.Bodyweight)
            .GreaterThanOrEqualTo(0)
            .When(v => v.Bodyweight.HasValue);

        RuleFor(v => v)
            .Must(v => v.Weight.HasValue || v.Reps.HasValue || v.Duration.HasValue ||
                       v.Distance.HasValue || v.Bodyweight.HasValue || !string.IsNullOrEmpty(v.BandColor))
            .WithMessage("At least one measurement field must be provided");
    }
}
