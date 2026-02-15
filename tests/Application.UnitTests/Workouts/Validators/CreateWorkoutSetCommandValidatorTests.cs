using Hoist.Application.Workouts.Commands.CreateWorkoutSet;

namespace Hoist.Application.UnitTests.Workouts.Validators;

public class CreateWorkoutSetCommandValidatorTests
{
    private CreateWorkoutSetCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new CreateWorkoutSetCommandValidator();
    }

    [Test]
    public async Task ShouldHaveErrorWhenWorkoutIdIsZero()
    {
        var command = new CreateWorkoutSetCommand
        {
            WorkoutId = 0,
            WorkoutExerciseId = 1,
            Weight = 100
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "WorkoutId");
    }

    [Test]
    public async Task ShouldHaveErrorWhenWorkoutExerciseIdIsZero()
    {
        var command = new CreateWorkoutSetCommand
        {
            WorkoutId = 1,
            WorkoutExerciseId = 0,
            Weight = 100
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "WorkoutExerciseId");
    }

    [Test]
    public async Task ShouldHaveErrorWhenNoMeasurementFieldsProvided()
    {
        var command = new CreateWorkoutSetCommand
        {
            WorkoutId = 1,
            WorkoutExerciseId = 1,
            Weight = null,
            Reps = null,
            Duration = null,
            Distance = null,
            Bodyweight = null,
            BandColor = null
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage == "At least one measurement field must be provided");
    }

    [Test]
    public async Task ShouldPassWhenOnlyWeightProvided()
    {
        var command = new CreateWorkoutSetCommand
        {
            WorkoutId = 1,
            WorkoutExerciseId = 1,
            Weight = 100,
            Reps = null,
            Duration = null,
            Distance = null,
            Bodyweight = null,
            BandColor = null
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldPassWhenOnlyRepsProvided()
    {
        var command = new CreateWorkoutSetCommand
        {
            WorkoutId = 1,
            WorkoutExerciseId = 1,
            Weight = null,
            Reps = 10,
            Duration = null,
            Distance = null,
            Bodyweight = null,
            BandColor = null
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldPassWhenOnlyDurationProvided()
    {
        var command = new CreateWorkoutSetCommand
        {
            WorkoutId = 1,
            WorkoutExerciseId = 1,
            Weight = null,
            Reps = null,
            Duration = 60,
            Distance = null,
            Bodyweight = null,
            BandColor = null
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldPassWhenOnlyDistanceProvided()
    {
        var command = new CreateWorkoutSetCommand
        {
            WorkoutId = 1,
            WorkoutExerciseId = 1,
            Weight = null,
            Reps = null,
            Duration = null,
            Distance = 5.0m,
            Bodyweight = null,
            BandColor = null
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldPassWhenOnlyBodyweightProvided()
    {
        var command = new CreateWorkoutSetCommand
        {
            WorkoutId = 1,
            WorkoutExerciseId = 1,
            Weight = null,
            Reps = null,
            Duration = null,
            Distance = null,
            Bodyweight = 75.5m,
            BandColor = null
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldPassWhenOnlyBandColorProvided()
    {
        var command = new CreateWorkoutSetCommand
        {
            WorkoutId = 1,
            WorkoutExerciseId = 1,
            Weight = null,
            Reps = null,
            Duration = null,
            Distance = null,
            Bodyweight = null,
            BandColor = "Red"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldHaveErrorWhenWeightIsNegative()
    {
        var command = new CreateWorkoutSetCommand
        {
            WorkoutId = 1,
            WorkoutExerciseId = 1,
            Weight = -10
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Weight");
    }

    [Test]
    public async Task ShouldHaveErrorWhenRepsIsNegative()
    {
        var command = new CreateWorkoutSetCommand
        {
            WorkoutId = 1,
            WorkoutExerciseId = 1,
            Reps = -5
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Reps");
    }

    [Test]
    public async Task ShouldHaveErrorWhenDurationIsNegative()
    {
        var command = new CreateWorkoutSetCommand
        {
            WorkoutId = 1,
            WorkoutExerciseId = 1,
            Duration = -30
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Duration");
    }

    [Test]
    public async Task ShouldHaveErrorWhenDistanceIsNegative()
    {
        var command = new CreateWorkoutSetCommand
        {
            WorkoutId = 1,
            WorkoutExerciseId = 1,
            Distance = -5.5m
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Distance");
    }

    [Test]
    public async Task ShouldHaveErrorWhenBodyweightIsNegative()
    {
        var command = new CreateWorkoutSetCommand
        {
            WorkoutId = 1,
            WorkoutExerciseId = 1,
            Bodyweight = -75.0m
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Bodyweight");
    }

    [Test]
    public async Task ShouldPassWhenAllFieldsAreValid()
    {
        var command = new CreateWorkoutSetCommand
        {
            WorkoutId = 1,
            WorkoutExerciseId = 1,
            Weight = 100,
            Reps = 10,
            Duration = 60,
            Distance = 5.0m,
            Bodyweight = 75.5m,
            BandColor = "Red",
            WeightUnit = "kg",
            DistanceUnit = "km"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }
}
