using Hoist.Application.ExerciseTemplates.Commands.CreateExerciseTemplate;
using Hoist.Application.WorkoutTemplates.Commands.CreateWorkoutTemplate;
using Hoist.Application.WorkoutTemplates.Commands.DeleteWorkoutTemplate;
using Hoist.Application.WorkoutTemplates.Commands.UpdateWorkoutTemplateExercises;
using Hoist.Domain.Entities;
using Hoist.Domain.Enums;

namespace Hoist.Application.FunctionalTests.WorkoutTemplates.Commands;

using static Testing;

public class DeleteWorkoutTemplateTests : BaseTestFixture
{
    [Test]
    public async Task ShouldDeleteTemplate()
    {
        await RunAsDefaultUserAsync();

        var workoutId = await SendAsync(new CreateWorkoutTemplateCommand
        {
            Name = "Push Day"
        });

        await SendAsync(new DeleteWorkoutTemplateCommand(workoutId));

        var workout = await FindAsync<WorkoutTemplate>(workoutId);

        workout.ShouldBeNull();
    }

    [Test]
    public async Task ShouldDeleteTemplateAndCascadeDeleteExerciseAssociations()
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

        var associationCountBefore = await CountAsync<WorkoutTemplateExercise>();

        associationCountBefore.ShouldBe(2);

        await SendAsync(new DeleteWorkoutTemplateCommand(workoutId));

        var workout = await FindAsync<WorkoutTemplate>(workoutId);

        workout.ShouldBeNull();

        var associationCountAfter = await CountAsync<WorkoutTemplateExercise>();

        associationCountAfter.ShouldBe(0);
    }

    [Test]
    public async Task ShouldThrowNotFoundForOtherUsersTemplate()
    {
        await RunAsDefaultUserAsync();

        var workoutId = await SendAsync(new CreateWorkoutTemplateCommand
        {
            Name = "Push Day"
        });

        await RunAsUserAsync("other@local", "Testing1234!", Array.Empty<string>());

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(new DeleteWorkoutTemplateCommand(workoutId)));
    }

    [Test]
    public async Task ShouldThrowNotFoundForNonExistentTemplate()
    {
        await RunAsDefaultUserAsync();

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(new DeleteWorkoutTemplateCommand(999)));
    }

    [Test]
    public async Task ShouldPreserveExerciseTemplatesAfterWorkoutDeletion()
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

        await SendAsync(new DeleteWorkoutTemplateCommand(workoutId));

        var exercise1 = await FindAsync<ExerciseTemplate>(exercise1Id);
        var exercise2 = await FindAsync<ExerciseTemplate>(exercise2Id);

        exercise1.ShouldNotBeNull();
        exercise1!.Name.ShouldBe("Bench Press");

        exercise2.ShouldNotBeNull();
        exercise2!.Name.ShouldBe("Squat");
    }
}
