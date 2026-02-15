using Hoist.Application.ExerciseTemplates.Commands.CreateExerciseTemplate;
using Hoist.Application.WorkoutTemplates.Commands.CreateWorkoutTemplate;
using Hoist.Application.WorkoutTemplates.Commands.UpdateWorkoutTemplateExercises;
using Hoist.Application.WorkoutTemplates.Queries.GetWorkoutTemplate;
using Hoist.Domain.Enums;

namespace Hoist.Application.FunctionalTests.WorkoutTemplates.Queries;

using static Testing;

public class GetWorkoutTemplateTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnDetailWithExercisesInPositionOrder()
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

        var exercise3Id = await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Deadlift",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        var workoutId = await SendAsync(new CreateWorkoutTemplateCommand
        {
            Name = "Push Day",
            Notes = "Test Notes"
        });

        await SendAsync(new UpdateWorkoutTemplateExercisesCommand
        {
            WorkoutTemplateId = workoutId,
            Exercises = new List<UpdateWorkoutTemplateExerciseItem>
            {
                new() { ExerciseTemplateId = exercise3Id },
                new() { ExerciseTemplateId = exercise1Id },
                new() { ExerciseTemplateId = exercise2Id }
            }
        });

        var result = await SendAsync(new GetWorkoutTemplateQuery(workoutId));

        result.ShouldNotBeNull();
        result.Id.ShouldBe(workoutId);
        result.Name.ShouldBe("Push Day");
        result.Notes.ShouldBe("Test Notes");
        result.Exercises.ShouldNotBeNull();
        result.Exercises.Count.ShouldBe(3);

        result.Exercises[0].Position.ShouldBe(1);
        result.Exercises[0].ExerciseTemplateId.ShouldBe(exercise3Id);
        result.Exercises[0].ExerciseName.ShouldBe("Deadlift");

        result.Exercises[1].Position.ShouldBe(2);
        result.Exercises[1].ExerciseTemplateId.ShouldBe(exercise1Id);
        result.Exercises[1].ExerciseName.ShouldBe("Bench Press");

        result.Exercises[2].Position.ShouldBe(3);
        result.Exercises[2].ExerciseTemplateId.ShouldBe(exercise2Id);
        result.Exercises[2].ExerciseName.ShouldBe("Squat");
    }

    [Test]
    public async Task ShouldThrowNotFoundForNonExistentWorkout()
    {
        await RunAsDefaultUserAsync();

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(new GetWorkoutTemplateQuery(999)));
    }

    [Test]
    public async Task ShouldThrowNotFoundForOtherUsersWorkout()
    {
        await RunAsDefaultUserAsync();

        var workoutId = await SendAsync(new CreateWorkoutTemplateCommand
        {
            Name = "Push Day"
        });

        await RunAsUserAsync("other@local", "Testing1234!", Array.Empty<string>());

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(new GetWorkoutTemplateQuery(workoutId)));
    }
}
