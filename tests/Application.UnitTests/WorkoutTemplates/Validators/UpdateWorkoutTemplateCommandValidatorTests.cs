using Hoist.Application.WorkoutTemplates.Commands.UpdateWorkoutTemplate;

namespace Hoist.Application.UnitTests.WorkoutTemplates.Validators;

public class UpdateWorkoutTemplateCommandValidatorTests
{
    private UpdateWorkoutTemplateCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new UpdateWorkoutTemplateCommandValidator();
    }

    [Test]
    public async Task ShouldHaveErrorWhenNameIsEmpty()
    {
        var command = new UpdateWorkoutTemplateCommand
        {
            Id = 1,
            Name = "",
            Notes = "Test notes",
            Location = "Test location"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }

    [Test]
    public async Task ShouldHaveErrorWhenNameIsWhitespace()
    {
        var command = new UpdateWorkoutTemplateCommand
        {
            Id = 1,
            Name = "   ",
            Notes = "Test notes",
            Location = "Test location"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }

    [Test]
    public async Task ShouldHaveErrorWhenNameExceedsMaxLength()
    {
        var command = new UpdateWorkoutTemplateCommand
        {
            Id = 1,
            Name = new string('A', 201),
            Notes = "Test notes",
            Location = "Test location"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }

    [Test]
    public async Task ShouldPassValidationWhenNameIsAtMaxLength()
    {
        var command = new UpdateWorkoutTemplateCommand
        {
            Id = 1,
            Name = new string('A', 200),
            Notes = "Test notes",
            Location = "Test location"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldHaveErrorWhenNotesExceedsMaxLength()
    {
        var command = new UpdateWorkoutTemplateCommand
        {
            Id = 1,
            Name = "Test workout",
            Notes = new string('A', 2001),
            Location = "Test location"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Notes");
    }

    [Test]
    public async Task ShouldPassValidationWhenNotesIsAtMaxLength()
    {
        var command = new UpdateWorkoutTemplateCommand
        {
            Id = 1,
            Name = "Test workout",
            Notes = new string('A', 2000),
            Location = "Test location"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldHaveErrorWhenLocationExceedsMaxLength()
    {
        var command = new UpdateWorkoutTemplateCommand
        {
            Id = 1,
            Name = "Test workout",
            Notes = "Test notes",
            Location = new string('A', 201)
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Location");
    }

    [Test]
    public async Task ShouldPassValidationWhenLocationIsAtMaxLength()
    {
        var command = new UpdateWorkoutTemplateCommand
        {
            Id = 1,
            Name = "Test workout",
            Notes = "Test notes",
            Location = new string('A', 200)
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldPassValidationWhenNotesIsNull()
    {
        var command = new UpdateWorkoutTemplateCommand
        {
            Id = 1,
            Name = "Test workout",
            Notes = null,
            Location = "Test location"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldPassValidationWhenLocationIsNull()
    {
        var command = new UpdateWorkoutTemplateCommand
        {
            Id = 1,
            Name = "Test workout",
            Notes = "Test notes",
            Location = null
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldPassValidationWhenAllFieldsAreValid()
    {
        var command = new UpdateWorkoutTemplateCommand
        {
            Id = 1,
            Name = "Morning Workout",
            Notes = "Full body workout routine",
            Location = "Main Gym"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }
}
