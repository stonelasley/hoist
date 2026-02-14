using Hoist.Application.Identity.Commands.ResendVerification;

namespace Hoist.Application.UnitTests.Identity.Validators;

public class ResendVerificationCommandValidatorTests
{
    private ResendVerificationCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new ResendVerificationCommandValidator();
    }

    [Test]
    public async Task ShouldHaveErrorWhenEmailIsEmpty()
    {
        var command = new ResendVerificationCommand { Email = "" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Test]
    public async Task ShouldHaveErrorWhenEmailIsWhitespace()
    {
        var command = new ResendVerificationCommand { Email = "   " };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Test]
    public async Task ShouldHaveErrorWhenEmailIsInvalidFormat()
    {
        var command = new ResendVerificationCommand { Email = "notanemail" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Test]
    public async Task ShouldHaveErrorWhenEmailIsMissingAtSymbol()
    {
        var command = new ResendVerificationCommand { Email = "test.example.com" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Test]
    public async Task ShouldHaveErrorWhenEmailIsMissingDomain()
    {
        var command = new ResendVerificationCommand { Email = "test@" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Test]
    public async Task ShouldPassValidationWhenEmailIsValid()
    {
        var command = new ResendVerificationCommand { Email = "test@example.com" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldPassValidationWithComplexEmail()
    {
        var command = new ResendVerificationCommand { Email = "user.name+tag@sub.domain.com" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldPassValidationWithSimpleEmail()
    {
        var command = new ResendVerificationCommand { Email = "a@b.c" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }
}
