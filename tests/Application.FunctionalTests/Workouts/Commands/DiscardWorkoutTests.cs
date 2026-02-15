using Hoist.Application.Common.Exceptions;
using Hoist.Application.Workouts.Commands.CreateWorkoutSet;
using Hoist.Application.Workouts.Commands.DiscardWorkout;
using Hoist.Application.Workouts.Commands.StartWorkout;
using Hoist.Domain.Entities;
using Hoist.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Hoist.Infrastructure.Data;

namespace Hoist.Application.FunctionalTests.Workouts.Commands;

using static Testing;

public class DiscardWorkoutTests : BaseTestFixture
{
    [Test]
    public async Task ShouldDiscardInProgressWorkout()
    {
        var userId = await RunAsDefaultUserAsync();

        // Setup workout with exercises and sets
        var benchPress = new ExerciseTemplate
        {
            Name = "Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            UserId = userId
        };

        await AddAsync(benchPress);

        var template = new WorkoutTemplate
        {
            Name = "Chest Day",
            UserId = userId
        };

        await AddAsync(template);

        var templateExercise = new WorkoutTemplateExercise
        {
            WorkoutTemplateId = template.Id,
            ExerciseTemplateId = benchPress.Id,
            Position = 1
        };

        await AddAsync(templateExercise);

        var workoutId = await SendAsync(new StartWorkoutCommand { WorkoutTemplateId = template.Id });

        // Add some sets
        using var scope = GetScopeFactory().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var workoutExercise = context.WorkoutExercises.First(we => we.WorkoutId == workoutId);

        var setId = await SendAsync(new CreateWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutExerciseId = workoutExercise.Id,
            Weight = 135m,
            Reps = 10,
            WeightUnit = "Lbs"
        });

        // Discard the workout
        var discardCommand = new DiscardWorkoutCommand { Id = workoutId };
        await SendAsync(discardCommand);

        // Verify workout was deleted
        var workout = await FindAsync<Workout>(workoutId);
        workout.ShouldBeNull();

        // Verify exercises were cascade deleted
        using var scope2 = GetScopeFactory().CreateScope();
        var context2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var exercises = context2.WorkoutExercises
            .Where(we => we.WorkoutId == workoutId)
            .ToList();

        exercises.Count.ShouldBe(0);

        // Verify sets were cascade deleted
        var sets = context2.WorkoutSets
            .Where(s => s.WorkoutExerciseId == workoutExercise.Id)
            .ToList();

        sets.Count.ShouldBe(0);
    }

    [Test]
    public async Task ShouldRejectDiscardOfCompletedWorkout()
    {
        var userId = await RunAsDefaultUserAsync();

        // Setup completed workout
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

        // Mark workout as completed
        using var scope = GetScopeFactory().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var workout = context.Workouts.Find(workoutId);
        workout!.Status = WorkoutStatus.Completed;
        workout.EndedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync();

        // Try to discard completed workout - should fail
        var discardCommand = new DiscardWorkoutCommand { Id = workoutId };
        await Should.ThrowAsync<ValidationException>(() => SendAsync(discardCommand));

        // Verify workout still exists
        var workoutAfter = await FindAsync<Workout>(workoutId);
        workoutAfter.ShouldNotBeNull();
        workoutAfter!.Status.ShouldBe(WorkoutStatus.Completed);
    }
}
