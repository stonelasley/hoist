using Hoist.Application.Workouts.Queries.GetRecentWorkouts;
using Hoist.Domain.Entities;
using Hoist.Domain.Enums;

namespace Hoist.Application.FunctionalTests.Workouts.Queries;

using static Testing;

public class GetRecentWorkoutsTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnMax3Workouts()
    {
        var userId = await RunAsDefaultUserAsync();

        // Create 4 completed workouts with different end dates
        var workout1 = new Workout
        {
            TemplateName = "Workout 1",
            Status = WorkoutStatus.Completed,
            StartedAt = DateTimeOffset.UtcNow.AddDays(-4),
            EndedAt = DateTimeOffset.UtcNow.AddDays(-4).AddHours(1),
            UserId = userId
        };

        var workout2 = new Workout
        {
            TemplateName = "Workout 2",
            Status = WorkoutStatus.Completed,
            StartedAt = DateTimeOffset.UtcNow.AddDays(-3),
            EndedAt = DateTimeOffset.UtcNow.AddDays(-3).AddHours(1),
            UserId = userId
        };

        var workout3 = new Workout
        {
            TemplateName = "Workout 3",
            Status = WorkoutStatus.Completed,
            StartedAt = DateTimeOffset.UtcNow.AddDays(-2),
            EndedAt = DateTimeOffset.UtcNow.AddDays(-2).AddHours(1),
            UserId = userId
        };

        var workout4 = new Workout
        {
            TemplateName = "Workout 4",
            Status = WorkoutStatus.Completed,
            StartedAt = DateTimeOffset.UtcNow.AddDays(-1),
            EndedAt = DateTimeOffset.UtcNow.AddDays(-1).AddHours(1),
            UserId = userId
        };

        await AddAsync(workout1);
        await AddAsync(workout2);
        await AddAsync(workout3);
        await AddAsync(workout4);

        var query = new GetRecentWorkoutsQuery();
        var result = await SendAsync(query);

        result.Count.ShouldBe(3);
    }

    [Test]
    public async Task ShouldOrderByEndedAtDescending()
    {
        var userId = await RunAsDefaultUserAsync();

        // Create 3 completed workouts with different end dates
        var workout1 = new Workout
        {
            TemplateName = "Oldest Workout",
            Status = WorkoutStatus.Completed,
            StartedAt = DateTimeOffset.UtcNow.AddDays(-3),
            EndedAt = DateTimeOffset.UtcNow.AddDays(-3).AddHours(1),
            UserId = userId
        };

        var workout2 = new Workout
        {
            TemplateName = "Middle Workout",
            Status = WorkoutStatus.Completed,
            StartedAt = DateTimeOffset.UtcNow.AddDays(-2),
            EndedAt = DateTimeOffset.UtcNow.AddDays(-2).AddHours(1),
            UserId = userId
        };

        var workout3 = new Workout
        {
            TemplateName = "Newest Workout",
            Status = WorkoutStatus.Completed,
            StartedAt = DateTimeOffset.UtcNow.AddDays(-1),
            EndedAt = DateTimeOffset.UtcNow.AddDays(-1).AddHours(1),
            UserId = userId
        };

        await AddAsync(workout1);
        await AddAsync(workout2);
        await AddAsync(workout3);

        var query = new GetRecentWorkoutsQuery();
        var result = await SendAsync(query);

        result.Count.ShouldBe(3);
        result[0].TemplateName.ShouldBe("Newest Workout");
        result[1].TemplateName.ShouldBe("Middle Workout");
        result[2].TemplateName.ShouldBe("Oldest Workout");
    }

    [Test]
    public async Task ShouldExcludeInProgressWorkouts()
    {
        var userId = await RunAsDefaultUserAsync();

        // Create 1 in-progress workout
        var inProgressWorkout = new Workout
        {
            TemplateName = "In Progress Workout",
            Status = WorkoutStatus.InProgress,
            StartedAt = DateTimeOffset.UtcNow,
            UserId = userId
        };

        // Create 2 completed workouts
        var completedWorkout1 = new Workout
        {
            TemplateName = "Completed Workout 1",
            Status = WorkoutStatus.Completed,
            StartedAt = DateTimeOffset.UtcNow.AddDays(-2),
            EndedAt = DateTimeOffset.UtcNow.AddDays(-2).AddHours(1),
            UserId = userId
        };

        var completedWorkout2 = new Workout
        {
            TemplateName = "Completed Workout 2",
            Status = WorkoutStatus.Completed,
            StartedAt = DateTimeOffset.UtcNow.AddDays(-1),
            EndedAt = DateTimeOffset.UtcNow.AddDays(-1).AddHours(1),
            UserId = userId
        };

        await AddAsync(inProgressWorkout);
        await AddAsync(completedWorkout1);
        await AddAsync(completedWorkout2);

        var query = new GetRecentWorkoutsQuery();
        var result = await SendAsync(query);

        result.Count.ShouldBe(2);
        result.ShouldNotContain(w => w.TemplateName == "In Progress Workout");
        result.ShouldContain(w => w.TemplateName == "Completed Workout 1");
        result.ShouldContain(w => w.TemplateName == "Completed Workout 2");
    }

    [Test]
    public async Task ShouldReturnEmptyForNewUser()
    {
        await RunAsDefaultUserAsync();

        var query = new GetRecentWorkoutsQuery();
        var result = await SendAsync(query);

        result.ShouldBeEmpty();
    }
}
