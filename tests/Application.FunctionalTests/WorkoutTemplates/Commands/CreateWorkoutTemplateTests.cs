using Hoist.Application.Common.Exceptions;
using Hoist.Application.WorkoutTemplates.Commands.CreateWorkoutTemplate;
using Hoist.Domain.Entities;

namespace Hoist.Application.FunctionalTests.WorkoutTemplates.Commands;

using static Testing;

public class CreateWorkoutTemplateTests : BaseTestFixture
{
    [Test]
    public async Task ShouldCreateWithValidData()
    {
        var userId = await RunAsDefaultUserAsync();

        var command = new CreateWorkoutTemplateCommand
        {
            Name = "Push Day",
            Notes = "Focus on chest and triceps",
            Location = "Gold's Gym"
        };

        var workoutId = await SendAsync(command);

        var workout = await FindAsync<WorkoutTemplate>(workoutId);

        workout.ShouldNotBeNull();
        workout!.Id.ShouldBe(workoutId);
        workout.Name.ShouldBe(command.Name);
        workout.Notes.ShouldBe(command.Notes);
        workout.Location.ShouldBe(command.Location);
        workout.UserId.ShouldBe(userId);
        workout.CreatedBy.ShouldBe(userId);
        workout.Created.ShouldBe(DateTime.Now, TimeSpan.FromMilliseconds(10000));
        workout.LastModifiedBy.ShouldBe(userId);
        workout.LastModified.ShouldBe(DateTime.Now, TimeSpan.FromMilliseconds(10000));
    }

    [Test]
    public async Task ShouldRequireName()
    {
        await RunAsDefaultUserAsync();

        var command = new CreateWorkoutTemplateCommand();

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequireNonEmptyName()
    {
        await RunAsDefaultUserAsync();

        var command = new CreateWorkoutTemplateCommand
        {
            Name = ""
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldAllowOptionalNotesAndLocation()
    {
        var userId = await RunAsDefaultUserAsync();

        var command = new CreateWorkoutTemplateCommand
        {
            Name = "Pull Day"
        };

        var workoutId = await SendAsync(command);

        var workout = await FindAsync<WorkoutTemplate>(workoutId);

        workout.ShouldNotBeNull();
        workout!.Name.ShouldBe("Pull Day");
        workout.Notes.ShouldBeNull();
        workout.Location.ShouldBeNull();
    }

    [Test]
    public async Task ShouldSetCreatedAndModifiedDates()
    {
        var userId = await RunAsDefaultUserAsync();

        var beforeCreate = DateTime.Now;

        var workoutId = await SendAsync(new CreateWorkoutTemplateCommand
        {
            Name = "Leg Day"
        });

        var afterCreate = DateTime.Now;

        var workout = await FindAsync<WorkoutTemplate>(workoutId);

        workout.ShouldNotBeNull();
        workout!.Created.ShouldBeGreaterThanOrEqualTo(beforeCreate);
        workout.Created.ShouldBeLessThanOrEqualTo(afterCreate);
        workout.LastModified.ShouldBeGreaterThanOrEqualTo(beforeCreate);
        workout.LastModified.ShouldBeLessThanOrEqualTo(afterCreate);
    }

    [Test]
    public async Task ShouldAssociateWithCurrentUser()
    {
        var userId = await RunAsDefaultUserAsync();

        var workoutId = await SendAsync(new CreateWorkoutTemplateCommand
        {
            Name = "Full Body"
        });

        var workout = await FindAsync<WorkoutTemplate>(workoutId);

        workout.ShouldNotBeNull();
        workout!.UserId.ShouldBe(userId);
        workout.CreatedBy.ShouldBe(userId);
        workout.LastModifiedBy.ShouldBe(userId);
    }
}
