using Hoist.Application.Identity.Commands.ForgotPassword;

namespace Hoist.Application.UnitTests.Identity.Validators;

public class ForgotPasswordCommandValidatorTests
{
    private ForgotPasswordCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new ForgotPasswordCommandValidator();
    }

    [Test]
    public async Task ShouldHaveErrorWhenEmailIsEmpty()
    {
        var command = new ForgotPasswordCommand { Email = "" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Test]
    public async Task ShouldHaveErrorWhenEmailIsWhitespace()
    {
        var command = new ForgotPasswordCommand { Email = "   " };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Test]
    public async Task ShouldHaveErrorWhenEmailIsInvalidFormat()
    {
        var command = new ForgotPasswordCommand { Email = "notanemail" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Test]
    public async Task ShouldHaveErrorWhenEmailIsMissingAtSymbol()
    {
        var command = new ForgotPasswordCommand { Email = "test.example.com" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Test]
    public async Task ShouldHaveErrorWhenEmailIsMissingDomain()
    {
        var command = new ForgotPasswordCommand { Email = "test@" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Test]
    public async Task ShouldPassValidationWhenEmailIsValid()
    {
        var command = new ForgotPasswordCommand { Email = "test@example.com" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldPassValidationWithComplexEmail()
    {
        var command = new ForgotPasswordCommand { Email = "user.name+tag@sub.domain.com" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldPassValidationWithSimpleEmail()
    {
        var command = new ForgotPasswordCommand { Email = "a@b.c" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }
}
