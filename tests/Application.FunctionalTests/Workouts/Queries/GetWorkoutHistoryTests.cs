using Hoist.Application.Workouts.Queries.GetWorkoutHistory;
using Hoist.Domain.Entities;
using Hoist.Domain.Enums;

namespace Hoist.Application.FunctionalTests.Workouts.Queries;

using static Testing;

public class GetWorkoutHistoryTests : BaseTestFixture
{
    private async Task<Workout> CreateCompletedWorkout(string userId, string templateName, DateTimeOffset endedAt, int? rating = null, string? notes = null, int? locationId = null, string? locationName = null)
    {
        var workout = new Workout
        {
            TemplateName = templateName,
            Status = WorkoutStatus.Completed,
            StartedAt = endedAt.AddHours(-1),
            EndedAt = endedAt,
            Rating = rating,
            Notes = notes,
            LocationId = locationId,
            LocationName = locationName,
            UserId = userId
        };
        await AddAsync(workout);
        return workout;
    }

    [Test]
    public async Task ShouldReturnDefaultSortByDateDesc()
    {
        var userId = await RunAsDefaultUserAsync();

        // Create 3 completed workouts with different end dates
        await CreateCompletedWorkout(userId, "Oldest", DateTimeOffset.UtcNow.AddDays(-3));
        await CreateCompletedWorkout(userId, "Middle", DateTimeOffset.UtcNow.AddDays(-2));
        await CreateCompletedWorkout(userId, "Newest", DateTimeOffset.UtcNow.AddDays(-1));

        var query = new GetWorkoutHistoryQuery();
        var result = await SendAsync(query);

        result.Items.Count.ShouldBe(3);
        result.Items[0].TemplateName.ShouldBe("Newest");
        result.Items[1].TemplateName.ShouldBe("Middle");
        result.Items[2].TemplateName.ShouldBe("Oldest");
        result.NextCursor.ShouldBeNull();
    }

    [Test]
    public async Task ShouldSortByRating()
    {
        var userId = await RunAsDefaultUserAsync();

        // Create workouts with different ratings
        await CreateCompletedWorkout(userId, "Rating 3", DateTimeOffset.UtcNow.AddDays(-3), rating: 3);
        await CreateCompletedWorkout(userId, "Rating 5", DateTimeOffset.UtcNow.AddDays(-2), rating: 5);
        await CreateCompletedWorkout(userId, "Rating 1", DateTimeOffset.UtcNow.AddDays(-1), rating: 1);

        var query = new GetWorkoutHistoryQuery { SortBy = "rating", SortDirection = "desc" };
        var result = await SendAsync(query);

        result.Items.Count.ShouldBe(3);
        result.Items[0].TemplateName.ShouldBe("Rating 5");
        result.Items[1].TemplateName.ShouldBe("Rating 3");
        result.Items[2].TemplateName.ShouldBe("Rating 1");
    }

    [Test]
    public async Task ShouldSortAscending()
    {
        var userId = await RunAsDefaultUserAsync();

        await CreateCompletedWorkout(userId, "Oldest", DateTimeOffset.UtcNow.AddDays(-3));
        await CreateCompletedWorkout(userId, "Middle", DateTimeOffset.UtcNow.AddDays(-2));
        await CreateCompletedWorkout(userId, "Newest", DateTimeOffset.UtcNow.AddDays(-1));

        var query = new GetWorkoutHistoryQuery { SortBy = "date", SortDirection = "asc" };
        var result = await SendAsync(query);

        result.Items.Count.ShouldBe(3);
        result.Items[0].TemplateName.ShouldBe("Oldest");
        result.Items[1].TemplateName.ShouldBe("Middle");
        result.Items[2].TemplateName.ShouldBe("Newest");
    }

    [Test]
    public async Task ShouldFilterByLocation()
    {
        var userId = await RunAsDefaultUserAsync();

        // Create actual locations
        var location1 = new Location { Name = "Gym A", UserId = userId };
        var location2 = new Location { Name = "Gym B", UserId = userId };
        await AddAsync(location1);
        await AddAsync(location2);

        await CreateCompletedWorkout(userId, "Location 1", DateTimeOffset.UtcNow.AddDays(-3), locationId: location1.Id, locationName: "Gym A");
        await CreateCompletedWorkout(userId, "Location 2", DateTimeOffset.UtcNow.AddDays(-2), locationId: location2.Id, locationName: "Gym B");
        await CreateCompletedWorkout(userId, "Location 1 Again", DateTimeOffset.UtcNow.AddDays(-1), locationId: location1.Id, locationName: "Gym A");

        var query = new GetWorkoutHistoryQuery { LocationId = location1.Id };
        var result = await SendAsync(query);

        result.Items.Count.ShouldBe(2);
        result.Items.ShouldAllBe(w => w.LocationName == "Gym A");
    }

    [Test]
    public async Task ShouldFilterByMinRating()
    {
        var userId = await RunAsDefaultUserAsync();

        await CreateCompletedWorkout(userId, "Rating 2", DateTimeOffset.UtcNow.AddDays(-4), rating: 2);
        await CreateCompletedWorkout(userId, "Rating 3", DateTimeOffset.UtcNow.AddDays(-3), rating: 3);
        await CreateCompletedWorkout(userId, "Rating 4", DateTimeOffset.UtcNow.AddDays(-2), rating: 4);
        await CreateCompletedWorkout(userId, "Rating 5", DateTimeOffset.UtcNow.AddDays(-1), rating: 5);

        var query = new GetWorkoutHistoryQuery { MinRating = 4 };
        var result = await SendAsync(query);

        result.Items.Count.ShouldBe(2);
        result.Items.ShouldAllBe(w => w.Rating >= 4);
    }

    [Test]
    public async Task ShouldSearchByNotes()
    {
        var userId = await RunAsDefaultUserAsync();

        await CreateCompletedWorkout(userId, "Workout 1", DateTimeOffset.UtcNow.AddDays(-3), notes: "Great session with heavy squats");
        await CreateCompletedWorkout(userId, "Workout 2", DateTimeOffset.UtcNow.AddDays(-2), notes: "Light cardio day");
        await CreateCompletedWorkout(userId, "Workout 3", DateTimeOffset.UtcNow.AddDays(-1), notes: "Personal record on bench press");

        var query = new GetWorkoutHistoryQuery { Search = "press" };
        var result = await SendAsync(query);

        result.Items.Count.ShouldBe(1);
        result.Items[0].TemplateName.ShouldBe("Workout 3");
    }

    [Test]
    public async Task ShouldSearchCaseInsensitive()
    {
        var userId = await RunAsDefaultUserAsync();

        await CreateCompletedWorkout(userId, "Workout 1", DateTimeOffset.UtcNow.AddDays(-2), notes: "Great SESSION");
        await CreateCompletedWorkout(userId, "Workout 2", DateTimeOffset.UtcNow.AddDays(-1), notes: "Other notes");

        var query = new GetWorkoutHistoryQuery { Search = "session" };
        var result = await SendAsync(query);

        result.Items.Count.ShouldBe(1);
        result.Items[0].TemplateName.ShouldBe("Workout 1");
    }

    [Test]
    public async Task ShouldPaginateWithCursor()
    {
        var userId = await RunAsDefaultUserAsync();

        // Create 5 workouts
        await CreateCompletedWorkout(userId, "Workout 1", DateTimeOffset.UtcNow.AddDays(-5));
        await CreateCompletedWorkout(userId, "Workout 2", DateTimeOffset.UtcNow.AddDays(-4));
        await CreateCompletedWorkout(userId, "Workout 3", DateTimeOffset.UtcNow.AddDays(-3));
        await CreateCompletedWorkout(userId, "Workout 4", DateTimeOffset.UtcNow.AddDays(-2));
        await CreateCompletedWorkout(userId, "Workout 5", DateTimeOffset.UtcNow.AddDays(-1));

        // Get first page (page size 2)
        var firstPageQuery = new GetWorkoutHistoryQuery { PageSize = 2 };
        var firstPage = await SendAsync(firstPageQuery);

        firstPage.Items.Count.ShouldBe(2);
        firstPage.Items[0].TemplateName.ShouldBe("Workout 5");
        firstPage.Items[1].TemplateName.ShouldBe("Workout 4");
        firstPage.NextCursor.ShouldNotBeNull();

        // Get second page using cursor
        var secondPageQuery = new GetWorkoutHistoryQuery { PageSize = 2, Cursor = firstPage.NextCursor };
        var secondPage = await SendAsync(secondPageQuery);

        secondPage.Items.Count.ShouldBe(2);
        secondPage.Items[0].TemplateName.ShouldBe("Workout 3");
        secondPage.Items[1].TemplateName.ShouldBe("Workout 2");
        secondPage.NextCursor.ShouldNotBeNull();

        // Get third (final) page
        var thirdPageQuery = new GetWorkoutHistoryQuery { PageSize = 2, Cursor = secondPage.NextCursor };
        var thirdPage = await SendAsync(thirdPageQuery);

        thirdPage.Items.Count.ShouldBe(1);
        thirdPage.Items[0].TemplateName.ShouldBe("Workout 1");
        thirdPage.NextCursor.ShouldBeNull();
    }

    [Test]
    public async Task ShouldReturnEmptyForNewUser()
    {
        await RunAsDefaultUserAsync();

        var query = new GetWorkoutHistoryQuery();
        var result = await SendAsync(query);

        result.Items.ShouldBeEmpty();
        result.NextCursor.ShouldBeNull();
    }

    [Test]
    public async Task ShouldExcludeInProgressWorkouts()
    {
        var userId = await RunAsDefaultUserAsync();

        // Create in-progress workout
        var inProgressWorkout = new Workout
        {
            TemplateName = "In Progress",
            Status = WorkoutStatus.InProgress,
            StartedAt = DateTimeOffset.UtcNow,
            UserId = userId
        };
        await AddAsync(inProgressWorkout);

        // Create completed workout
        await CreateCompletedWorkout(userId, "Completed", DateTimeOffset.UtcNow.AddDays(-1));

        var query = new GetWorkoutHistoryQuery();
        var result = await SendAsync(query);

        result.Items.Count.ShouldBe(1);
        result.Items[0].TemplateName.ShouldBe("Completed");
    }

    [Test]
    public async Task ShouldCombineFiltersAndSort()
    {
        var userId = await RunAsDefaultUserAsync();

        // Create actual locations
        var location1 = new Location { Name = "Gym A", UserId = userId };
        var location2 = new Location { Name = "Gym B", UserId = userId };
        await AddAsync(location1);
        await AddAsync(location2);

        await CreateCompletedWorkout(userId, "Loc1 Rating5", DateTimeOffset.UtcNow.AddDays(-4), rating: 5, locationId: location1.Id, locationName: "Gym A", notes: "Great workout");
        await CreateCompletedWorkout(userId, "Loc1 Rating3", DateTimeOffset.UtcNow.AddDays(-3), rating: 3, locationId: location1.Id, locationName: "Gym A", notes: "Good workout");
        await CreateCompletedWorkout(userId, "Loc2 Rating4", DateTimeOffset.UtcNow.AddDays(-2), rating: 4, locationId: location2.Id, locationName: "Gym B", notes: "Nice workout");
        await CreateCompletedWorkout(userId, "Loc1 Rating4", DateTimeOffset.UtcNow.AddDays(-1), rating: 4, locationId: location1.Id, locationName: "Gym A", notes: "Great session");

        var query = new GetWorkoutHistoryQuery
        {
            LocationId = location1.Id,
            MinRating = 4,
            Search = "Great",
            SortBy = "rating",
            SortDirection = "desc"
        };
        var result = await SendAsync(query);

        result.Items.Count.ShouldBe(2);
        result.Items[0].TemplateName.ShouldBe("Loc1 Rating5");
        result.Items[1].TemplateName.ShouldBe("Loc1 Rating4");
    }
}
