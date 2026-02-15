using Hoist.Application.Workouts.Commands.StartWorkout;

namespace Hoist.Application.UnitTests.Workouts.Validators;

public class StartWorkoutCommandValidatorTests
{
    private StartWorkoutCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new StartWorkoutCommandValidator();
    }

    [Test]
    public async Task ShouldHaveErrorWhenWorkoutTemplateIdIsZero()
    {
        var command = new StartWorkoutCommand
        {
            WorkoutTemplateId = 0
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "WorkoutTemplateId");
    }

    [Test]
    public async Task ShouldHaveErrorWhenWorkoutTemplateIdIsNegative()
    {
        var command = new StartWorkoutCommand
        {
            WorkoutTemplateId = -1
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "WorkoutTemplateId");
    }

    [Test]
    public async Task ShouldPassWhenWorkoutTemplateIdIsPositive()
    {
        var command = new StartWorkoutCommand
        {
            WorkoutTemplateId = 1
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }
}
