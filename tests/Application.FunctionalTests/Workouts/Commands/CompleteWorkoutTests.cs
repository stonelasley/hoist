using Hoist.Application.Common.Exceptions;
using Hoist.Application.Workouts.Commands.CompleteWorkout;
using Hoist.Application.Workouts.Commands.StartWorkout;
using Hoist.Domain.Entities;
using Hoist.Domain.Enums;

namespace Hoist.Application.FunctionalTests.Workouts.Commands;

using static Testing;

public class CompleteWorkoutTests : BaseTestFixture
{
    [Test]
    public async Task ShouldCompleteInProgressWorkout()
    {
        var userId = await RunAsDefaultUserAsync();

        // Create exercise template and workout template
        var exercise = new ExerciseTemplate
        {
            Name = "Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            UserId = userId
        };

        await AddAsync(exercise);

        var workoutTemplate = new WorkoutTemplate
        {
            Name = "Push Day",
            UserId = userId
        };

        await AddAsync(workoutTemplate);

        var templateExercise = new WorkoutTemplateExercise
        {
            WorkoutTemplateId = workoutTemplate.Id,
            ExerciseTemplateId = exercise.Id,
            Position = 1
        };

        await AddAsync(templateExercise);

        // Start workout
        var workoutId = await SendAsync(new StartWorkoutCommand { WorkoutTemplateId = workoutTemplate.Id });

        // Complete workout
        var command = new CompleteWorkoutCommand { Id = workoutId };
        await SendAsync(command);

        // Verify workout was completed
        var workout = await FindAsync<Workout>(workoutId);
        workout.ShouldNotBeNull();
        workout!.Status.ShouldBe(WorkoutStatus.Completed);
        workout.EndedAt.ShouldNotBeNull();
        workout.EndedAt!.Value.ShouldBe(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Test]
    public async Task ShouldCompleteWithNotesAndRating()
    {
        var userId = await RunAsDefaultUserAsync();

        // Create exercise template and workout template
        var exercise = new ExerciseTemplate
        {
            Name = "Squat",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            UserId = userId
        };

        await AddAsync(exercise);

        var workoutTemplate = new WorkoutTemplate
        {
            Name = "Leg Day",
            UserId = userId
        };

        await AddAsync(workoutTemplate);

        var templateExercise = new WorkoutTemplateExercise
        {
            WorkoutTemplateId = workoutTemplate.Id,
            ExerciseTemplateId = exercise.Id,
            Position = 1
        };

        await AddAsync(templateExercise);

        // Start workout
        var workoutId = await SendAsync(new StartWorkoutCommand { WorkoutTemplateId = workoutTemplate.Id });

        // Complete workout with notes and rating
        var command = new CompleteWorkoutCommand
        {
            Id = workoutId,
            Notes = "Great workout!",
            Rating = 5
        };

        await SendAsync(command);

        // Verify workout was completed with notes and rating
        var workout = await FindAsync<Workout>(workoutId);
        workout.ShouldNotBeNull();
        workout!.Status.ShouldBe(WorkoutStatus.Completed);
        workout.Notes.ShouldBe("Great workout!");
        workout.Rating.ShouldBe(5);
    }

    [Test]
    public async Task ShouldRejectAlreadyCompletedWorkout()
    {
        var userId = await RunAsDefaultUserAsync();

        // Create exercise template and workout template
        var exercise = new ExerciseTemplate
        {
            Name = "Deadlift",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            UserId = userId
        };

        await AddAsync(exercise);

        var workoutTemplate = new WorkoutTemplate
        {
            Name = "Back Day",
            UserId = userId
        };

        await AddAsync(workoutTemplate);

        var templateExercise = new WorkoutTemplateExercise
        {
            WorkoutTemplateId = workoutTemplate.Id,
            ExerciseTemplateId = exercise.Id,
            Position = 1
        };

        await AddAsync(templateExercise);

        // Start and complete workout
        var workoutId = await SendAsync(new StartWorkoutCommand { WorkoutTemplateId = workoutTemplate.Id });
        await SendAsync(new CompleteWorkoutCommand { Id = workoutId });

        // Try to complete again - should fail
        await Should.ThrowAsync<ValidationException>(() => SendAsync(new CompleteWorkoutCommand { Id = workoutId }));
    }

    [Test]
    public async Task ShouldDefaultEndedAtToNow()
    {
        var userId = await RunAsDefaultUserAsync();

        // Create exercise template and workout template
        var exercise = new ExerciseTemplate
        {
            Name = "Overhead Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            UserId = userId
        };

        await AddAsync(exercise);

        var workoutTemplate = new WorkoutTemplate
        {
            Name = "Shoulder Day",
            UserId = userId
        };

        await AddAsync(workoutTemplate);

        var templateExercise = new WorkoutTemplateExercise
        {
            WorkoutTemplateId = workoutTemplate.Id,
            ExerciseTemplateId = exercise.Id,
            Position = 1
        };

        await AddAsync(templateExercise);

        // Start workout
        var workoutId = await SendAsync(new StartWorkoutCommand { WorkoutTemplateId = workoutTemplate.Id });

        // Complete workout without providing EndedAt
        var command = new CompleteWorkoutCommand { Id = workoutId };
        await SendAsync(command);

        // Verify EndedAt was set to current time
        var workout = await FindAsync<Workout>(workoutId);
        workout.ShouldNotBeNull();
        workout!.EndedAt.ShouldNotBeNull();
        workout.EndedAt!.Value.ShouldBe(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));
    }
}
