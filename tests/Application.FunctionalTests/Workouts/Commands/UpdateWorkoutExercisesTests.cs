using Hoist.Application.Common.Exceptions;
using Hoist.Application.Workouts.Commands.CreateWorkoutSet;
using Hoist.Application.Workouts.Commands.StartWorkout;
using Hoist.Application.Workouts.Commands.UpdateWorkoutExercises;
using Hoist.Domain.Entities;
using Hoist.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Hoist.Infrastructure.Data;

namespace Hoist.Application.FunctionalTests.Workouts.Commands;

using static Testing;

public class UpdateWorkoutExercisesTests : BaseTestFixture
{
    [Test]
    public async Task ShouldAddNewExercise()
    {
        var userId = await RunAsDefaultUserAsync();

        // Setup workout with one exercise
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

        var template = new WorkoutTemplate
        {
            Name = "Full Body",
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

        // Add squat to the workout
        var updateCommand = new UpdateWorkoutExercisesCommand
        {
            WorkoutId = workoutId,
            Exercises = new List<ExerciseInput>
            {
                new() { ExerciseTemplateId = benchPress.Id },
                new() { ExerciseTemplateId = squat.Id }
            }
        };

        await SendAsync(updateCommand);

        // Verify both exercises exist
        using var scope = GetScopeFactory().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var exercises = context.WorkoutExercises
            .Where(we => we.WorkoutId == workoutId)
            .OrderBy(we => we.Position)
            .ToList();

        exercises.Count.ShouldBe(2);
        exercises[0].ExerciseName.ShouldBe("Bench Press");
        exercises[0].Position.ShouldBe(1);
        exercises[1].ExerciseName.ShouldBe("Squat");
        exercises[1].Position.ShouldBe(2);
    }

    [Test]
    public async Task ShouldRemoveExerciseAndCascadeDeleteSets()
    {
        var userId = await RunAsDefaultUserAsync();

        // Setup workout with two exercises, one with sets
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

        var template = new WorkoutTemplate
        {
            Name = "Upper/Lower",
            UserId = userId
        };

        await AddAsync(template);

        await AddAsync(new WorkoutTemplateExercise
        {
            WorkoutTemplateId = template.Id,
            ExerciseTemplateId = benchPress.Id,
            Position = 1
        });

        await AddAsync(new WorkoutTemplateExercise
        {
            WorkoutTemplateId = template.Id,
            ExerciseTemplateId = squat.Id,
            Position = 2
        });

        var workoutId = await SendAsync(new StartWorkoutCommand { WorkoutTemplateId = template.Id });

        // Add sets to squat
        using var scope = GetScopeFactory().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var squatExercise = context.WorkoutExercises
            .First(we => we.WorkoutId == workoutId && we.ExerciseName == "Squat");

        await SendAsync(new CreateWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutExerciseId = squatExercise.Id,
            Weight = 225m,
            Reps = 5,
            WeightUnit = "Lbs"
        });

        // Remove squat from workout
        var updateCommand = new UpdateWorkoutExercisesCommand
        {
            WorkoutId = workoutId,
            Exercises = new List<ExerciseInput>
            {
                new() { ExerciseTemplateId = benchPress.Id }
            }
        };

        await SendAsync(updateCommand);

        // Verify squat was removed
        using var scope2 = GetScopeFactory().CreateScope();
        var context2 = scope2.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var exercises = context2.WorkoutExercises
            .Where(we => we.WorkoutId == workoutId)
            .ToList();

        exercises.Count.ShouldBe(1);
        exercises[0].ExerciseName.ShouldBe("Bench Press");

        // Verify sets were cascade deleted
        var sets = context2.WorkoutSets
            .Where(s => s.WorkoutExerciseId == squatExercise.Id)
            .ToList();

        sets.Count.ShouldBe(0);
    }

    [Test]
    public async Task ShouldPreserveSetsOnRetainedExercises()
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

        var squat = new ExerciseTemplate
        {
            Name = "Squat",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            UserId = userId
        };

        await AddAsync(benchPress);
        await AddAsync(squat);

        var template = new WorkoutTemplate
        {
            Name = "Test Workout",
            UserId = userId
        };

        await AddAsync(template);

        await AddAsync(new WorkoutTemplateExercise
        {
            WorkoutTemplateId = template.Id,
            ExerciseTemplateId = benchPress.Id,
            Position = 1
        });

        var workoutId = await SendAsync(new StartWorkoutCommand { WorkoutTemplateId = template.Id });

        using var scope = GetScopeFactory().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var benchExercise = context.WorkoutExercises
            .First(we => we.WorkoutId == workoutId);

        // Add sets to bench press
        var set1Id = await SendAsync(new CreateWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutExerciseId = benchExercise.Id,
            Weight = 135m,
            Reps = 10,
            WeightUnit = "Lbs"
        });

        var set2Id = await SendAsync(new CreateWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutExerciseId = benchExercise.Id,
            Weight = 185m,
            Reps = 8,
            WeightUnit = "Lbs"
        });

        // Update exercises list to include both (adding squat)
        var updateCommand = new UpdateWorkoutExercisesCommand
        {
            WorkoutId = workoutId,
            Exercises = new List<ExerciseInput>
            {
                new() { ExerciseTemplateId = benchPress.Id },
                new() { ExerciseTemplateId = squat.Id }
            }
        };

        await SendAsync(updateCommand);

        // Verify bench press sets were preserved
        var set1 = await FindAsync<WorkoutSet>(set1Id);
        var set2 = await FindAsync<WorkoutSet>(set2Id);

        set1.ShouldNotBeNull();
        set2.ShouldNotBeNull();
        set1!.Weight.ShouldBe(135m);
        set2!.Weight.ShouldBe(185m);
    }

    [Test]
    public async Task ShouldReorderExercises()
    {
        var userId = await RunAsDefaultUserAsync();

        // Setup workout with two exercises
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

        var template = new WorkoutTemplate
        {
            Name = "Full Body",
            UserId = userId
        };

        await AddAsync(template);

        await AddAsync(new WorkoutTemplateExercise
        {
            WorkoutTemplateId = template.Id,
            ExerciseTemplateId = benchPress.Id,
            Position = 1
        });

        await AddAsync(new WorkoutTemplateExercise
        {
            WorkoutTemplateId = template.Id,
            ExerciseTemplateId = squat.Id,
            Position = 2
        });

        var workoutId = await SendAsync(new StartWorkoutCommand { WorkoutTemplateId = template.Id });

        // Reorder exercises (swap order)
        var updateCommand = new UpdateWorkoutExercisesCommand
        {
            WorkoutId = workoutId,
            Exercises = new List<ExerciseInput>
            {
                new() { ExerciseTemplateId = squat.Id },
                new() { ExerciseTemplateId = benchPress.Id }
            }
        };

        await SendAsync(updateCommand);

        // Verify new order
        using var scope = GetScopeFactory().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var exercises = context.WorkoutExercises
            .Where(we => we.WorkoutId == workoutId)
            .OrderBy(we => we.Position)
            .ToList();

        exercises.Count.ShouldBe(2);
        exercises[0].ExerciseName.ShouldBe("Squat");
        exercises[0].Position.ShouldBe(1);
        exercises[1].ExerciseName.ShouldBe("Bench Press");
        exercises[1].Position.ShouldBe(2);
    }
}
