using Hoist.Application.Common.Interfaces;
using Hoist.Domain.Entities;
using Hoist.Domain.Enums;

namespace Hoist.Application.Workouts.Commands.UpdateWorkoutSet;

public record UpdateWorkoutSetCommand : IRequest
{
    public int WorkoutId { get; init; }

    public int WorkoutExerciseId { get; init; }

    public int SetId { get; init; }

    public decimal? Weight { get; init; }

    public int? Reps { get; init; }

    public int? Duration { get; init; }

    public decimal? Distance { get; init; }

    public decimal? Bodyweight { get; init; }

    public string? BandColor { get; init; }

    public string? WeightUnit { get; init; }

    public string? DistanceUnit { get; init; }
}

public class UpdateWorkoutSetCommandHandler : IRequestHandler<UpdateWorkoutSetCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateWorkoutSetCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(UpdateWorkoutSetCommand request, CancellationToken cancellationToken)
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

        // Update fields
        workoutSet.Weight = request.Weight;
        workoutSet.Reps = request.Reps;
        workoutSet.Duration = request.Duration;
        workoutSet.Distance = request.Distance;
        workoutSet.Bodyweight = request.Bodyweight;
        workoutSet.BandColor = request.BandColor;
        workoutSet.WeightUnit = weightUnit;
        workoutSet.DistanceUnit = distanceUnit;

        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class UpdateWorkoutSetCommandValidator : AbstractValidator<UpdateWorkoutSetCommand>
{
    public UpdateWorkoutSetCommandValidator()
    {
        RuleFor(v => v.WorkoutId)
            .GreaterThan(0);

        RuleFor(v => v.WorkoutExerciseId)
            .GreaterThan(0);

        RuleFor(v => v.SetId)
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
