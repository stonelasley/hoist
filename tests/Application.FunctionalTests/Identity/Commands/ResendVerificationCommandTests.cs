using Hoist.Application.Common.Exceptions;
using Hoist.Application.Common.Interfaces;
using Hoist.Application.Identity.Commands.Register;
using Hoist.Application.Identity.Commands.ResendVerification;
using Microsoft.Extensions.DependencyInjection;

namespace Hoist.Application.FunctionalTests.Identity.Commands;

using static Testing;

public class ResendVerificationCommandTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireEmail()
    {
        var command = new ResendVerificationCommand
        {
            Email = ""
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequireValidEmail()
    {
        var command = new ResendVerificationCommand
        {
            Email = "invalid-email"
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldNotThrowForNonExistentEmail()
    {
        // This should not throw to prevent user enumeration
        var command = new ResendVerificationCommand
        {
            Email = "nonexistent@example.com"
        };

        await Should.NotThrowAsync(async () => await SendAsync(command));
    }

    [Test]
    public async Task ShouldSucceedForUnverifiedUser()
    {
        // Register a user (email will not be verified)
        await RegisterUser("unverified@example.com", "Password123!");

        // Resend verification
        var command = new ResendVerificationCommand
        {
            Email = "unverified@example.com"
        };

        await Should.NotThrowAsync(async () => await SendAsync(command));
    }

    [Test]
    public async Task ShouldNotThrowForAlreadyVerifiedUser()
    {
        // Register and verify a user
        var userId = await RegisterUser("verified@example.com", "Password123!");
        await VerifyUser(userId);

        // Try to resend verification for already verified user
        var command = new ResendVerificationCommand
        {
            Email = "verified@example.com"
        };

        // Should not throw - command handles this gracefully
        await Should.NotThrowAsync(async () => await SendAsync(command));
    }

    [Test]
    public async Task ShouldNotThrowForEmailWithVariousFormats()
    {
        // Test with uppercase
        var command1 = new ResendVerificationCommand
        {
            Email = "TEST@EXAMPLE.COM"
        };
        await Should.NotThrowAsync(async () => await SendAsync(command1));

        // Test with mixed case
        var command2 = new ResendVerificationCommand
        {
            Email = "Test.User@Example.Com"
        };
        await Should.NotThrowAsync(async () => await SendAsync(command2));

        // Test with subdomain
        var command3 = new ResendVerificationCommand
        {
            Email = "user@subdomain.example.com"
        };
        await Should.NotThrowAsync(async () => await SendAsync(command3));
    }

    [Test]
    public async Task ShouldCompleteQuicklyForNonExistentEmail()
    {
        // Ensure the command doesn't take too long (no heavy processing for non-existent users)
        var command = new ResendVerificationCommand
        {
            Email = "doesnotexist@example.com"
        };

        var startTime = DateTime.UtcNow;
        await SendAsync(command);
        var duration = DateTime.UtcNow - startTime;

        // Should complete within a reasonable time (5 seconds)
        duration.ShouldBeLessThan(TimeSpan.FromSeconds(5));
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
