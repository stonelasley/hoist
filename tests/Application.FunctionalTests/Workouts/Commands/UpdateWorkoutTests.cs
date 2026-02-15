using Hoist.Application.Common.Exceptions;
using Hoist.Application.Workouts.Commands.StartWorkout;
using Hoist.Application.Workouts.Commands.UpdateWorkout;
using Hoist.Domain.Entities;
using Hoist.Domain.Enums;

namespace Hoist.Application.FunctionalTests.Workouts.Commands;

using static Testing;

public class UpdateWorkoutTests : BaseTestFixture
{
    [Test]
    public async Task ShouldUpdateNotesAndRating()
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

        // Update notes and rating
        var command = new UpdateWorkoutCommand
        {
            Id = workoutId,
            Notes = "Updated notes",
            Rating = 4
        };

        await SendAsync(command);

        // Verify workout was updated
        var workout = await FindAsync<Workout>(workoutId);
        workout.ShouldNotBeNull();
        workout!.Notes.ShouldBe("Updated notes");
        workout.Rating.ShouldBe(4);
    }

    [Test]
    public async Task ShouldUpdateTimes()
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

        // Update times
        var newStartedAt = DateTimeOffset.UtcNow.AddHours(-2);
        var newEndedAt = DateTimeOffset.UtcNow.AddHours(-1);

        var command = new UpdateWorkoutCommand
        {
            Id = workoutId,
            StartedAt = newStartedAt,
            EndedAt = newEndedAt
        };

        await SendAsync(command);

        // Verify times were updated
        var workout = await FindAsync<Workout>(workoutId);
        workout.ShouldNotBeNull();
        workout!.StartedAt.ShouldBe(newStartedAt, TimeSpan.FromSeconds(1));
        workout.EndedAt!.Value.ShouldBe(newEndedAt, TimeSpan.FromSeconds(1));
    }

    [Test]
    public async Task ShouldUpdateLocationAndSnapshotName()
    {
        var userId = await RunAsDefaultUserAsync();

        // Create location
        var location = new Location
        {
            Name = "Gold's Gym",
            Address = "123 Main St",
            UserId = userId
        };

        await AddAsync(location);

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

        // Start workout
        var workoutId = await SendAsync(new StartWorkoutCommand { WorkoutTemplateId = workoutTemplate.Id });

        // Update location
        var command = new UpdateWorkoutCommand
        {
            Id = workoutId,
            LocationId = location.Id
        };

        await SendAsync(command);

        // Verify location was updated and name was snapshotted
        var workout = await FindAsync<Workout>(workoutId);
        workout.ShouldNotBeNull();
        workout!.LocationId.ShouldBe(location.Id);
        workout.LocationName.ShouldBe("Gold's Gym");
    }

    [Test]
    public async Task ShouldRejectForNonOwnedWorkout()
    {
        var userId1 = await RunAsDefaultUserAsync();

        // Create exercise template and workout template for user 1
        var exercise = new ExerciseTemplate
        {
            Name = "Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            UserId = userId1
        };

        await AddAsync(exercise);

        var workoutTemplate = new WorkoutTemplate
        {
            Name = "Push Day",
            UserId = userId1
        };

        await AddAsync(workoutTemplate);

        var templateExercise = new WorkoutTemplateExercise
        {
            WorkoutTemplateId = workoutTemplate.Id,
            ExerciseTemplateId = exercise.Id,
            Position = 1
        };

        await AddAsync(templateExercise);

        // Start workout as user 1
        var workoutId = await SendAsync(new StartWorkoutCommand { WorkoutTemplateId = workoutTemplate.Id });

        // Switch to user 2
        await RunAsUserAsync("user2@localhost", "Password123!", Array.Empty<string>());

        // Try to update workout - should fail
        var command = new UpdateWorkoutCommand
        {
            Id = workoutId,
            Notes = "Hacked notes"
        };

        await Should.ThrowAsync<Application.Common.Exceptions.NotFoundException>(() => SendAsync(command));
    }
}
