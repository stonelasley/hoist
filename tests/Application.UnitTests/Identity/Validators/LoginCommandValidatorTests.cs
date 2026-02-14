using Hoist.Application.Identity.Commands.Login;

namespace Hoist.Application.UnitTests.Identity.Validators;

public class LoginCommandValidatorTests
{
    private LoginCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new LoginCommandValidator();
    }

    [Test]
    public async Task ShouldHaveErrorWhenEmailIsEmpty()
    {
        var command = new LoginCommand { Email = "", Password = "Test123!" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Test]
    public async Task ShouldHaveErrorWhenEmailIsWhitespace()
    {
        var command = new LoginCommand { Email = "   ", Password = "Test123!" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Test]
    public async Task ShouldHaveErrorWhenEmailIsInvalidFormat()
    {
        var command = new LoginCommand { Email = "notanemail", Password = "Test123!" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Test]
    public async Task ShouldHaveErrorWhenPasswordIsEmpty()
    {
        var command = new LoginCommand { Email = "test@example.com", Password = "" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Password");
    }

    [Test]
    public async Task ShouldHaveErrorWhenPasswordIsWhitespace()
    {
        var command = new LoginCommand { Email = "test@example.com", Password = "   " };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Password");
    }

    [Test]
    public async Task ShouldPassValidationWhenAllFieldsAreValid()
    {
        var command = new LoginCommand { Email = "test@example.com", Password = "ValidPassword123!" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldPassValidationWithValidEmailAndSimplePassword()
    {
        var command = new LoginCommand { Email = "user@domain.com", Password = "pass" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }
}
