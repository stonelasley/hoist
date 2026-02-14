using Hoist.Application.Identity.Commands.GoogleLogin;

namespace Hoist.Application.UnitTests.Identity.Validators;

public class GoogleLoginCommandValidatorTests
{
    private GoogleLoginCommandValidator _validator;

    [SetUp]
    public void Setup()
    {
        _validator = new GoogleLoginCommandValidator();
    }

    [Test]
    public async Task ShouldHaveErrorWhenIdTokenIsEmpty()
    {
        var command = new GoogleLoginCommand { IdToken = "" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "IdToken");
    }

    [Test]
    public async Task ShouldHaveErrorWhenIdTokenIsWhitespace()
    {
        var command = new GoogleLoginCommand { IdToken = "   " };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == "IdToken");
    }

    [Test]
    public async Task ShouldPassValidationWhenIdTokenIsValid()
    {
        var command = new GoogleLoginCommand { IdToken = "valid-google-id-token" };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldPassValidationWithLongIdToken()
    {
        var command = new GoogleLoginCommand { IdToken = new string('A', 500) };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldPassValidationWithJwtLikeToken()
    {
        var command = new GoogleLoginCommand
        {
            IdToken = "eyJhbGciOiJSUzI1NiIsImtpZCI6IjEifQ.eyJpc3MiOiJodHRwczovL2FjY291bnRzLmdvb2dsZS5jb20ifQ.signature"
        };
        var result = await _validator.ValidateAsync(command);
        result.IsValid.ShouldBeTrue();
    }
}
