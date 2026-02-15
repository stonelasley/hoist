using Hoist.Application.ExerciseTemplates.Commands.CreateExerciseTemplate;
using Hoist.Application.WorkoutTemplates.Commands.CreateWorkoutTemplate;
using Hoist.Application.WorkoutTemplates.Commands.UpdateWorkoutTemplateExercises;
using Hoist.Application.WorkoutTemplates.Queries.GetWorkoutTemplate;
using Hoist.Domain.Entities;
using Hoist.Domain.Enums;

namespace Hoist.Application.FunctionalTests.WorkoutTemplates.Commands;

using static Testing;

public class UpdateWorkoutTemplateExercisesTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReplaceExerciseList()
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

        var result1 = await SendAsync(new GetWorkoutTemplateQuery(workoutId));

        result1.Exercises.Count.ShouldBe(2);
        result1.Exercises[0].ExerciseTemplateId.ShouldBe(exercise1Id);
        result1.Exercises[1].ExerciseTemplateId.ShouldBe(exercise2Id);

        await SendAsync(new UpdateWorkoutTemplateExercisesCommand
        {
            WorkoutTemplateId = workoutId,
            Exercises = new List<UpdateWorkoutTemplateExerciseItem>
            {
                new() { ExerciseTemplateId = exercise3Id }
            }
        });

        var result2 = await SendAsync(new GetWorkoutTemplateQuery(workoutId));

        result2.Exercises.Count.ShouldBe(1);
        result2.Exercises[0].ExerciseTemplateId.ShouldBe(exercise3Id);
    }

    [Test]
    public async Task ShouldAllowDuplicateExercises()
    {
        await RunAsDefaultUserAsync();

        var exerciseId = await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Bench Press",
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
                new() { ExerciseTemplateId = exerciseId },
                new() { ExerciseTemplateId = exerciseId },
                new() { ExerciseTemplateId = exerciseId }
            }
        });

        var result = await SendAsync(new GetWorkoutTemplateQuery(workoutId));

        result.Exercises.Count.ShouldBe(3);
        result.Exercises.ShouldAllBe(e => e.ExerciseTemplateId == exerciseId);
        result.Exercises[0].Position.ShouldBe(1);
        result.Exercises[1].Position.ShouldBe(2);
        result.Exercises[2].Position.ShouldBe(3);
    }

    [Test]
    public async Task ShouldValidateExerciseOwnership()
    {
        await RunAsDefaultUserAsync();

        var exercise1Id = await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "User1 Exercise",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        await RunAsUserAsync("other@local", "Testing1234!", Array.Empty<string>());

        var exercise2Id = await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "User2 Exercise",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        var workoutId = await SendAsync(new CreateWorkoutTemplateCommand
        {
            Name = "User2 Workout"
        });

        var command = new UpdateWorkoutTemplateExercisesCommand
        {
            WorkoutTemplateId = workoutId,
            Exercises = new List<UpdateWorkoutTemplateExerciseItem>
            {
                new() { ExerciseTemplateId = exercise1Id }
            }
        };

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldThrowNotFoundForOtherUsersWorkout()
    {
        await RunAsDefaultUserAsync();

        var exerciseId = await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        var workoutId = await SendAsync(new CreateWorkoutTemplateCommand
        {
            Name = "Push Day"
        });

        await RunAsUserAsync("other@local", "Testing1234!", Array.Empty<string>());

        var otherExerciseId = await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Other Exercise",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        var command = new UpdateWorkoutTemplateExercisesCommand
        {
            WorkoutTemplateId = workoutId,
            Exercises = new List<UpdateWorkoutTemplateExerciseItem>
            {
                new() { ExerciseTemplateId = otherExerciseId }
            }
        };

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldPreservePositionOrdering()
    {
        await RunAsDefaultUserAsync();

        var exercise1Id = await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Exercise 1",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        var exercise2Id = await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Exercise 2",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        var exercise3Id = await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Exercise 3",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        var workoutId = await SendAsync(new CreateWorkoutTemplateCommand
        {
            Name = "Test Workout"
        });

        await SendAsync(new UpdateWorkoutTemplateExercisesCommand
        {
            WorkoutTemplateId = workoutId,
            Exercises = new List<UpdateWorkoutTemplateExerciseItem>
            {
                new() { ExerciseTemplateId = exercise2Id },
                new() { ExerciseTemplateId = exercise3Id },
                new() { ExerciseTemplateId = exercise1Id }
            }
        });

        var result = await SendAsync(new GetWorkoutTemplateQuery(workoutId));

        result.Exercises.Count.ShouldBe(3);

        result.Exercises[0].Position.ShouldBe(1);
        result.Exercises[0].ExerciseTemplateId.ShouldBe(exercise2Id);

        result.Exercises[1].Position.ShouldBe(2);
        result.Exercises[1].ExerciseTemplateId.ShouldBe(exercise3Id);

        result.Exercises[2].Position.ShouldBe(3);
        result.Exercises[2].ExerciseTemplateId.ShouldBe(exercise1Id);
    }

    [Test]
    public async Task ShouldAllowEmptyListToRemoveAllExercises()
    {
        await RunAsDefaultUserAsync();

        var exerciseId = await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Bench Press",
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
                new() { ExerciseTemplateId = exerciseId }
            }
        });

        var result1 = await SendAsync(new GetWorkoutTemplateQuery(workoutId));

        result1.Exercises.Count.ShouldBe(1);

        await SendAsync(new UpdateWorkoutTemplateExercisesCommand
        {
            WorkoutTemplateId = workoutId,
            Exercises = new List<UpdateWorkoutTemplateExerciseItem>()
        });

        var result2 = await SendAsync(new GetWorkoutTemplateQuery(workoutId));

        result2.Exercises.Count.ShouldBe(0);
    }

    [Test]
    public async Task ShouldThrowNotFoundForNonExistentExercise()
    {
        await RunAsDefaultUserAsync();

        var workoutId = await SendAsync(new CreateWorkoutTemplateCommand
        {
            Name = "Push Day"
        });

        var command = new UpdateWorkoutTemplateExercisesCommand
        {
            WorkoutTemplateId = workoutId,
            Exercises = new List<UpdateWorkoutTemplateExerciseItem>
            {
                new() { ExerciseTemplateId = 999 }
            }
        };

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(command));
    }
}
