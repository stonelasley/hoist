using Hoist.Application.ExerciseTemplates.Commands.CreateExerciseTemplate;
using Hoist.Application.ExerciseTemplates.Commands.DeleteExerciseTemplate;
using Hoist.Application.ExerciseTemplates.Queries.GetExerciseTemplates;
using Hoist.Domain.Enums;

namespace Hoist.Application.FunctionalTests.ExerciseTemplates.Queries;

using static Testing;

public class GetExerciseTemplatesTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnUserExercisesOnly()
    {
        var userId1 = await RunAsDefaultUserAsync();

        var exercise1Id = await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "User1 Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        var exercise2Id = await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "User1 Squat",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        var userId2 = await RunAsUserAsync("other@local", "Testing1234!", Array.Empty<string>());

        var exercise3Id = await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "User2 Deadlift",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        var user2Exercises = await SendAsync(new GetExerciseTemplatesQuery());

        user2Exercises.ShouldNotBeNull();
        user2Exercises.Count.ShouldBe(1);
        user2Exercises[0].Id.ShouldBe(exercise3Id);
        user2Exercises[0].Name.ShouldBe("User2 Deadlift");

        await RunAsDefaultUserAsync();

        var user1Exercises = await SendAsync(new GetExerciseTemplatesQuery());

        user1Exercises.ShouldNotBeNull();
        user1Exercises.Count.ShouldBe(2);
        var exerciseIds = user1Exercises.Select(e => e.Id).ToList();
        exerciseIds.ShouldContain(exercise1Id);
        exerciseIds.ShouldContain(exercise2Id);
        exerciseIds.ShouldNotContain(exercise3Id);
    }

    [Test]
    public async Task ShouldFilterBySearchCaseInsensitive()
    {
        await RunAsDefaultUserAsync();

        await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Squat",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Incline Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        var result = await SendAsync(new GetExerciseTemplatesQuery { Search = "bench" });

        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);
        result.ShouldAllBe(e => e.Name.Contains("Bench", StringComparison.OrdinalIgnoreCase));
    }

    [Test]
    public async Task ShouldFilterByImplementType()
    {
        await RunAsDefaultUserAsync();

        await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Barbell Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Dumbbell Bench Press",
            ImplementType = ImplementType.Dumbbell,
            ExerciseType = ExerciseType.Reps
        });

        await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Cable Fly",
            ImplementType = ImplementType.Band,
            ExerciseType = ExerciseType.Reps
        });

        var result = await SendAsync(new GetExerciseTemplatesQuery { ImplementType = ImplementType.Dumbbell });

        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Dumbbell Bench Press");
        result[0].ImplementType.ShouldBe("Dumbbell");
    }

    [Test]
    public async Task ShouldFilterByExerciseType()
    {
        await RunAsDefaultUserAsync();

        await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Plank",
            ImplementType = ImplementType.Bodyweight,
            ExerciseType = ExerciseType.Duration
        });

        await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Running",
            ImplementType = ImplementType.Bodyweight,
            ExerciseType = ExerciseType.Distance
        });

        var result = await SendAsync(new GetExerciseTemplatesQuery { ExerciseType = ExerciseType.Duration });

        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Plank");
        result[0].ExerciseType.ShouldBe("Duration");
    }

    [Test]
    public async Task ShouldFilterByCombinedSearchAndFilters()
    {
        await RunAsDefaultUserAsync();

        await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Barbell Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Dumbbell Bench Press",
            ImplementType = ImplementType.Dumbbell,
            ExerciseType = ExerciseType.Reps
        });

        await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Barbell Squat",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        var result = await SendAsync(new GetExerciseTemplatesQuery
        {
            Search = "bench",
            ImplementType = ImplementType.Barbell
        });

        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].Name.ShouldBe("Barbell Bench Press");
    }

    [Test]
    public async Task ShouldExcludeSoftDeletedExercises()
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

        await SendAsync(new DeleteExerciseTemplateCommand(exercise1Id));

        var result = await SendAsync(new GetExerciseTemplatesQuery());

        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);
        result[0].Id.ShouldBe(exercise2Id);
        result[0].Name.ShouldBe("Squat");
    }

    [Test]
    public async Task ShouldReturnEmptyResultForUserWithNoExercises()
    {
        await RunAsDefaultUserAsync();

        var result = await SendAsync(new GetExerciseTemplatesQuery());

        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }
}
