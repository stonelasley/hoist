using Hoist.Application.ExerciseTemplates.Commands.CreateExerciseTemplate;
using Hoist.Application.WorkoutTemplates.Commands.CreateWorkoutTemplate;
using Hoist.Application.WorkoutTemplates.Commands.UpdateWorkoutTemplateExercises;
using Hoist.Application.WorkoutTemplates.Queries.GetWorkoutTemplates;
using Hoist.Domain.Enums;

namespace Hoist.Application.FunctionalTests.WorkoutTemplates.Queries;

using static Testing;

public class GetWorkoutTemplatesTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnUserWorkoutTemplatesOnly()
    {
        var userId1 = await RunAsDefaultUserAsync();

        var workout1Id = await SendAsync(new CreateWorkoutTemplateCommand
        {
            Name = "User1 Push Day"
        });

        var workout2Id = await SendAsync(new CreateWorkoutTemplateCommand
        {
            Name = "User1 Pull Day"
        });

        var userId2 = await RunAsUserAsync("other@local", "Testing1234!", Array.Empty<string>());

        var workout3Id = await SendAsync(new CreateWorkoutTemplateCommand
        {
            Name = "User2 Leg Day"
        });

        var user2Workouts = await SendAsync(new GetWorkoutTemplatesQuery());

        user2Workouts.ShouldNotBeNull();
        user2Workouts.Count.ShouldBe(1);
        user2Workouts[0].Id.ShouldBe(workout3Id);
        user2Workouts[0].Name.ShouldBe("User2 Leg Day");

        await RunAsDefaultUserAsync();

        var user1Workouts = await SendAsync(new GetWorkoutTemplatesQuery());

        user1Workouts.ShouldNotBeNull();
        user1Workouts.Count.ShouldBe(2);
        var workoutIds = user1Workouts.Select(w => w.Id).ToList();
        workoutIds.ShouldContain(workout1Id);
        workoutIds.ShouldContain(workout2Id);
        workoutIds.ShouldNotContain(workout3Id);
    }

    [Test]
    public async Task ShouldReturnExerciseCount()
    {
        await RunAsDefaultUserAsync();

        var exercise1Id = await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        var exercise2Id = await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Squat",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        var workoutId = await SendAsync(new CreateWorkoutTemplateCommand
        {
            Name = "Push Day"
        });

        await SendAsync(new UpdateWorkoutTemplateExercisesCommand
        {
            WorkoutTemplateId = workoutId,
            Exercises = new List<UpdateWorkoutTemplateExerciseItem>
            {
                new() { ExerciseTemplateId = exercise1Id },
                new() { ExerciseTemplateId = exercise2Id }
            }
        });

        var workouts = await SendAsync(new GetWorkoutTemplatesQuery());

        workouts.ShouldNotBeNull();
        workouts.Count.ShouldBe(1);
        workouts[0].Id.ShouldBe(workoutId);
        workouts[0].ExerciseCount.ShouldBe(2);
    }

    [Test]
    public async Task ShouldExcludeOtherUsersTemplates()
    {
        await RunAsDefaultUserAsync();

        var workout1Id = await SendAsync(new CreateWorkoutTemplateCommand
        {
            Name = "User1 Workout"
        });

        await RunAsUserAsync("other@local", "Testing1234!", Array.Empty<string>());

        var workout2Id = await SendAsync(new CreateWorkoutTemplateCommand
        {
            Name = "User2 Workout"
        });

        var workouts = await SendAsync(new GetWorkoutTemplatesQuery());

        workouts.ShouldNotBeNull();
        workouts.Count.ShouldBe(1);
        workouts[0].Id.ShouldBe(workout2Id);
        workouts[0].Name.ShouldBe("User2 Workout");
    }

    [Test]
    public async Task ShouldReturnEmptyResultForUserWithNoWorkouts()
    {
        await RunAsDefaultUserAsync();

        var result = await SendAsync(new GetWorkoutTemplatesQuery());

        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }
}
