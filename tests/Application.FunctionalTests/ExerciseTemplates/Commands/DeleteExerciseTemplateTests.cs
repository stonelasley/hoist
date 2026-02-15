using Hoist.Application.Common.Interfaces;
using Hoist.Application.ExerciseTemplates.Commands.CreateExerciseTemplate;
using Hoist.Application.ExerciseTemplates.Commands.DeleteExerciseTemplate;
using Hoist.Application.ExerciseTemplates.Queries.GetExerciseTemplates;
using Hoist.Application.WorkoutTemplates.Commands.CreateWorkoutTemplate;
using Hoist.Application.WorkoutTemplates.Commands.UpdateWorkoutTemplateExercises;
using Hoist.Application.WorkoutTemplates.Queries.GetWorkoutTemplate;
using Hoist.Domain.Entities;
using Hoist.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Hoist.Application.FunctionalTests.ExerciseTemplates.Commands;

using static Testing;

public class DeleteExerciseTemplateTests : BaseTestFixture
{
    [Test]
    public async Task ShouldSoftDeleteExercise()
    {
        await RunAsDefaultUserAsync();

        var exerciseId = await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        await SendAsync(new DeleteExerciseTemplateCommand(exerciseId));

        var exercise = await FindAsync<ExerciseTemplate>(exerciseId);

        exercise.ShouldBeNull();
    }

    [Test]
    public async Task ShouldSetDeletedProperties()
    {
        await RunAsDefaultUserAsync();

        var exerciseId = await SendAsync(new CreateExerciseTemplateCommand
        {
            Name = "Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        });

        var beforeDelete = DateTime.UtcNow;

        await SendAsync(new DeleteExerciseTemplateCommand(exerciseId));

        var afterDelete = DateTime.UtcNow;

        using var scope = GetScopeFactory().CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var exercise = await context.ExerciseTemplates
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.Id == exerciseId);

        exercise.ShouldNotBeNull();
        exercise!.IsDeleted.ShouldBeTrue();
        exercise.DeletedAt.ShouldNotBeNull();
        exercise.DeletedAt!.Value.ShouldBeGreaterThanOrEqualTo(beforeDelete);
        exercise.DeletedAt.Value.ShouldBeLessThanOrEqualTo(afterDelete);
    }

    [Test]
    public async Task ShouldExcludeFromGetExerciseTemplates()
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

        var exercises = await SendAsync(new GetExerciseTemplatesQuery());

        exercises.ShouldNotBeNull();
        exercises.Count.ShouldBe(1);
        exercises[0].Id.ShouldBe(exercise2Id);
        exercises[0].Name.ShouldBe("Squat");
    }

    [Test]
    public async Task ShouldStillBeVisibleInWorkoutTemplateDetail()
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

        await SendAsync(new DeleteExerciseTemplateCommand(exerciseId));

        var workout = await SendAsync(new GetWorkoutTemplateQuery(workoutId));

        workout.ShouldNotBeNull();
        workout.Exercises.ShouldNotBeNull();
        workout.Exercises.Count.ShouldBe(1);
        workout.Exercises[0].ExerciseTemplateId.ShouldBe(exerciseId);
        workout.Exercises[0].ExerciseName.ShouldBe("Bench Press");
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

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(new DeleteExerciseTemplateCommand(exerciseId)));
    }

    [Test]
    public async Task ShouldThrowNotFoundForNonExistentExercise()
    {
        await RunAsDefaultUserAsync();

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(new DeleteExerciseTemplateCommand(999)));
    }

}
