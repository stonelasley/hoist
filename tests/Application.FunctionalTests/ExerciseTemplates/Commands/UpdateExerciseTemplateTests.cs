using Hoist.Application.Common.Exceptions;
using Hoist.Application.ExerciseTemplates.Commands.CreateExerciseTemplate;
using Hoist.Application.ExerciseTemplates.Commands.UpdateExerciseTemplate;
using Hoist.Domain.Entities;
using Hoist.Domain.Enums;

namespace Hoist.Application.FunctionalTests.ExerciseTemplates.Commands;

using static Testing;

public class UpdateExerciseTemplateTests : BaseTestFixture
{
    [Test]
    public async Task ShouldUpdateAllFields()
    {
        var userId = await RunAsDefaultUserAsync();

        var exerciseId = await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            Model = "Original Model"
        });

        var command = new UpdateExerciseTemplateCommand
        {
            Id = exerciseId,
            Name = "Incline Bench Press",
            ImplementType = ImplementType.Dumbbell,
            ExerciseType = ExerciseType.Duration,
            Model = "Updated Model"
        };

        await SendAsync(command);

        var exercise = await FindAsync<ExerciseTemplate>(exerciseId);

        exercise.ShouldNotBeNull();
        exercise!.Id.ShouldBe(exerciseId);
        exercise.Name.ShouldBe(command.Name);
        exercise.ImplementType.ShouldBe(command.ImplementType);
        exercise.ExerciseType.ShouldBe(command.ExerciseType);
        exercise.Model.ShouldBe(command.Model);
        exercise.LastModifiedBy.ShouldBe(userId);
        exercise.LastModified.ShouldBe(DateTime.Now, TimeSpan.FromMilliseconds(10000));
    }

    [Test]
    public async Task ShouldRequireName()
    {
        await RunAsDefaultUserAsync();

        var exerciseId = await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        var command = new UpdateExerciseTemplateCommand
        {
            Id = exerciseId,
            Name = "",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldEnforceUniqueNameExcludingSelf()
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

        var updateToExistingName = new UpdateExerciseTemplateCommand
        {
            Id = exercise2Id,
            Name = "Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(updateToExistingName));

        var updateToSameName = new UpdateExerciseTemplateCommand
        {
            Id = exercise1Id,
            Name = "Bench Press",
            ImplementType = ImplementType.Dumbbell,
            ExerciseType = ExerciseType.Duration
        };

        await SendAsync(updateToSameName);

        var exercise = await FindAsync<ExerciseTemplate>(exercise1Id);

        exercise.ShouldNotBeNull();
        exercise!.Name.ShouldBe("Bench Press");
        exercise.ImplementType.ShouldBe(ImplementType.Dumbbell);
    }

    [Test]
    public async Task ShouldThrowNotFoundForOtherUsersExercise()
    {
        await RunAsDefaultUserAsync();

        var exerciseId = await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        await RunAsUserAsync("other@local", "Testing1234!", Array.Empty<string>());

        var command = new UpdateExerciseTemplateCommand
        {
            Id = exerciseId,
            Name = "Updated Name",
            ImplementType = ImplementType.Dumbbell,
            ExerciseType = ExerciseType.Reps
        };

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldThrowNotFoundForNonExistentExercise()
    {
        await RunAsDefaultUserAsync();

        var command = new UpdateExerciseTemplateCommand
        {
            Id = 999,
            Name = "Updated Name",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        };

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(command));
    }
}
