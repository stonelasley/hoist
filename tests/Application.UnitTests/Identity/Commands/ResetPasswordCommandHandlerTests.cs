using Hoist.Application.Common.Interfaces;
using Hoist.Application.Common.Models;
using Hoist.Application.Identity.Commands.ResetPassword;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Hoist.Application.UnitTests.Identity.Commands;

public class ResetPasswordCommandHandlerTests
{
    private Mock<IIdentityService> _identityService;
    private ResetPasswordCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _identityService = new Mock<IIdentityService>();
        _handler = new ResetPasswordCommandHandler(_identityService.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnSucceeded_WhenResetIsValid()
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com",
            Token = "valid-reset-token",
            NewPassword = "NewSecurePass123!"
        };

        var userId = "user-123";

        _identityService
            .Setup(x => x.FindUserByEmailAsync(command.Email))
            .ReturnsAsync(userId);

        _identityService
            .Setup(x => x.ResetPasswordAsync(userId, command.Token, command.NewPassword))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public async Task Handle_ShouldReturnFailed_WhenUserNotFound()
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            Email = "nonexistent@example.com",
            Token = "token",
            NewPassword = "NewPassword123!"
        };

        _identityService
            .Setup(x => x.FindUserByEmailAsync(command.Email))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldBeEmpty();

        _identityService.Verify(
            x => x.ResetPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task Handle_ShouldReturnFailed_WhenTokenIsInvalid()
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com",
            Token = "invalid-token",
            NewPassword = "NewPassword123!"
        };

        var userId = "user-123";
        var errors = new[] { "Invalid token" };

        _identityService
            .Setup(x => x.FindUserByEmailAsync(command.Email))
            .ReturnsAsync(userId);

        _identityService
            .Setup(x => x.ResetPasswordAsync(userId, command.Token, command.NewPassword))
            .ReturnsAsync(Result.Failure(errors));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldBe(errors);
    }

    [Test]
    public async Task Handle_ShouldReturnFailed_WhenPasswordIsInvalid()
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com",
            Token = "valid-token",
            NewPassword = "weak"
        };

        var userId = "user-123";
        var errors = new[] { "Password is too weak", "Password must contain uppercase" };

        _identityService
            .Setup(x => x.FindUserByEmailAsync(command.Email))
            .ReturnsAsync(userId);

        _identityService
            .Setup(x => x.ResetPasswordAsync(userId, command.Token, command.NewPassword))
            .ReturnsAsync(Result.Failure(errors));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.Errors.ShouldBe(errors);
    }

    [Test]
    public async Task Handle_ShouldCallResetPasswordAsync_WithCorrectParameters()
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            Email = "test@example.com",
            Token = "reset-token-456",
            NewPassword = "SecureNewPassword123!"
        };

        var userId = "user-456";

        _identityService
            .Setup(x => x.FindUserByEmailAsync(command.Email))
            .ReturnsAsync(userId);

        _identityService
            .Setup(x => x.ResetPasswordAsync(userId, command.Token, command.NewPassword))
            .ReturnsAsync(Result.Success());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _identityService.Verify(
            x => x.ResetPasswordAsync(userId, command.Token, command.NewPassword),
            Times.Once);
    }
}
