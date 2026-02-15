using Hoist.Application.Common.Exceptions;
using Hoist.Application.WorkoutTemplates.Commands.CreateWorkoutTemplate;
using Hoist.Application.WorkoutTemplates.Commands.UpdateWorkoutTemplate;
using Hoist.Domain.Entities;

namespace Hoist.Application.FunctionalTests.WorkoutTemplates.Commands;

using static Testing;

public class UpdateWorkoutTemplateTests : BaseTestFixture
{
    [Test]
    public async Task ShouldUpdateAllFields()
    {
        var userId = await RunAsDefaultUserAsync();

        var workoutId = await SendAsync(new CreateWorkoutTemplateCommand
        {
            Name = "Push Day",
            Notes = "Original Notes"
        });

        var command = new UpdateWorkoutTemplateCommand
        {
            Id = workoutId,
            Name = "Updated Push Day",
            Notes = "Updated Notes"
        };

        await SendAsync(command);

        var workout = await FindAsync<WorkoutTemplate>(workoutId);

        workout.ShouldNotBeNull();
        workout!.Id.ShouldBe(workoutId);
        workout.Name.ShouldBe(command.Name);
        workout.Notes.ShouldBe(command.Notes);
        workout.LastModifiedBy.ShouldBe(userId);
        workout.LastModified.ShouldBe(DateTime.Now, TimeSpan.FromMilliseconds(10000));
    }

    [Test]
    public async Task ShouldRequireName()
    {
        await RunAsDefaultUserAsync();

        var workoutId = await SendAsync(new CreateWorkoutTemplateCommand
        {
            Name = "Push Day"
        });

        var command = new UpdateWorkoutTemplateCommand
        {
            Id = workoutId,
            Name = ""
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequireNonEmptyName()
    {
        await RunAsDefaultUserAsync();

        var workoutId = await SendAsync(new CreateWorkoutTemplateCommand
        {
            Name = "Push Day"
        });

        var command = new UpdateWorkoutTemplateCommand
        {
            Id = workoutId,
            Name = null
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
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

        var command = new UpdateWorkoutTemplateCommand
        {
            Id = workoutId,
            Name = "Updated Name"
        };

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldThrowNotFoundForNonExistentTemplate()
    {
        await RunAsDefaultUserAsync();

        var command = new UpdateWorkoutTemplateCommand
        {
            Id = 999,
            Name = "Updated Name"
        };

        await Should.ThrowAsync<NotFoundException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldUpdateModifiedDate()
    {
        var userId = await RunAsDefaultUserAsync();

        var workoutId = await SendAsync(new CreateWorkoutTemplateCommand
        {
            Name = "Push Day"
        });

        var originalWorkout = await FindAsync<WorkoutTemplate>(workoutId);
        var originalModified = originalWorkout!.LastModified;

        await Task.Delay(100);

        await SendAsync(new UpdateWorkoutTemplateCommand
        {
            Id = workoutId,
            Name = "Updated Push Day"
        });

        var updatedWorkout = await FindAsync<WorkoutTemplate>(workoutId);

        updatedWorkout.ShouldNotBeNull();
        updatedWorkout!.LastModified.ShouldBeGreaterThan(originalModified);
        updatedWorkout.LastModifiedBy.ShouldBe(userId);
    }

    [Test]
    public async Task ShouldAllowClearingOptionalFields()
    {
        await RunAsDefaultUserAsync();

        var workoutId = await SendAsync(new CreateWorkoutTemplateCommand
        {
            Name = "Push Day",
            Notes = "Some notes"
        });

        await SendAsync(new UpdateWorkoutTemplateCommand
        {
            Id = workoutId,
            Name = "Push Day",
            Notes = null
        });

        var workout = await FindAsync<WorkoutTemplate>(workoutId);

        workout.ShouldNotBeNull();
        workout!.Notes.ShouldBeNull();
        workout.LocationId.ShouldBeNull();
    }
}
