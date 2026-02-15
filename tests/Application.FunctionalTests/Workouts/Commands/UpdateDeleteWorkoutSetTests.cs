using Hoist.Application.Common.Exceptions;
using Hoist.Application.Workouts.Commands.CreateWorkoutSet;
using Hoist.Application.Workouts.Commands.DeleteWorkoutSet;
using Hoist.Application.Workouts.Commands.StartWorkout;
using Hoist.Application.Workouts.Commands.UpdateWorkoutSet;
using Hoist.Domain.Entities;
using Hoist.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Hoist.Infrastructure.Data;

namespace Hoist.Application.FunctionalTests.Workouts.Commands;

using static Testing;

public class UpdateDeleteWorkoutSetTests : BaseTestFixture
{
    [Test]
    public async Task ShouldUpdateSetFields()
    {
        var userId = await RunAsDefaultUserAsync();

        // Setup workout with a set
        var exercise = new ExerciseTemplate
        {
            Name = "Squat",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            UserId = userId
        };

        await AddAsync(exercise);

        var template = new WorkoutTemplate
        {
            Name = "Leg Day",
            UserId = userId
        };

        await AddAsync(template);

        var templateExercise = new WorkoutTemplateExercise
        {
            WorkoutTemplateId = template.Id,
            ExerciseTemplateId = exercise.Id,
            Position = 1
        };

        await AddAsync(templateExercise);

        var workoutId = await SendAsync(new StartWorkoutCommand { WorkoutTemplateId = template.Id });

        using var scope = GetScopeFactory().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var workoutExercise = context.WorkoutExercises.First(we => we.WorkoutId == workoutId);

        // Create initial set
        var setId = await SendAsync(new CreateWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutExerciseId = workoutExercise.Id,
            Weight = 135m,
            Reps = 10,
            WeightUnit = "Lbs"
        });

        // Update the set
        var updateCommand = new UpdateWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutExerciseId = workoutExercise.Id,
            SetId = setId,
            Weight = 185m,
            Reps = 8,
            WeightUnit = "Lbs"
        };

        await SendAsync(updateCommand);

        // Verify update
        var updatedSet = await FindAsync<WorkoutSet>(setId);
        updatedSet.ShouldNotBeNull();
        updatedSet!.Weight.ShouldBe(185m);
        updatedSet.Reps.ShouldBe(8);
        updatedSet.WeightUnit.ShouldBe(WeightUnit.Lbs);
    }

    [Test]
    public async Task ShouldDeleteSetAndResequencePositions()
    {
        var userId = await RunAsDefaultUserAsync();

        // Setup workout with 3 sets
        var exercise = new ExerciseTemplate
        {
            Name = "Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            UserId = userId
        };

        await AddAsync(exercise);

        var template = new WorkoutTemplate
        {
            Name = "Chest Day",
            UserId = userId
        };

        await AddAsync(template);

        var templateExercise = new WorkoutTemplateExercise
        {
            WorkoutTemplateId = template.Id,
            ExerciseTemplateId = exercise.Id,
            Position = 1
        };

        await AddAsync(templateExercise);

        var workoutId = await SendAsync(new StartWorkoutCommand { WorkoutTemplateId = template.Id });

        using var scope = GetScopeFactory().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var workoutExercise = context.WorkoutExercises.First(we => we.WorkoutId == workoutId);

        // Create 3 sets
        var set1Id = await SendAsync(new CreateWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutExerciseId = workoutExercise.Id,
            Weight = 135m,
            Reps = 10,
            WeightUnit = "Lbs"
        });

        var set2Id = await SendAsync(new CreateWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutExerciseId = workoutExercise.Id,
            Weight = 185m,
            Reps = 8,
            WeightUnit = "Lbs"
        });

        var set3Id = await SendAsync(new CreateWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutExerciseId = workoutExercise.Id,
            Weight = 205m,
            Reps = 5,
            WeightUnit = "Lbs"
        });

        // Delete middle set
        var deleteCommand = new DeleteWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutExerciseId = workoutExercise.Id,
            SetId = set2Id
        };

        await SendAsync(deleteCommand);

        // Verify deletion
        var deletedSet = await FindAsync<WorkoutSet>(set2Id);
        deletedSet.ShouldBeNull();

        // Verify resequencing
        var set1 = await FindAsync<WorkoutSet>(set1Id);
        set1.ShouldNotBeNull();
        set1!.Position.ShouldBe(1);

        var set3 = await FindAsync<WorkoutSet>(set3Id);
        set3.ShouldNotBeNull();
        set3!.Position.ShouldBe(2); // Position should be decremented from 3 to 2
        set3.Weight.ShouldBe(205m); // Verify it's still the correct set
    }

    [Test]
    public async Task ShouldRejectUpdateWithInvalidOwnership()
    {
        var userId1 = await RunAsDefaultUserAsync();

        // Create workout for user 1
        var exercise = new ExerciseTemplate
        {
            Name = "Deadlift",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            UserId = userId1
        };

        await AddAsync(exercise);

        var template = new WorkoutTemplate
        {
            Name = "Back Day",
            UserId = userId1
        };

        await AddAsync(template);

        var templateExercise = new WorkoutTemplateExercise
        {
            WorkoutTemplateId = template.Id,
            ExerciseTemplateId = exercise.Id,
            Position = 1
        };

        await AddAsync(templateExercise);

        var workoutId = await SendAsync(new StartWorkoutCommand { WorkoutTemplateId = template.Id });

        using var scope = GetScopeFactory().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var workoutExercise = context.WorkoutExercises.First(we => we.WorkoutId == workoutId);

        var setId = await SendAsync(new CreateWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutExerciseId = workoutExercise.Id,
            Weight = 225m,
            Reps = 5,
            WeightUnit = "Lbs"
        });

        // Switch to a different user
        await RunAsUserAsync("test2@local", "Testing1234!", Array.Empty<string>());

        // Try to update the set as different user - should fail
        var updateCommand = new UpdateWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutExerciseId = workoutExercise.Id,
            SetId = setId,
            Weight = 275m,
            Reps = 3,
            WeightUnit = "Lbs"
        };

        await Should.ThrowAsync<Application.Common.Exceptions.NotFoundException>(() => SendAsync(updateCommand));
    }
}
