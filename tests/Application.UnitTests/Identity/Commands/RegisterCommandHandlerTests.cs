using Hoist.Application.Common.Interfaces;
using Hoist.Application.Common.Models;
using Hoist.Application.Identity.Commands.Register;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Hoist.Application.UnitTests.Identity.Commands;

public class RegisterCommandHandlerTests
{
    private Mock<IIdentityService> _identityService;
    private Mock<IEmailSender> _emailSender;
    private RegisterCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _identityService = new Mock<IIdentityService>();
        _emailSender = new Mock<IEmailSender>();
        _handler = new RegisterCommandHandler(_identityService.Object, _emailSender.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnSucceeded_AndSendVerificationEmail_OnValidRegistration()
    {
        // Arrange
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "Password123!",
            Age = 25
        };

        var userId = "user-123";
        var token = "verification-token-123";
        var successResult = Result.Success();

        _identityService
            .Setup(x => x.CreateUserWithDetailsAsync(
                command.FirstName,
                command.LastName,
                command.Email,
                command.Password,
                command.Age))
            .ReturnsAsync((successResult, userId));

        _identityService
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(userId))
            .ReturnsAsync(token);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();

        _emailSender.Verify(
            x => x.SendVerificationEmailAsync(
                command.Email,
                userId,
                token,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_ShouldReturnFailed_WhenUserCreationFails()
    {
        // Arrange
        var command = new RegisterCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Password = "weak",
            Age = 25
        };

        var errors = new[] { "Password is too weak", "Password must contain uppercase" };
        var failedResult = Result.Failure(errors);

        _identityService
            .Setup(x => x.CreateUserWithDetailsAsync(
                command.FirstName,
                command.LastName,
                command.Email,
                command.Password,
                command.Age))
            .ReturnsAsync((failedResult, string.Empty));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldBe(errors);

        _emailSender.Verify(
            x => x.SendVerificationEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Handle_ShouldCallSendVerificationEmailAsync_OnSuccess()
    {
        // Arrange
        var command = new RegisterCommand
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Password = "SecurePass123!",
            Age = null
        };

        var userId = "user-456";
        var token = "token-456";
        var successResult = Result.Success();

        _identityService
            .Setup(x => x.CreateUserWithDetailsAsync(
                command.FirstName,
                command.LastName,
                command.Email,
                command.Password,
                command.Age))
            .ReturnsAsync((successResult, userId));

        _identityService
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(userId))
            .ReturnsAsync(token);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _identityService.Verify(
            x => x.GenerateEmailConfirmationTokenAsync(userId),
            Times.Once);

        _emailSender.Verify(
            x => x.SendVerificationEmailAsync(
                command.Email,
                userId,
                token,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_ShouldNotSendVerificationEmail_WhenUserCreationFails()
    {
        // Arrange
        var command = new RegisterCommand
        {
            FirstName = "Test",
            LastName = "User",
            Email = "existing@example.com",
            Password = "Password123!",
            Age = 30
        };

        var failedResult = Result.Failure(new[] { "User already exists" });

        _identityService
            .Setup(x => x.CreateUserWithDetailsAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int?>()))
            .ReturnsAsync((failedResult, string.Empty));

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _identityService.Verify(
            x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<string>()),
            Times.Never);

        _emailSender.Verify(
            x => x.SendVerificationEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
