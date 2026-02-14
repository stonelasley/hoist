using Hoist.Application.Identity.Commands.ResetPassword;

namespace Hoist.Application.UnitTests.Identity.Validators;

public class ResetPasswordCommandValidatorTests
{
    private ResetPasswordCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new ResetPasswordCommandValidator();
    }

    [Test]
    public async Task ShouldHaveErrorWhenEmailIsEmpty()
    {
        var command = new ResetPasswordCommand
        {
            Email = "",
            Token = "valid-token",
            NewPassword = "ValidPass123!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Test]
    public async Task ShouldHaveErrorWhenEmailIsWhitespace()
    {
        var command = new ResetPasswordCommand
        {
            Email = "   ",
            Token = "valid-token",
            NewPassword = "ValidPass123!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Test]
    public async Task ShouldHaveErrorWhenEmailIsInvalidFormat()
    {
        var command = new ResetPasswordCommand
        {
            Email = "notanemail",
            Token = "valid-token",
            NewPassword = "ValidPass123!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
    }

    [Test]
    public async Task ShouldHaveErrorWhenTokenIsEmpty()
    {
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com",
            Token = "",
            NewPassword = "ValidPass123!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Token");
    }

    [Test]
    public async Task ShouldHaveErrorWhenTokenIsWhitespace()
    {
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com",
            Token = "   ",
            NewPassword = "ValidPass123!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Token");
    }

    [Test]
    public async Task ShouldHaveErrorWhenNewPasswordIsEmpty()
    {
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com",
            Token = "valid-token",
            NewPassword = ""
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "NewPassword");
    }

    [Test]
    public async Task ShouldHaveErrorWhenNewPasswordIsWhitespace()
    {
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com",
            Token = "valid-token",
            NewPassword = "   "
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "NewPassword");
    }

    [Test]
    public async Task ShouldHaveErrorWhenNewPasswordIsTooShort()
    {
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com",
            Token = "valid-token",
            NewPassword = "Short1!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "NewPassword");
    }

    [Test]
    public async Task ShouldHaveErrorWhenNewPasswordMissingUppercase()
    {
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com",
            Token = "valid-token",
            NewPassword = "validpass123!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "NewPassword" && e.ErrorMessage.Contains("uppercase"));
    }

    [Test]
    public async Task ShouldHaveErrorWhenNewPasswordMissingLowercase()
    {
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com",
            Token = "valid-token",
            NewPassword = "VALIDPASS123!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "NewPassword" && e.ErrorMessage.Contains("lowercase"));
    }

    [Test]
    public async Task ShouldHaveErrorWhenNewPasswordMissingDigit()
    {
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com",
            Token = "valid-token",
            NewPassword = "ValidPass!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "NewPassword" && e.ErrorMessage.Contains("digit"));
    }

    [Test]
    public async Task ShouldHaveErrorWhenNewPasswordMissingSpecialCharacter()
    {
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com",
            Token = "valid-token",
            NewPassword = "ValidPass123"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "NewPassword" && e.ErrorMessage.Contains("special character"));
    }

    [Test]
    public async Task ShouldHaveMultipleErrorsWhenMultipleFieldsAreInvalid()
    {
        var command = new ResetPasswordCommand
        {
            Email = "",
            Token = "",
            NewPassword = ""
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Email");
        result.Errors.ShouldContain(e => e.PropertyName == "Token");
        result.Errors.ShouldContain(e => e.PropertyName == "NewPassword");
    }

    [Test]
    public async Task ShouldPassValidationWhenAllFieldsAreValid()
    {
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com",
            Token = "reset-token-xyz",
            NewPassword = "ValidPass123!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldPassValidationWhenNewPasswordIsExactlyMinLength()
    {
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com",
            Token = "valid-token",
            NewPassword = "Valid12!"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldPassValidationWithComplexPassword()
    {
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com",
            Token = "valid-token",
            NewPassword = "C0mpl3x!P@ssw0rd#2024"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldPassValidationWithUnderscoreAsSpecialCharacter()
    {
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com",
            Token = "valid-token",
            NewPassword = "Valid_Pass123"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }
}
