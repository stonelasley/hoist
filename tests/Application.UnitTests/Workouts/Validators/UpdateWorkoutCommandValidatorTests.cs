using Hoist.Application.Workouts.Commands.UpdateWorkout;

namespace Hoist.Application.UnitTests.Workouts.Validators;

public class UpdateWorkoutCommandValidatorTests
{
    private UpdateWorkoutCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new UpdateWorkoutCommandValidator();
    }

    [Test]
    public async Task ShouldHaveErrorWhenIdIsZero()
    {
        var command = new UpdateWorkoutCommand
        {
            Id = 0
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Id");
    }

    [Test]
    public async Task ShouldHaveErrorWhenRatingIsLessThan1()
    {
        var command = new UpdateWorkoutCommand
        {
            Id = 1,
            Rating = 0
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Rating");
    }

    [Test]
    public async Task ShouldHaveErrorWhenRatingIsGreaterThan5()
    {
        var command = new UpdateWorkoutCommand
        {
            Id = 1,
            Rating = 6
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Rating");
    }

    [Test]
    public async Task ShouldPassWhenRatingIs1()
    {
        var command = new UpdateWorkoutCommand
        {
            Id = 1,
            Rating = 1
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldPassWhenRatingIs5()
    {
        var command = new UpdateWorkoutCommand
        {
            Id = 1,
            Rating = 5
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldPassWhenRatingIsNull()
    {
        var command = new UpdateWorkoutCommand
        {
            Id = 1,
            Rating = null
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldHaveErrorWhenNotesExceedsMaxLength()
    {
        var command = new UpdateWorkoutCommand
        {
            Id = 1,
            Notes = new string('A', 2001)
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Notes");
    }

    [Test]
    public async Task ShouldPassWhenNotesIsAtMaxLength()
    {
        var command = new UpdateWorkoutCommand
        {
            Id = 1,
            Notes = new string('A', 2000)
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldHaveErrorWhenEndedAtBeforeStartedAt()
    {
        var startedAt = DateTimeOffset.UtcNow;
        var endedAt = startedAt.AddHours(-1);

        var command = new UpdateWorkoutCommand
        {
            Id = 1,
            StartedAt = startedAt,
            EndedAt = endedAt
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("EndedAt must be after StartedAt"));
    }

    [Test]
    public async Task ShouldPassWhenEndedAtAfterStartedAt()
    {
        var startedAt = DateTimeOffset.UtcNow;
        var endedAt = startedAt.AddHours(1);

        var command = new UpdateWorkoutCommand
        {
            Id = 1,
            StartedAt = startedAt,
            EndedAt = endedAt
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }
}
