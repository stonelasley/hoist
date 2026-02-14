using Hoist.Application.Common.Exceptions;
using Hoist.Application.Common.Interfaces;
using Hoist.Application.Identity.Commands.Login;
using Hoist.Application.Identity.Commands.Register;
using Microsoft.Extensions.DependencyInjection;

namespace Hoist.Application.FunctionalTests.Identity.Commands;

using static Testing;

public class LoginCommandTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireEmail()
    {
        var command = new LoginCommand
        {
            Email = "",
            Password = "Password123!"
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequireValidEmail()
    {
        var command = new LoginCommand
        {
            Email = "invalid-email",
            Password = "Password123!"
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequirePassword()
    {
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = ""
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldFailWithInvalidCredentials()
    {
        // Register a user first
        await RegisterUser("validuser@example.com", "Password123!");

        // Try to login with wrong password
        var command = new LoginCommand
        {
            Email = "validuser@example.com",
            Password = "WrongPassword123!"
        };

        var result = await SendAsync(command);

        result.ShouldNotBeNull();
        result.Succeeded.ShouldBeFalse();
        result.EmailNotVerified.ShouldBeFalse();
        result.UserId.ShouldBeNull();
    }

    [Test]
    public async Task ShouldFailWithNonExistentEmail()
    {
        var command = new LoginCommand
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        var result = await SendAsync(command);

        result.ShouldNotBeNull();
        result.Succeeded.ShouldBeFalse();
        result.EmailNotVerified.ShouldBeFalse();
        result.UserId.ShouldBeNull();
    }

    [Test]
    public async Task ShouldReturnEmailNotVerifiedForUnverifiedUser()
    {
        // Register a user (email will not be verified)
        await RegisterUser("unverified@example.com", "Password123!");

        // Try to login with correct credentials but unverified email
        var command = new LoginCommand
        {
            Email = "unverified@example.com",
            Password = "Password123!"
        };

        var result = await SendAsync(command);

        result.ShouldNotBeNull();
        result.Succeeded.ShouldBeFalse();
        result.EmailNotVerified.ShouldBeTrue();
        result.UserId.ShouldBeNull();
    }

    [Test]
    public async Task ShouldSucceedForVerifiedUserWithCorrectCredentials()
    {
        // Register and verify a user
        var userId = await RegisterAndVerifyUser("verified@example.com", "Password123!");

        // Login with correct credentials
        var command = new LoginCommand
        {
            Email = "verified@example.com",
            Password = "Password123!"
        };

        var result = await SendAsync(command);

        result.ShouldNotBeNull();
        result.Succeeded.ShouldBeTrue();
        result.EmailNotVerified.ShouldBeFalse();
        result.UserId.ShouldBe(userId);
    }

    private async Task<string> RegisterUser(string email, string password)
    {
        var registerCommand = new RegisterCommand
        {
            FirstName = "Test",
            LastName = "User",
            Email = email,
            Password = password
        };

        var result = await SendAsync(registerCommand);
        result.Succeeded.ShouldBeTrue();

        using var scope = GetScopeFactory().CreateScope();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
        var userId = await identityService.FindUserByEmailAsync(email);
        userId.ShouldNotBeNull();

        return userId!;
    }

    private async Task<string> RegisterAndVerifyUser(string email, string password)
    {
        var userId = await RegisterUser(email, password);

        // Manually confirm the email using the identity service
        using var scope = GetScopeFactory().CreateScope();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
        var token = await identityService.GenerateEmailConfirmationTokenAsync(userId);
        var confirmResult = await identityService.ConfirmEmailAsync(userId, token);
        confirmResult.Succeeded.ShouldBeTrue();

        return userId;
    }

    private static IServiceScopeFactory GetScopeFactory()
    {
        return (IServiceScopeFactory)typeof(Testing)
            .GetField("_scopeFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .GetValue(null)!;
    }
}
