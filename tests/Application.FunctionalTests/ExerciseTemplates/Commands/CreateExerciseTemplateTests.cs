using Hoist.Application.Common.Exceptions;
using Hoist.Application.ExerciseTemplates.Commands.CreateExerciseTemplate;
using Hoist.Domain.Entities;
using Hoist.Domain.Enums;

namespace Hoist.Application.FunctionalTests.ExerciseTemplates.Commands;

using static Testing;

public class CreateExerciseTemplateTests : BaseTestFixture
{
    [Test]
    public async Task ShouldCreateWithValidData()
    {
        var userId = await RunAsDefaultUserAsync();

        var command = new CreateExerciseTemplateCommand
        {
            Name = "Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            Model = "Test Model"
        };

        var exerciseId = await SendAsync(command);

        var exercise = await FindAsync<ExerciseTemplate>(exerciseId);

        exercise.ShouldNotBeNull();
        exercise!.Id.ShouldBe(exerciseId);
        exercise.Name.ShouldBe(command.Name);
        exercise.ImplementType.ShouldBe(command.ImplementType);
        exercise.ExerciseType.ShouldBe(command.ExerciseType);
        exercise.Model.ShouldBe(command.Model);
        exercise.UserId.ShouldBe(userId);
        exercise.IsDeleted.ShouldBeFalse();
        exercise.DeletedAt.ShouldBeNull();
        exercise.CreatedBy.ShouldBe(userId);
        exercise.Created.ShouldBe(DateTime.Now, TimeSpan.FromMilliseconds(10000));
    }

    [Test]
    public async Task ShouldRequireMinimumFields()
    {
        await RunAsDefaultUserAsync();

        var command = new CreateExerciseTemplateCommand();

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequireName()
    {
        await RunAsDefaultUserAsync();

        var command = new CreateExerciseTemplateCommand
        {
            Name = "",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequireValidImplementType()
    {
        await RunAsDefaultUserAsync();

        var command = new CreateExerciseTemplateCommand
        {
            Name = "Test Exercise",
            ImplementType = (ImplementType)999,
            ExerciseType = ExerciseType.Reps
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequireValidExerciseType()
    {
        await RunAsDefaultUserAsync();

        var command = new CreateExerciseTemplateCommand
        {
            Name = "Test Exercise",
            ImplementType = ImplementType.Barbell,
            ExerciseType = (ExerciseType)999
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldEnforceUniqueNamePerUser()
    {
        await RunAsDefaultUserAsync();

        var command = new CreateExerciseTemplateCommand
        {
            Name = "Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        };

        await SendAsync(command);

        var duplicateCommand = new CreateExerciseTemplateCommand
        {
            Name = "Bench Press",
            ImplementType = ImplementType.Dumbbell,
            ExerciseType = ExerciseType.Reps
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(duplicateCommand));
    }

    [Test]
    public async Task ShouldAllowSameNameForDifferentUsers()
    {
        await RunAsDefaultUserAsync();

        var command1 = new CreateExerciseTemplateCommand
        {
            Name = "Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        };

        var exercise1Id = await SendAsync(command1);

        var userId2 = await RunAsUserAsync("other@local", "Testing1234!", Array.Empty<string>());

        var command2 = new CreateExerciseTemplateCommand
        {
            Name = "Bench Press",
            ImplementType = ImplementType.Dumbbell,
            ExerciseType = ExerciseType.Reps
        };

        var exercise2Id = await SendAsync(command2);

        exercise2Id.ShouldNotBe(exercise1Id);

        var exercise2 = await FindAsync<ExerciseTemplate>(exercise2Id);

        exercise2.ShouldNotBeNull();
        exercise2!.Name.ShouldBe("Bench Press");
        exercise2.UserId.ShouldBe(userId2);
    }
}
