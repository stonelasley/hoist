using Hoist.Application.UserPreferences.Commands.UpsertUserPreferences;
using Hoist.Application.UserPreferences.Queries.GetUserPreferences;

namespace Hoist.Application.FunctionalTests.UserPreferences.Queries;

using static Testing;

public class GetUserPreferencesTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnDefaultsWhenNoRecordExists()
    {
        await RunAsDefaultUserAsync();

        var result = await SendAsync(new GetUserPreferencesQuery());

        result.ShouldNotBeNull();
        result.WeightUnit.ShouldBe("Lbs");
        result.DistanceUnit.ShouldBe("Miles");
        result.Bodyweight.ShouldBeNull();
    }

    [Test]
    public async Task ShouldReturnSavedPreferencesAfterUpsert()
    {
        await RunAsDefaultUserAsync();

        await SendAsync(new UpsertUserPreferencesCommand
        {
            WeightUnit = "Kg",
            DistanceUnit = "Kilometers",
            Bodyweight = 85.5m
        });

        var result = await SendAsync(new GetUserPreferencesQuery());

        result.ShouldNotBeNull();
        result.WeightUnit.ShouldBe("Kg");
        result.DistanceUnit.ShouldBe("Kilometers");
        result.Bodyweight.ShouldBe(85.5m);
    }
}
