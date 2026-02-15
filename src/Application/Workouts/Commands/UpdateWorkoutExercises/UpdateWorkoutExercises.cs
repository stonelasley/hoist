using Hoist.Application.Common.Interfaces;
using Hoist.Domain.Entities;
using Hoist.Domain.Enums;

namespace Hoist.Application.Workouts.Commands.UpdateWorkoutExercises;

public record UpdateWorkoutExercisesCommand : IRequest
{
    public int WorkoutId { get; init; }

    public List<ExerciseInput> Exercises { get; init; } = new();
}

public record ExerciseInput
{
    public int ExerciseTemplateId { get; init; }
}

public class UpdateWorkoutExercisesCommandHandler : IRequestHandler<UpdateWorkoutExercisesCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateWorkoutExercisesCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(UpdateWorkoutExercisesCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id!;

        // Load workout and verify ownership and status
        var workout = await _context.Workouts
            .Include(w => w.Exercises)
                .ThenInclude(e => e.Sets)
            .Where(w => w.Id == request.WorkoutId && w.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        if (workout == null)
        {
            throw new Common.Exceptions.NotFoundException(nameof(Workout), request.WorkoutId);
        }

        if (workout.Status != WorkoutStatus.InProgress)
        {
            throw new Common.Exceptions.ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure("Status", "Only in-progress workouts can be modified")
            });
        }

        // Get exercise template IDs from request
        var exerciseTemplateIds = request.Exercises.Select(e => e.ExerciseTemplateId).ToList();

        // Load exercise templates
        var exerciseTemplates = await _context.ExerciseTemplates
            .Where(et => exerciseTemplateIds.Contains(et.Id))
            .ToDictionaryAsync(et => et.Id, cancellationToken);

        // Verify all exercise templates exist
        foreach (var exerciseTemplateId in exerciseTemplateIds)
        {
            if (!exerciseTemplates.ContainsKey(exerciseTemplateId))
            {
                throw new Common.Exceptions.NotFoundException(nameof(ExerciseTemplate), exerciseTemplateId);
            }
        }

        // Build list of exercises to keep and new exercises to create
        var updatedExercises = new List<WorkoutExercise>();
        var position = 1;

        foreach (var exerciseInput in request.Exercises)
        {
            // Check if exercise already exists in workout
            var existingExercise = workout.Exercises
                .FirstOrDefault(e => e.ExerciseTemplateId == exerciseInput.ExerciseTemplateId);

            if (existingExercise != null)
            {
                // Keep existing exercise with its sets, update position
                existingExercise.Position = position;
                updatedExercises.Add(existingExercise);
            }
            else
            {
                // Create new workout exercise
                var template = exerciseTemplates[exerciseInput.ExerciseTemplateId];
                var newExercise = new WorkoutExercise
                {
                    WorkoutId = workout.Id,
                    ExerciseTemplateId = exerciseInput.ExerciseTemplateId,
                    ExerciseName = template.Name,
                    ImplementType = template.ImplementType,
                    ExerciseType = template.ExerciseType,
                    Position = position
                };

                _context.WorkoutExercises.Add(newExercise);
                updatedExercises.Add(newExercise);
            }

            position++;
        }

        // Remove exercises not in the new list (cascade deletes sets)
        var exercisesToRemove = workout.Exercises
            .Where(e => !updatedExercises.Contains(e))
            .ToList();

        foreach (var exercise in exercisesToRemove)
        {
            _context.WorkoutExercises.Remove(exercise);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}

public class UpdateWorkoutExercisesCommandValidator : AbstractValidator<UpdateWorkoutExercisesCommand>
{
    public UpdateWorkoutExercisesCommandValidator()
    {
        RuleFor(v => v.WorkoutId)
            .GreaterThan(0);

        RuleFor(v => v.Exercises)
            .NotEmpty()
            .WithMessage("At least one exercise is required");

        RuleForEach(v => v.Exercises)
            .ChildRules(exercise =>
            {
                exercise.RuleFor(e => e.ExerciseTemplateId)
                    .GreaterThan(0);
            });
    }
}
