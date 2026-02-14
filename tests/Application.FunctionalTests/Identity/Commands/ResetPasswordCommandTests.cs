using Hoist.Application.Common.Exceptions;
using Hoist.Application.Common.Interfaces;
using Hoist.Application.Identity.Commands.Register;
using Hoist.Application.Identity.Commands.ResetPassword;
using Microsoft.Extensions.DependencyInjection;

namespace Hoist.Application.FunctionalTests.Identity.Commands;

using static Testing;

public class ResetPasswordCommandTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireEmail()
    {
        var command = new ResetPasswordCommand
        {
            Email = "",
            Token = "some-token",
            NewPassword = "NewPassword123!"
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequireValidEmail()
    {
        var command = new ResetPasswordCommand
        {
            Email = "invalid-email",
            Token = "some-token",
            NewPassword = "NewPassword123!"
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequireToken()
    {
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com",
            Token = "",
            NewPassword = "NewPassword123!"
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequireNewPassword()
    {
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com",
            Token = "some-token",
            NewPassword = ""
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequirePasswordMinimumLength()
    {
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com",
            Token = "some-token",
            NewPassword = "Pass1!"
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequirePasswordWithUppercase()
    {
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com",
            Token = "some-token",
            NewPassword = "password123!"
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequirePasswordWithLowercase()
    {
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com",
            Token = "some-token",
            NewPassword = "PASSWORD123!"
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequirePasswordWithDigit()
    {
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com",
            Token = "some-token",
            NewPassword = "Password!"
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequirePasswordWithSpecialCharacter()
    {
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com",
            Token = "some-token",
            NewPassword = "Password123"
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldFailWithInvalidToken()
    {
        // Register a user
        var userId = await RegisterUser("resetuser@example.com", "OldPassword123!");

        // Try to reset password with invalid token
        var command = new ResetPasswordCommand
        {
            Email = "resetuser@example.com",
            Token = "invalid-token",
            NewPassword = "NewPassword123!"
        };

        var result = await SendAsync(command);

        result.ShouldNotBeNull();
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldFailWithNonExistentEmail()
    {
        var command = new ResetPasswordCommand
        {
            Email = "nonexistent@example.com",
            Token = "some-token",
            NewPassword = "NewPassword123!"
        };

        var result = await SendAsync(command);

        result.ShouldNotBeNull();
        result.Succeeded.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldSucceedWithValidToken()
    {
        // Register a user
        var userId = await RegisterUser("validreset@example.com", "OldPassword123!");

        // Generate password reset token
        using var scope = GetScopeFactory().CreateScope();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
        var token = await identityService.GeneratePasswordResetTokenAsync(userId);

        // Reset password with valid token
        var command = new ResetPasswordCommand
        {
            Email = "validreset@example.com",
            Token = token,
            NewPassword = "NewSecurePassword123!"
        };

        var result = await SendAsync(command);

        result.ShouldNotBeNull();
        result.Succeeded.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();

        // Verify the password was actually changed
        var passwordValid = await identityService.CheckPasswordAsync(userId, "NewSecurePassword123!");
        passwordValid.ShouldBeTrue();

        var oldPasswordValid = await identityService.CheckPasswordAsync(userId, "OldPassword123!");
        oldPasswordValid.ShouldBeFalse();
    }

    [Test]
    public async Task ShouldFailWhenTokenIsForDifferentUser()
    {
        // Register two users
        var userId1 = await RegisterUser("user1@example.com", "Password123!");
        var userId2 = await RegisterUser("user2@example.com", "Password123!");

        // Generate token for user1
        using var scope = GetScopeFactory().CreateScope();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
        var token1 = await identityService.GeneratePasswordResetTokenAsync(userId1);

        // Try to use user1's token for user2
        var command = new ResetPasswordCommand
        {
            Email = "user2@example.com",
            Token = token1,
            NewPassword = "NewPassword123!"
        };

        var result = await SendAsync(command);

        result.ShouldNotBeNull();
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldFailWhenTokenIsUsedTwice()
    {
        // Register a user
        var userId = await RegisterUser("tokenreuse@example.com", "OldPassword123!");

        // Generate password reset token
        using var scope = GetScopeFactory().CreateScope();
        var identityService = scope.ServiceProvider.GetRequiredService<IIdentityService>();
        var token = await identityService.GeneratePasswordResetTokenAsync(userId);

        // Use token first time
        var firstCommand = new ResetPasswordCommand
        {
            Email = "tokenreuse@example.com",
            Token = token,
            NewPassword = "NewPassword123!"
        };

        var firstResult = await SendAsync(firstCommand);
        firstResult.Succeeded.ShouldBeTrue();

        // Try to use same token again
        var secondCommand = new ResetPasswordCommand
        {
            Email = "tokenreuse@example.com",
            Token = token,
            NewPassword = "AnotherPassword123!"
        };

        var secondResult = await SendAsync(secondCommand);
        secondResult.Succeeded.ShouldBeFalse();
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

    private static IServiceScopeFactory GetScopeFactory()
    {
        return (IServiceScopeFactory)typeof(Testing)
            .GetField("_scopeFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .GetValue(null)!;
    }
}
