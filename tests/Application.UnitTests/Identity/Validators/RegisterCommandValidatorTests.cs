using Hoist.Application.Identity.Commands.Register;

namespace Hoist.Application.UnitTests.Identity.Validators;

public class RegisterCommandValidatorTests
{
    private RegisterCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new RegisterCommandValidator();
    }

    [Test]
    public async Task ShouldHaveErrorWhenFirstNameIsEmpty()
    {
        var command = new RegisterCommand
        {
            FirstName = "",
            LastName = "Doe",
            Email = "test@example.com",
            Password = "ValidPass123!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "FirstName");
    }

    [Test]
    public async Task ShouldHaveErrorWhenFirstNameIsWhitespace()
    {
        var command = new RegisterCommand
        {
            FirstName = "   ",
            LastName = "Doe",
            Email = "test@example.com",
            Password = "ValidPass123!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "FirstName");
    }

    [Test]
    public async Task ShouldHaveErrorWhenFirstNameExceedsMaxLength()
    {
        var command = new RegisterCommand
        {
            FirstName = new string('A', 101),
            LastName = "Doe",
            Email = "test@example.com",
            Password = "ValidPass123!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "FirstName");
    }

    [Test]
    public async Task ShouldHaveErrorWhenLastNameIsEmpty()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "",
            Email = "test@example.com",
            Password = "ValidPass123!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "LastName");
    }

    [Test]
    public async Task ShouldHaveErrorWhenLastNameIsWhitespace()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "   ",
            Email = "test@example.com",
            Password = "ValidPass123!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "LastName");
    }

    [Test]
    public async Task ShouldHaveErrorWhenLastNameExceedsMaxLength()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = new string('B', 101),
            Email = "test@example.com",
            Password = "ValidPass123!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "LastName");
    }

    [Test]
    public async Task ShouldHaveErrorWhenEmailIsEmpty()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "",
            Password = "ValidPass123!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Test]
    public async Task ShouldHaveErrorWhenEmailIsWhitespace()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "   ",
            Password = "ValidPass123!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Test]
    public async Task ShouldHaveErrorWhenEmailIsInvalidFormat()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "notanemail",
            Password = "ValidPass123!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Test]
    public async Task ShouldHaveErrorWhenPasswordIsEmpty()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "test@example.com",
            Password = ""
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Password");
    }

    [Test]
    public async Task ShouldHaveErrorWhenPasswordIsWhitespace()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "test@example.com",
            Password = "   "
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Password");
    }

    [Test]
    public async Task ShouldHaveErrorWhenPasswordIsTooShort()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "test@example.com",
            Password = "Short1!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Password");
    }

    [Test]
    public async Task ShouldHaveErrorWhenPasswordMissingUppercase()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "test@example.com",
            Password = "validpass123!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("uppercase"));
    }

    [Test]
    public async Task ShouldHaveErrorWhenPasswordMissingLowercase()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "test@example.com",
            Password = "VALIDPASS123!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("lowercase"));
    }

    [Test]
    public async Task ShouldHaveErrorWhenPasswordMissingDigit()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "test@example.com",
            Password = "ValidPass!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("digit"));
    }

    [Test]
    public async Task ShouldHaveErrorWhenPasswordMissingSpecialCharacter()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "test@example.com",
            Password = "ValidPass123"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("special character"));
    }

    [Test]
    public async Task ShouldHaveErrorWhenAgeIsLessThan13()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "test@example.com",
            Password = "ValidPass123!",
            Age = 12
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Age" && e.ErrorMessage.Contains("13"));
    }

    [Test]
    public async Task ShouldPassValidationWhenAgeIsNull()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "test@example.com",
            Password = "ValidPass123!",
            Age = null
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldPassValidationWhenAgeIs13()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "test@example.com",
            Password = "ValidPass123!",
            Age = 13
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldPassValidationWhenAgeIsGreaterThan13()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "test@example.com",
            Password = "ValidPass123!",
            Age = 25
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldPassValidationWhenAllFieldsAreValid()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "ValidPass123!",
            Age = 30
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldPassValidationWhenFirstNameIsAtMaxLength()
    {
        var command = new RegisterCommand
        {
            FirstName = new string('A', 100),
            LastName = "Doe",
            Email = "test@example.com",
            Password = "ValidPass123!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldPassValidationWhenLastNameIsAtMaxLength()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = new string('B', 100),
            Email = "test@example.com",
            Password = "ValidPass123!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldPassValidationWhenPasswordIsExactlyMinLength()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "test@example.com",
            Password = "Valid12!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }
}
