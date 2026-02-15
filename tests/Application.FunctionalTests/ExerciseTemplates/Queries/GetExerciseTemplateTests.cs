using Hoist.Application.ExerciseTemplates.Commands.CreateExerciseTemplate;
using Hoist.Application.ExerciseTemplates.Commands.DeleteExerciseTemplate;
using Hoist.Application.ExerciseTemplates.Queries.GetExerciseTemplate;
using Hoist.Domain.Enums;

namespace Hoist.Application.FunctionalTests.ExerciseTemplates.Queries;

using static Testing;

public class GetExerciseTemplateTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnDetail()
    {
        await RunAsDefaultUserAsync();

        var exerciseId = await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            Model = "Test Model"
        });

        var result = await SendAsync(new GetExerciseTemplateQuery(exerciseId));

        result.ShouldNotBeNull();
        result.Id.ShouldBe(exerciseId);
        result.Name.ShouldBe("Bench Press");
        result.ImplementType.ShouldBe("Barbell");
        result.ExerciseType.ShouldBe("Reps");
        result.Model.ShouldBe("Test Model");
    }

    [Test]
    public async Task ShouldThrowNotFoundForNonExistentExercise()
    {
        await RunAsDefaultUserAsync();

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(new GetExerciseTemplateQuery(999)));
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

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(new GetExerciseTemplateQuery(exerciseId)));
    }

    [Test]
    public async Task ShouldThrowNotFoundForSoftDeletedExercise()
    {
        await RunAsDefaultUserAsync();

        var exerciseId = await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        await SendAsync(new DeleteExerciseTemplateCommand(exerciseId));

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(new GetExerciseTemplateQuery(exerciseId)));
    }
}
