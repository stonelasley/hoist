using Hoist.Application.Common.Interfaces;

namespace Hoist.Application.WorkoutTemplates.Commands.UpdateWorkoutTemplateExercises;

public record UpdateWorkoutTemplateExercisesCommand : IRequest
{
    public int WorkoutTemplateId { get; init; }
    public List<UpdateWorkoutTemplateExerciseItem> Exercises { get; init; } = [];
}

public record UpdateWorkoutTemplateExerciseItem
{
    public int ExerciseTemplateId { get; init; }
}

public class UpdateWorkoutTemplateExercisesCommandHandler : IRequestHandler<UpdateWorkoutTemplateExercisesCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _user;

    public UpdateWorkoutTemplateExercisesCommandHandler(IApplicationDbContext context, IUser user)
    {
        _context = context;
        _user = user;
    }

    public async Task Handle(UpdateWorkoutTemplateExercisesCommand request, CancellationToken cancellationToken)
    {
        var userId = _user.Id;

        // Load workout template and verify ownership
        var workoutTemplate = await _context.WorkoutTemplates
            .FirstOrDefaultAsync(x => x.Id == request.WorkoutTemplateId && x.UserId == userId, cancellationToken);

        Guard.Against.NotFound(request.WorkoutTemplateId, workoutTemplate);

        // Validate all exercise templates exist and are owned by the user
        var exerciseTemplateIds = request.Exercises
            .Select(e => e.ExerciseTemplateId)
            .Distinct()
            .ToList();

        if (exerciseTemplateIds.Any())
        {
            var validExerciseIds = await _context.ExerciseTemplates
                .Where(e => exerciseTemplateIds.Contains(e.Id) && e.UserId == userId && !e.IsDeleted)
                .Select(e => e.Id)
                .ToListAsync(cancellationToken);

            var invalidExerciseIds = exerciseTemplateIds.Except(validExerciseIds).ToList();
            foreach (var invalidId in invalidExerciseIds)
            {
                Guard.Against.NotFound(invalidId, (object?)null);
            }
        }

        // Remove all existing workout template exercises
        var existingExercises = await _context.WorkoutTemplateExercises
            .Where(wte => wte.WorkoutTemplateId == request.WorkoutTemplateId)
            .ToListAsync(cancellationToken);

        _context.WorkoutTemplateExercises.RemoveRange(existingExercises);

        // Create new workout template exercises with positions
        var newExercises = request.Exercises
            .Select((exercise, index) => new Domain.Entities.WorkoutTemplateExercise
            {
                WorkoutTemplateId = request.WorkoutTemplateId,
                ExerciseTemplateId = exercise.ExerciseTemplateId,
                Position = index + 1
            })
            .ToList();

        _context.WorkoutTemplateExercises.AddRange(newExercises);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
