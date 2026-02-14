using Hoist.Application.Identity.Commands.VerifyEmail;

namespace Hoist.Application.UnitTests.Identity.Validators;

public class VerifyEmailCommandValidatorTests
{
    private VerifyEmailCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new VerifyEmailCommandValidator();
    }

    [Test]
    public async Task ShouldHaveErrorWhenUserIdIsEmpty()
    {
        var command = new VerifyEmailCommand { UserId = "", Token = "valid-token" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "UserId");
    }

    [Test]
    public async Task ShouldHaveErrorWhenUserIdIsWhitespace()
    {
        var command = new VerifyEmailCommand { UserId = "   ", Token = "valid-token" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "UserId");
    }

    [Test]
    public async Task ShouldHaveErrorWhenTokenIsEmpty()
    {
        var command = new VerifyEmailCommand { UserId = "user-123", Token = "" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Token");
    }

    [Test]
    public async Task ShouldHaveErrorWhenTokenIsWhitespace()
    {
        var command = new VerifyEmailCommand { UserId = "user-123", Token = "   " };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "Token");
    }

    [Test]
    public async Task ShouldHaveErrorWhenBothUserIdAndTokenAreEmpty()
    {
        var command = new VerifyEmailCommand { UserId = "", Token = "" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "UserId");
        result.Errors.ShouldContain(e => e.PropertyName == "Token");
    }

    [Test]
    public async Task ShouldPassValidationWhenAllFieldsAreValid()
    {
        var command = new VerifyEmailCommand { UserId = "user-123", Token = "verification-token-xyz" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldPassValidationWithGuidUserId()
    {
        var command = new VerifyEmailCommand
        {
            UserId = "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
            Token = "token123"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldPassValidationWithLongToken()
    {
        var command = new VerifyEmailCommand
        {
            UserId = "user-123",
            Token = new string('A', 200)
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }
}
