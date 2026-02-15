using Hoist.Application.UserPreferences.Commands.UpsertUserPreferences;

namespace Hoist.Application.UnitTests.UserPreferences.Validators;

public class UpsertUserPreferencesCommandValidatorTests
{
    private UpsertUserPreferencesCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new UpsertUserPreferencesCommandValidator();
    }

    [Test]
    public async Task ShouldHaveErrorWhenWeightUnitIsEmpty()
    {
        var command = new UpsertUserPreferencesCommand
        {
            WeightUnit = "",
            DistanceUnit = "Miles"
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "WeightUnit");
    }

    [Test]
    public async Task ShouldHaveErrorWhenWeightUnitIsInvalid()
    {
        var command = new UpsertUserPreferencesCommand
        {
            WeightUnit = "InvalidUnit",
            DistanceUnit = "Miles"
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "WeightUnit");
    }

    [Test]
    public async Task ShouldPassWhenWeightUnitIsLbs()
    {
        var command = new UpsertUserPreferencesCommand
        {
            WeightUnit = "Lbs",
            DistanceUnit = "Miles"
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldPassWhenWeightUnitIsKg()
    {
        var command = new UpsertUserPreferencesCommand
        {
            WeightUnit = "Kg",
            DistanceUnit = "Miles"
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldHaveErrorWhenDistanceUnitIsEmpty()
    {
        var command = new UpsertUserPreferencesCommand
        {
            WeightUnit = "Kg",
            DistanceUnit = ""
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "DistanceUnit");
    }

    [Test]
    public async Task ShouldHaveErrorWhenDistanceUnitIsInvalid()
    {
        var command = new UpsertUserPreferencesCommand
        {
            WeightUnit = "Kg",
            DistanceUnit = "InvalidUnit"
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "DistanceUnit");
    }

    [Test]
    public async Task ShouldPassForAllValidDistanceUnits()
    {
        var validUnits = new[] { "Miles", "Kilometers", "Meters", "Yards" };

        foreach (var unit in validUnits)
        {
            var command = new UpsertUserPreferencesCommand
            {
                WeightUnit = "Kg",
                DistanceUnit = unit
            };

            var result = await _validator.ValidateAsync(command);

            result.IsValid.ShouldBeTrue();
        }
    }

    [Test]
    public async Task ShouldHaveErrorWhenBodyweightIsNegative()
    {
        var command = new UpsertUserPreferencesCommand
        {
            WeightUnit = "Kg",
            DistanceUnit = "Kilometers",
            Bodyweight = -10.0m
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Bodyweight");
    }

    [Test]
    public async Task ShouldHaveErrorWhenBodyweightIsZero()
    {
        var command = new UpsertUserPreferencesCommand
        {
            WeightUnit = "Kg",
            DistanceUnit = "Kilometers",
            Bodyweight = 0m
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Bodyweight");
    }

    [Test]
    public async Task ShouldPassWhenBodyweightIsNull()
    {
        var command = new UpsertUserPreferencesCommand
        {
            WeightUnit = "Kg",
            DistanceUnit = "Kilometers",
            Bodyweight = null
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldPassWhenBodyweightIsPositive()
    {
        var command = new UpsertUserPreferencesCommand
        {
            WeightUnit = "Kg",
            DistanceUnit = "Kilometers",
            Bodyweight = 75.5m
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldPassWithAllValidFields()
    {
        var command = new UpsertUserPreferencesCommand
        {
            WeightUnit = "Kg",
            DistanceUnit = "Kilometers",
            Bodyweight = 85.5m
        };

        var result = await _validator.ValidateAsync(command);

        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }
}
