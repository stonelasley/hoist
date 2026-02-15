using Hoist.Application.Common.Exceptions;
using Hoist.Application.Workouts.Commands.CreateWorkoutSet;
using Hoist.Application.Workouts.Commands.StartWorkout;
using Hoist.Application.Workouts.Queries.GetInProgressWorkout;
using Hoist.Application.Workouts.Queries.GetWorkout;
using Hoist.Domain.Entities;
using Hoist.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Hoist.Infrastructure.Data;

namespace Hoist.Application.FunctionalTests.Workouts.Queries;

using static Testing;

public class GetWorkoutTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnInProgressWorkout()
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
            Name = "Full Body",
            Notes = "Upper and lower body workout",
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

        // Add sets to exercises
        using var scope = GetScopeFactory().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var benchExercise = context.WorkoutExercises
            .First(we => we.WorkoutId == workoutId && we.ExerciseName == "Bench Press");

        await SendAsync(new CreateWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutExerciseId = benchExercise.Id,
            Weight = 135m,
            Reps = 10,
            WeightUnit = "Lbs"
        });

        await SendAsync(new CreateWorkoutSetCommand
        {
            WorkoutId = workoutId,
            WorkoutExerciseId = benchExercise.Id,
            Weight = 185m,
            Reps = 8,
            WeightUnit = "Lbs"
        });

        // Get in-progress workout
        var query = new GetInProgressWorkoutQuery();
        var result = await SendAsync(query);

        result.ShouldNotBeNull();
        result!.Id.ShouldBe(workoutId);
        result.TemplateName.ShouldBe("Full Body");
        result.Status.ShouldBe("InProgress");
        result.Exercises.Count.ShouldBe(2);

        var benchDto = result.Exercises.First(e => e.ExerciseName == "Bench Press");
        benchDto.Sets.Count.ShouldBe(2);
        benchDto.Sets[0].Weight.ShouldBe(135m);
        benchDto.Sets[0].Reps.ShouldBe(10);
        benchDto.Sets[1].Weight.ShouldBe(185m);
        benchDto.Sets[1].Reps.ShouldBe(8);
    }

    [Test]
    public async Task ShouldReturnNullWhenNoInProgressWorkout()
    {
        await RunAsDefaultUserAsync();

        var query = new GetInProgressWorkoutQuery();
        var result = await SendAsync(query);

        result.ShouldBeNull();
    }

    [Test]
    public async Task ShouldReturnWorkoutById()
    {
        var userId = await RunAsDefaultUserAsync();

        // Setup workout
        var exercise = new ExerciseTemplate
        {
            Name = "Deadlift",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            UserId = userId
        };

        await AddAsync(exercise);

        var template = new WorkoutTemplate
        {
            Name = "Back Day",
            UserId = userId
        };

        await AddAsync(template);

        await AddAsync(new WorkoutTemplateExercise
        {
            WorkoutTemplateId = template.Id,
            ExerciseTemplateId = exercise.Id,
            Position = 1
        });

        var workoutId = await SendAsync(new StartWorkoutCommand { WorkoutTemplateId = template.Id });

        // Get workout by ID
        var query = new GetWorkoutQuery { Id = workoutId };
        var result = await SendAsync(query);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(workoutId);
        result.TemplateName.ShouldBe("Back Day");
        result.Status.ShouldBe("InProgress");
        result.Exercises.Count.ShouldBe(1);
        result.Exercises[0].ExerciseName.ShouldBe("Deadlift");
    }

    [Test]
    public async Task ShouldThrowNotFoundForInvalidId()
    {
        await RunAsDefaultUserAsync();

        var query = new GetWorkoutQuery { Id = 99999 };

        await Should.ThrowAsync<Ardalis.GuardClauses.NotFoundException>(() => SendAsync(query));
    }
}
