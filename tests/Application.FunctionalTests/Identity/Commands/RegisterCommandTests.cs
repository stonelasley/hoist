using Hoist.Application.Common.Exceptions;
using Hoist.Application.Identity.Commands.Register;
using Hoist.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Hoist.Application.FunctionalTests.Identity.Commands;

using static Testing;

public class RegisterCommandTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireFirstName()
    {
        var command = new RegisterCommand
        {
            FirstName = "",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password123!"
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequireLastName()
    {
        var command = new RegisterCommand
        {
            FirstName = "Test",
            LastName = "",
            Email = "test@example.com",
            Password = "Password123!"
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequireValidEmail()
    {
        var command = new RegisterCommand
        {
            FirstName = "Test",
            LastName = "User",
            Email = "invalid-email",
            Password = "Password123!"
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequirePasswordWithUppercase()
    {
        var command = new RegisterCommand
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "password123!"
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequirePasswordWithLowercase()
    {
        var command = new RegisterCommand
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "PASSWORD123!"
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequirePasswordWithDigit()
    {
        var command = new RegisterCommand
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password!"
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequirePasswordWithSpecialCharacter()
    {
        var command = new RegisterCommand
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password123"
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRequirePasswordMinimumLength()
    {
        var command = new RegisterCommand
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "Pass1!"
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldRejectAgeUnder13()
    {
        var command = new RegisterCommand
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = "Password123!",
            Age = 12
        };

        await Should.ThrowAsync<ValidationException>(() => SendAsync(command));
    }

    [Test]
    public async Task ShouldSuccessfullyRegisterValidUser()
    {
        var command = new RegisterCommand
        {
            FirstName = "Test",
            LastName = "User",
            Email = "newuser@example.com",
            Password = "Password123!",
            Age = 25
        };

        var result = await SendAsync(command);

        result.ShouldNotBeNull();
        result.Succeeded.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldFailWhenEmailAlreadyExists()
    {
        // Create first user
        var firstCommand = new RegisterCommand
        {
            FirstName = "First",
            LastName = "User",
            Email = "duplicate@example.com",
            Password = "Password123!"
        };

        var firstResult = await SendAsync(firstCommand);
        firstResult.Succeeded.ShouldBeTrue();

        // Try to register with same email
        var duplicateCommand = new RegisterCommand
        {
            FirstName = "Second",
            LastName = "User",
            Email = "duplicate@example.com",
            Password = "DifferentPassword123!"
        };

        var duplicateResult = await SendAsync(duplicateCommand);

        duplicateResult.Succeeded.ShouldBeFalse();
        duplicateResult.Errors.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldCreateUserWithCorrectDetails()
    {
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "SecurePassword123!",
            Age = 30
        };

        var result = await SendAsync(command);
        result.Succeeded.ShouldBeTrue();

        // Verify user was created with correct details
        using var scope = GetScopeFactory().CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(command.Email);

        user.ShouldNotBeNull();
        user!.Email.ShouldBe(command.Email);
        user.FirstName.ShouldBe(command.FirstName);
        user.LastName.ShouldBe(command.LastName);
        user.Age.ShouldBe(command.Age);
        user.EmailConfirmed.ShouldBeFalse(); // Should not be confirmed initially
    }

    private static IServiceScopeFactory GetScopeFactory()
    {
        // Access the scope factory from Testing class
        return (IServiceScopeFactory)typeof(Testing)
            .GetField("_scopeFactory", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .GetValue(null)!;
    }
}
