using Hoist.Application.WorkoutTemplates.Commands.UpdateWorkoutTemplateExercises;

namespace Hoist.Application.UnitTests.WorkoutTemplates.Validators;

public class UpdateWorkoutTemplateExercisesCommandValidatorTests
{
    private UpdateWorkoutTemplateExercisesCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new UpdateWorkoutTemplateExercisesCommandValidator();
    }

    [Test]
    public async Task ShouldHaveErrorWhenWorkoutTemplateIdIsZero()
    {
        var command = new UpdateWorkoutTemplateExercisesCommand
        {
            WorkoutTemplateId = 0,
            Exercises = []
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "WorkoutTemplateId");
    }

    [Test]
    public async Task ShouldHaveErrorWhenWorkoutTemplateIdIsNegative()
    {
        var command = new UpdateWorkoutTemplateExercisesCommand
        {
            WorkoutTemplateId = -1,
            Exercises = []
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "WorkoutTemplateId");
    }

    [Test]
    public async Task ShouldHaveErrorWhenExerciseTemplateIdIsZero()
    {
        var command = new UpdateWorkoutTemplateExercisesCommand
        {
            WorkoutTemplateId = 1,
            Exercises =
            [
                new UpdateWorkoutTemplateExerciseItem { ExerciseTemplateId = 0 }
            ]
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName.Contains("ExerciseTemplateId"));
    }

    [Test]
    public async Task ShouldHaveErrorWhenExerciseTemplateIdIsNegative()
    {
        var command = new UpdateWorkoutTemplateExercisesCommand
        {
            WorkoutTemplateId = 1,
            Exercises =
            [
                new UpdateWorkoutTemplateExerciseItem { ExerciseTemplateId = -1 }
            ]
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName.Contains("ExerciseTemplateId"));
    }

    [Test]
    public async Task ShouldHaveMultipleErrorsWhenMultipleExerciseTemplateIdsAreInvalid()
    {
        var command = new UpdateWorkoutTemplateExercisesCommand
        {
            WorkoutTemplateId = 1,
            Exercises =
            [
                new UpdateWorkoutTemplateExerciseItem { ExerciseTemplateId = 0 },
                new UpdateWorkoutTemplateExerciseItem { ExerciseTemplateId = -1 },
                new UpdateWorkoutTemplateExerciseItem { ExerciseTemplateId = 1 }
            ]
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        var exerciseErrors = result.Errors.Where(e => e.PropertyName.Contains("ExerciseTemplateId")).ToList();
        exerciseErrors.Count.ShouldBe(2);
    }

    [Test]
    public async Task ShouldPassValidationWhenExercisesListIsEmpty()
    {
        var command = new UpdateWorkoutTemplateExercisesCommand
        {
            WorkoutTemplateId = 1,
            Exercises = []
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldPassValidationWhenAllExerciseTemplateIdsAreValid()
    {
        var command = new UpdateWorkoutTemplateExercisesCommand
        {
            WorkoutTemplateId = 1,
            Exercises =
            [
                new UpdateWorkoutTemplateExerciseItem { ExerciseTemplateId = 1 },
                new UpdateWorkoutTemplateExerciseItem { ExerciseTemplateId = 2 },
                new UpdateWorkoutTemplateExerciseItem { ExerciseTemplateId = 3 }
            ]
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldPassValidationWhenCommandIsCompletelyValid()
    {
        var command = new UpdateWorkoutTemplateExercisesCommand
        {
            WorkoutTemplateId = 5,
            Exercises =
            [
                new UpdateWorkoutTemplateExerciseItem { ExerciseTemplateId = 10 },
                new UpdateWorkoutTemplateExerciseItem { ExerciseTemplateId = 20 }
            ]
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }
}
