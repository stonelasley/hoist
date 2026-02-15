using Hoist.Application.Common.Exceptions;
using Hoist.Application.UserPreferences.Commands.UpsertUserPreferences;
using Hoist.Application.UserPreferences.Queries.GetUserPreferences;
using Hoist.Domain.Entities;

namespace Hoist.Application.FunctionalTests.UserPreferences.Commands;

using static Testing;

public class UpsertUserPreferencesTests : BaseTestFixture
{
    [Test]
    public async Task ShouldCreateOnFirstCall()
    {
        var userId = await RunAsDefaultUserAsync();

        var command = new UpsertUserPreferencesCommand
        {
            WeightUnit = "Kg",
            DistanceUnit = "Kilometers",
            Bodyweight = 75.0m
        };

        await SendAsync(command);

        var result = await SendAsync(new GetUserPreferencesQuery());

        result.ShouldNotBeNull();
        result.WeightUnit.ShouldBe("Kg");
        result.DistanceUnit.ShouldBe("Kilometers");
        result.Bodyweight.ShouldBe(75.0m);
    }

    [Test]
    public async Task ShouldUpdateOnSubsequentCall()
    {
        await RunAsDefaultUserAsync();

        var createCommand = new UpsertUserPreferencesCommand
        {
            WeightUnit = "Lbs",
            DistanceUnit = "Miles",
            Bodyweight = 180.0m
        };

        await SendAsync(createCommand);

        var updateCommand = new UpsertUserPreferencesCommand
        {
            WeightUnit = "Kg",
            DistanceUnit = "Kilometers",
            Bodyweight = 85.0m
        };

        await SendAsync(updateCommand);

        var result = await SendAsync(new GetUserPreferencesQuery());

        result.ShouldNotBeNull();
        result.WeightUnit.ShouldBe("Kg");
        result.DistanceUnit.ShouldBe("Kilometers");
        result.Bodyweight.ShouldBe(85.0m);
    }

    [Test]
    public async Task ShouldRejectInvalidWeightUnit()
    {
        await RunAsDefaultUserAsync();

        var command = new UpsertUserPreferencesCommand
        {
            WeightUnit = "InvalidUnit",
            DistanceUnit = "Miles"
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRejectNegativeBodyweight()
    {
        await RunAsDefaultUserAsync();

        var command = new UpsertUserPreferencesCommand
        {
            WeightUnit = "Kg",
            DistanceUnit = "Kilometers",
            Bodyweight = -10.0m
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldAcceptNullBodyweight()
    {
        await RunAsDefaultUserAsync();

        var command = new UpsertUserPreferencesCommand
        {
            WeightUnit = "Kg",
            DistanceUnit = "Kilometers",
            Bodyweight = null
        };

        await SendAsync(command);

        var result = await SendAsync(new GetUserPreferencesQuery());

        result.ShouldNotBeNull();
        result.Bodyweight.ShouldBeNull();
    }

    [Test]
    public async Task ShouldRejectEmptyWeightUnit()
    {
        await RunAsDefaultUserAsync();

        var command = new UpsertUserPreferencesCommand
        {
            WeightUnit = "",
            DistanceUnit = "Miles"
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRejectEmptyDistanceUnit()
    {
        await RunAsDefaultUserAsync();

        var command = new UpsertUserPreferencesCommand
        {
            WeightUnit = "Kg",
            DistanceUnit = ""
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRejectInvalidDistanceUnit()
    {
        await RunAsDefaultUserAsync();

        var command = new UpsertUserPreferencesCommand
        {
            WeightUnit = "Kg",
            DistanceUnit = "InvalidUnit"
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }
}
