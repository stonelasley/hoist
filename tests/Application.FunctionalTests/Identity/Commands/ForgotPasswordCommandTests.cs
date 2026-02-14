using Hoist.Application.Common.Exceptions;
using Hoist.Application.Common.Interfaces;
using Hoist.Application.Identity.Commands.ForgotPassword;
using Hoist.Application.Identity.Commands.Register;
using Microsoft.Extensions.DependencyInjection;

namespace Hoist.Application.FunctionalTests.Identity.Commands;

using static Testing;

public class ForgotPasswordCommandTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireEmail()
    {
        var command = new ForgotPasswordCommand
        {
            Email = ""
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequireValidEmail()
    {
        var command = new ForgotPasswordCommand
        {
            Email = "invalid-email"
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldSucceedEvenForNonExistentEmail()
    {
        // This should not throw to prevent user enumeration
        var command = new ForgotPasswordCommand
        {
            Email = "nonexistent@example.com"
        };

        await Should.NotThrowAsync(async () => await SendAsync(command));
    }

    [Test]
    public async Task ShouldSucceedForExistingUser()
    {
        // Register a user
        await RegisterUser("existinguser@example.com", "Password123!");

        // Request password reset
        var command = new ForgotPasswordCommand
        {
            Email = "existinguser@example.com"
        };

        await Should.NotThrowAsync(async () => await SendAsync(command));
    }

    [Test]
    public async Task ShouldNotThrowForEmailWithVariousFormats()
    {
        // Test with uppercase
        var command1 = new ForgotPasswordCommand
        {
            Email = "TEST@EXAMPLE.COM"
        };
        await Should.NotThrowAsync(async () => await SendAsync(command1));

        // Test with mixed case
        var command2 = new ForgotPasswordCommand
        {
            Email = "Test.User@Example.Com"
        };
        await Should.NotThrowAsync(async () => await SendAsync(command2));

        // Test with subdomain
        var command3 = new ForgotPasswordCommand
        {
            Email = "user@subdomain.example.com"
        };
        await Should.NotThrowAsync(async () => await SendAsync(command3));
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
