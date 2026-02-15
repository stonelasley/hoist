using Hoist.Application.Common.Exceptions;
using Hoist.Application.Workouts.Commands.CreateWorkoutSet;
using Hoist.Application.Workouts.Commands.StartWorkout;
using Hoist.Domain.Entities;
using Hoist.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Hoist.Infrastructure.Data;

namespace Hoist.Application.FunctionalTests.Workouts.Commands;

using static Testing;

public class CreateWorkoutSetTests : BaseTestFixture
{
    [Test]
    public async Task ShouldCreateWeightAndRepsSet()
    {
        var userId = await RunAsDefaultUserAsync();

        // Setup: Create exercise template and start a workout
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

        // Start workout
        var startCommand = new StartWorkoutCommand { WorkoutTemplateId = template.Id };
        var workoutId = await SendAsync(startCommand);

        // Get the workout exercise id
        using var scope = GetScopeFactory().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var workoutExercise = context.WorkoutExercises
            .First(we => we.WorkoutId == workoutId);

        // Create a set with weight and reps
        var createSetCommand = new CreateWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutExerciseId = workoutExercise.Id,
            Weight = 135m,
            Reps = 10,
            WeightUnit = "Lbs"
        };

        var setId = await SendAsync(createSetCommand);

        // Verify the set was created
        var set = await FindAsync<WorkoutSet>(setId);
        set.ShouldNotBeNull();
        set!.WorkoutExerciseId.ShouldBe(workoutExercise.Id);
        set.Position.ShouldBe(1);
        set.Weight.ShouldBe(135m);
        set.Reps.ShouldBe(10);
        set.WeightUnit.ShouldBe(WeightUnit.Lbs);
        set.Duration.ShouldBeNull();
        set.Distance.ShouldBeNull();
    }

    [Test]
    public async Task ShouldCreateBodyweightAndRepsSet()
    {
        var userId = await RunAsDefaultUserAsync();

        // Setup: Create bodyweight exercise
        var pullup = new ExerciseTemplate
        {
            Name = "Pull-ups",
            ImplementType = ImplementType.Bodyweight,
            ExerciseType = ExerciseType.Reps,
            UserId = userId
        };

        await AddAsync(pullup);

        var template = new WorkoutTemplate
        {
            Name = "Back Day",
            UserId = userId
        };

        await AddAsync(template);

        var templateExercise = new WorkoutTemplateExercise
        {
            WorkoutTemplateId = template.Id,
            ExerciseTemplateId = pullup.Id,
            Position = 1
        };

        await AddAsync(templateExercise);

        var workoutId = await SendAsync(new StartWorkoutCommand { WorkoutTemplateId = template.Id });

        using var scope = GetScopeFactory().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var workoutExercise = context.WorkoutExercises.First(we => we.WorkoutId == workoutId);

        // Create bodyweight set
        var createSetCommand = new CreateWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutExerciseId = workoutExercise.Id,
            Bodyweight = 180m,
            Reps = 12
        };

        var setId = await SendAsync(createSetCommand);

        var set = await FindAsync<WorkoutSet>(setId);
        set.ShouldNotBeNull();
        set!.Bodyweight.ShouldBe(180m);
        set.Reps.ShouldBe(12);
        set.Weight.ShouldBeNull();
    }

    [Test]
    public async Task ShouldCreateDistanceAndDurationSet()
    {
        var userId = await RunAsDefaultUserAsync();

        // Setup: Create distance exercise
        var rowing = new ExerciseTemplate
        {
            Name = "Rowing Machine",
            ImplementType = ImplementType.SelectorizedMachine,
            ExerciseType = ExerciseType.Distance,
            UserId = userId
        };

        await AddAsync(rowing);

        var template = new WorkoutTemplate
        {
            Name = "Cardio Day",
            UserId = userId
        };

        await AddAsync(template);

        var templateExercise = new WorkoutTemplateExercise
        {
            WorkoutTemplateId = template.Id,
            ExerciseTemplateId = rowing.Id,
            Position = 1
        };

        await AddAsync(templateExercise);

        var workoutId = await SendAsync(new StartWorkoutCommand { WorkoutTemplateId = template.Id });

        using var scope = GetScopeFactory().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var workoutExercise = context.WorkoutExercises.First(we => we.WorkoutId == workoutId);

        // Create distance/duration set
        var createSetCommand = new CreateWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutExerciseId = workoutExercise.Id,
            Distance = 500m,
            Duration = 120,
            DistanceUnit = "Meters"
        };

        var setId = await SendAsync(createSetCommand);

        var set = await FindAsync<WorkoutSet>(setId);
        set.ShouldNotBeNull();
        set!.Distance.ShouldBe(500m);
        set.Duration.ShouldBe(120);
        set.DistanceUnit.ShouldBe(DistanceUnit.Meters);
    }

    [Test]
    public async Task ShouldRejectWithInvalidWorkoutId()
    {
        await RunAsDefaultUserAsync();

        var command = new CreateWorkoutSetCommand
        {
            WorkoutId = 99999,
            WorkoutExerciseId = 1,
            Weight = 100m,
            Reps = 10
        };

        await Should.ThrowAsync<Application.Common.Exceptions.NotFoundException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRejectWithNoMeasurements()
    {
        var userId = await RunAsDefaultUserAsync();

        var exercise = new ExerciseTemplate
        {
            Name = "Test Exercise",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            UserId = userId
        };

        await AddAsync(exercise);

        var template = new WorkoutTemplate
        {
            Name = "Test Workout",
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

        // Try to create set with no measurements
        var command = new CreateWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutExerciseId = workoutExercise.Id
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }
}
