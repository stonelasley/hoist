using Hoist.Application.Common.Interfaces;
using Hoist.Application.ExerciseTemplates.Commands.UpdateExerciseTemplate;
using Hoist.Domain.Entities;
using Hoist.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Hoist.Application.UnitTests.ExerciseTemplates.Validators;

public class UpdateExerciseTemplateCommandValidatorTests
{
    private UpdateExerciseTemplateCommandValidator _validator;
    private Mock<IApplicationDbContext> _mockContext;
    private Mock<IUser> _mockUser;
    private Mock<DbSet<ExerciseTemplate>> _mockExerciseTemplatesDbSet;

    [SetUp]
    public void Setup()
    {
        _mockContext = new Mock<IApplicationDbContext>();
        _mockUser = new Mock<IUser>();
        _mockUser.Setup(u => u.Id).Returns("test-user-id");

        // Set up an empty list of exercise templates (no duplicates)
        var exerciseTemplates = new List<ExerciseTemplate>().AsQueryable();

        _mockExerciseTemplatesDbSet = new Mock<DbSet<ExerciseTemplate>>();
        _mockExerciseTemplatesDbSet.As<IQueryable<ExerciseTemplate>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<ExerciseTemplate>(exerciseTemplates.Provider));
        _mockExerciseTemplatesDbSet.As<IQueryable<ExerciseTemplate>>()
            .Setup(m => m.Expression)
            .Returns(exerciseTemplates.Expression);
        _mockExerciseTemplatesDbSet.As<IQueryable<ExerciseTemplate>>()
            .Setup(m => m.ElementType)
            .Returns(exerciseTemplates.ElementType);
        _mockExerciseTemplatesDbSet.As<IQueryable<ExerciseTemplate>>()
            .Setup(m => m.GetEnumerator())
            .Returns(exerciseTemplates.GetEnumerator());
        _mockExerciseTemplatesDbSet.As<IAsyncEnumerable<ExerciseTemplate>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<ExerciseTemplate>(exerciseTemplates.GetEnumerator()));

        _mockContext.Setup(c => c.ExerciseTemplates).Returns(_mockExerciseTemplatesDbSet.Object);

        _validator = new UpdateExerciseTemplateCommandValidator(_mockContext.Object, _mockUser.Object);
    }

    [Test]
    public async Task ShouldHaveErrorWhenNameIsEmpty()
    {
        var command = new UpdateExerciseTemplateCommand
        {
            Id = 1,
            Name = "",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            Model = "Test model"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }

    [Test]
    public async Task ShouldHaveErrorWhenNameIsWhitespace()
    {
        var command = new UpdateExerciseTemplateCommand
        {
            Id = 1,
            Name = "   ",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            Model = "Test model"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }

    [Test]
    public async Task ShouldHaveErrorWhenNameExceedsMaxLength()
    {
        var command = new UpdateExerciseTemplateCommand
        {
            Id = 1,
            Name = new string('A', 201),
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            Model = "Test model"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Name");
    }

    [Test]
    public async Task ShouldPassValidationWhenNameIsAtMaxLength()
    {
        var command = new UpdateExerciseTemplateCommand
        {
            Id = 1,
            Name = new string('A', 200),
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            Model = "Test model"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldHaveErrorWhenModelExceedsMaxLength()
    {
        var command = new UpdateExerciseTemplateCommand
        {
            Id = 1,
            Name = "Test exercise",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            Model = new string('A', 501)
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Model");
    }

    [Test]
    public async Task ShouldPassValidationWhenModelIsAtMaxLength()
    {
        var command = new UpdateExerciseTemplateCommand
        {
            Id = 1,
            Name = "Test exercise",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            Model = new string('A', 500)
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldPassValidationWhenModelIsNull()
    {
        var command = new UpdateExerciseTemplateCommand
        {
            Id = 1,
            Name = "Test exercise",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            Model = null
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldHaveErrorWhenNameIsDuplicateForDifferentExercise()
    {
        // Set up a scenario where another exercise has the same name
        var existingExercise = new ExerciseTemplate
        {
            Id = 2,
            Name = "Bench Press",
            UserId = "test-user-id",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        };

        var exerciseTemplates = new List<ExerciseTemplate> { existingExercise }.AsQueryable();

        _mockExerciseTemplatesDbSet.As<IQueryable<ExerciseTemplate>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<ExerciseTemplate>(exerciseTemplates.Provider));
        _mockExerciseTemplatesDbSet.As<IQueryable<ExerciseTemplate>>()
            .Setup(m => m.Expression)
            .Returns(exerciseTemplates.Expression);
        _mockExerciseTemplatesDbSet.As<IQueryable<ExerciseTemplate>>()
            .Setup(m => m.ElementType)
            .Returns(exerciseTemplates.ElementType);
        _mockExerciseTemplatesDbSet.As<IQueryable<ExerciseTemplate>>()
            .Setup(m => m.GetEnumerator())
            .Returns(exerciseTemplates.GetEnumerator());
        _mockExerciseTemplatesDbSet.As<IAsyncEnumerable<ExerciseTemplate>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<ExerciseTemplate>(exerciseTemplates.GetEnumerator()));

        var command = new UpdateExerciseTemplateCommand
        {
            Id = 1,
            Name = "Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            Model = "Test model"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Name" && e.ErrorCode == "Unique");
    }

    [Test]
    public async Task ShouldPassValidationWhenNameIsSameAsCurrentExercise()
    {
        // Set up a scenario where the exercise being updated has the same name (no change)
        var existingExercise = new ExerciseTemplate
        {
            Id = 1,
            Name = "Bench Press",
            UserId = "test-user-id",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps
        };

        var exerciseTemplates = new List<ExerciseTemplate> { existingExercise }.AsQueryable();

        _mockExerciseTemplatesDbSet.As<IQueryable<ExerciseTemplate>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<ExerciseTemplate>(exerciseTemplates.Provider));
        _mockExerciseTemplatesDbSet.As<IQueryable<ExerciseTemplate>>()
            .Setup(m => m.Expression)
            .Returns(exerciseTemplates.Expression);
        _mockExerciseTemplatesDbSet.As<IQueryable<ExerciseTemplate>>()
            .Setup(m => m.ElementType)
            .Returns(exerciseTemplates.ElementType);
        _mockExerciseTemplatesDbSet.As<IQueryable<ExerciseTemplate>>()
            .Setup(m => m.GetEnumerator())
            .Returns(exerciseTemplates.GetEnumerator());
        _mockExerciseTemplatesDbSet.As<IAsyncEnumerable<ExerciseTemplate>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<ExerciseTemplate>(exerciseTemplates.GetEnumerator()));

        var command = new UpdateExerciseTemplateCommand
        {
            Id = 1,
            Name = "Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            Model = "Test model"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldPassValidationWhenNameIsUniqueForUser()
    {
        var command = new UpdateExerciseTemplateCommand
        {
            Id = 1,
            Name = "Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            Model = "Test model"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldPassValidationWhenAllFieldsAreValid()
    {
        var command = new UpdateExerciseTemplateCommand
        {
            Id = 1,
            Name = "Bench Press",
            ImplementType = ImplementType.Barbell,
            ExerciseType = ExerciseType.Reps,
            Model = "Standard barbell bench press"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }
}
