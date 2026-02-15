using Hoist.Application.Common.Exceptions;
using Hoist.Application.Workouts.Commands.StartWorkout;
using Hoist.Domain.Entities;
using Hoist.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Hoist.Infrastructure.Data;

namespace Hoist.Application.FunctionalTests.Workouts.Commands;

using static Testing;

public class StartWorkoutTests : BaseTestFixture
{
    [Test]
    public async Task ShouldStartWorkoutFromTemplate()
    {
        var userId = await RunAsDefaultUserAsync();

        // Create exercise templates
        var benchPress = new ExerciseTemplate
        {
            Name = "Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            UserId = userId
        };

        var squat = new ExerciseTemplate
        {
            Name = "Squat",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            UserId = userId
        };

        await AddAsync(benchPress);
        await AddAsync(squat);

        // Create workout template
        var workoutTemplate = new WorkoutTemplate
        {
            Name = "Push Day",
            Notes = "Upper body workout",
            UserId = userId
        };

        await AddAsync(workoutTemplate);

        // Add exercises to template
        var templateExercise1 = new WorkoutTemplateExercise
        {
            WorkoutTemplateId = workoutTemplate.Id,
            ExerciseTemplateId = benchPress.Id,
            Position = 1
        };

        var templateExercise2 = new WorkoutTemplateExercise
        {
            WorkoutTemplateId = workoutTemplate.Id,
            ExerciseTemplateId = squat.Id,
            Position = 2
        };

        await AddAsync(templateExercise1);
        await AddAsync(templateExercise2);

        // Start workout from template
        var command = new StartWorkoutCommand { WorkoutTemplateId = workoutTemplate.Id };
        var workoutId = await SendAsync(command);

        // Verify workout was created
        var workout = await FindAsync<Workout>(workoutId);
        workout.ShouldNotBeNull();
        workout!.TemplateName.ShouldBe("Push Day");
        workout.Status.ShouldBe(WorkoutStatus.InProgress);
        workout.WorkoutTemplateId.ShouldBe(workoutTemplate.Id);
        workout.UserId.ShouldBe(userId);
        workout.StartedAt.ShouldBe(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(10));

        // Verify exercises were snapshotted
        using var scope = GetScopeFactory().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var workoutExercises = context.WorkoutExercises
            .Where(we => we.WorkoutId == workoutId)
            .OrderBy(we => we.Position)
            .ToList();

        workoutExercises.Count.ShouldBe(2);

        var exercise1 = workoutExercises[0];
        exercise1.ExerciseName.ShouldBe("Bench Press");
        exercise1.ImplementType.ShouldBe(ImplementType.Barbell);
        exercise1.ExerciseType.ShouldBe(ExerciseType.Reps);
        exercise1.Position.ShouldBe(1);
        exercise1.ExerciseTemplateId.ShouldBe(benchPress.Id);

        var exercise2 = workoutExercises[1];
        exercise2.ExerciseName.ShouldBe("Squat");
        exercise2.ImplementType.ShouldBe(ImplementType.Barbell);
        exercise2.ExerciseType.ShouldBe(ExerciseType.Reps);
        exercise2.Position.ShouldBe(2);
        exercise2.ExerciseTemplateId.ShouldBe(squat.Id);
    }

    [Test]
    public async Task ShouldRejectWhenAnotherWorkoutInProgress()
    {
        var userId = await RunAsDefaultUserAsync();

        // Create exercise template
        var exercise = new ExerciseTemplate
        {
            Name = "Deadlift",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            UserId = userId
        };

        await AddAsync(exercise);

        // Create workout template
        var template = new WorkoutTemplate
        {
            Name = "Back Day",
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

        // Start first workout
        var command1 = new StartWorkoutCommand { WorkoutTemplateId = template.Id };
        await SendAsync(command1);

        // Try to start second workout - should fail
        var command2 = new StartWorkoutCommand { WorkoutTemplateId = template.Id };
        await Should.ThrowAsync<ValidationException>(() => SendAsync(command2));
    }

    [Test]
    public async Task ShouldRejectWithInvalidTemplateId()
    {
        await RunAsDefaultUserAsync();

        var command = new StartWorkoutCommand { WorkoutTemplateId = 99999 };

        await Should.ThrowAsync<Ardalis.GuardClauses.NotFoundException>(() => SendAsync(command));
    }
}
