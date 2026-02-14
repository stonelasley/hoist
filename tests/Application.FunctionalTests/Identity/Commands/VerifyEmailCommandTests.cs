using Hoist.Application.Common.Exceptions;
using Hoist.Application.Common.Interfaces;
using Hoist.Application.Identity.Commands.Register;
using Hoist.Application.Identity.Commands.VerifyEmail;
using Microsoft.Extensions.DependencyInjection;

namespace Hoist.Application.FunctionalTests.Identity.Commands;

using static Testing;

public class VerifyEmailCommandTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireUserId()
    {
        var command = new VerifyEmailCommand
        {
            UserId = "",
            Token = "valid-token"
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequireToken()
    {
        var command = new VerifyEmailCommand
        {
            UserId = "some-user-id",
            Token = ""
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldFailWithInvalidToken()
    {
        // Register a user
        var userId = await RegisterUser("testuser@example.com", "Password123!");

        // Try to verify with invalid token
        var command = new VerifyEmailCommand
        {
            UserId = userId,
            Token = "invalid-token"
        };

        var result = await SendAsync(command);

        result.ShouldNotBeNull();
        result.Succeeded.ShouldBeFalse();
        result.AlreadyVerified.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldFailWithNonExistentUserId()
    {
        var command = new VerifyEmailCommand
        {
            UserId = "non-existent-user-id",
            Token = "some-token"
        };

        var result = await SendAsync(command);

        result.ShouldNotBeNull();
        result.Succeeded.ShouldBeFalse();
        result.AlreadyVerified.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldSucceedWithValidToken()
    {
        // Register a user and get the verification token
        var (userId, token) = await RegisterUserAndGetToken("newuser@example.com", "Password123!");

        // Verify email with valid token
        var command = new VerifyEmailCommand
        {
            UserId = userId,
            Token = token
        };

        var result = await SendAsync(command);

        result.ShouldNotBeNull();
        result.Succeeded.ShouldBeTrue();
        result.AlreadyVerified.ShouldBeFalse();

        // Verify the email is confirmed
        using var scope = GetScopeFactory().CreateScope();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
        var isConfirmed = await identityService.IsEmailConfirmedAsync(userId);
        isConfirmed.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturnAlreadyVerifiedForAlreadyVerifiedUser()
    {
        // Register and verify a user
        var (userId, _) = await RegisterUserAndGetToken("verified@example.com", "Password123!");
        await VerifyUser(userId);

        // Try to verify again
        var command = new VerifyEmailCommand
        {
            UserId = userId,
            Token = "any-token"
        };

        var result = await SendAsync(command);

        result.ShouldNotBeNull();
        result.Succeeded.ShouldBeTrue();
        result.AlreadyVerified.ShouldBeTrue();
    }

    [Test]
    public async Task ShouldFailWhenTokenIsForDifferentUser()
    {
        // Register two users
        var (userId1, token1) = await RegisterUserAndGetToken("user1@example.com", "Password123!");
        var (userId2, _) = await RegisterUserAndGetToken("user2@example.com", "Password123!");

        // Try to use user1's token for user2
        var command = new VerifyEmailCommand
        {
            UserId = userId2,
            Token = token1
        };

        var result = await SendAsync(command);

        result.ShouldNotBeNull();
        result.Succeeded.ShouldBeFalse();
        result.AlreadyVerified.ShouldBeFalse();
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

    private async Task<(string userId, string token)> RegisterUserAndGetToken(string email, string password)
    {
        var userId = await RegisterUser(email, password);

        using var scope = GetScopeFactory().CreateScope();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
        var token = await identityService.GenerateEmailConfirmationTokenAsync(userId);

        return (userId, token);
    }

    private async Task VerifyUser(string userId)
    {
        using var scope = GetScopeFactory().CreateScope();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
        var token = await identityService.GenerateEmailConfirmationTokenAsync(userId);
        var result = await identityService.ConfirmEmailAsync(userId, token);
        result.Succeeded.ShouldBeTrue();
    }

    private static IServiceScopeFactory GetScopeFactory()
    {
        return (IServiceScopeFactory)typeof(Testing)
            .GetField("_scopeFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .GetValue(null)!;
    }
}
