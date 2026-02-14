using Hoist.Application.Common.Interfaces;
using Hoist.Application.Identity.Commands.ForgotPassword;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Hoist.Application.UnitTests.Identity.Commands;

public class ForgotPasswordCommandHandlerTests
{
    private Mock<IIdentityService> _identityService;
    private Mock<IEmailSender> _emailSender;
    private ForgotPasswordCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _identityService = new Mock<IIdentityService>();
        _emailSender = new Mock<IEmailSender>();
        _handler = new ForgotPasswordCommandHandler(_identityService.Object, _emailSender.Object);
    }

    [Test]
    public async Task Handle_ShouldSendResetEmail_WhenUserExistsAndHasPassword()
    {
        // Arrange
        var command = new ForgotPasswordCommand
        {
            Email = "test@example.com"
        };

        var userId = "user-123";
        var token = "reset-token-123";

        _identityService
            .Setup(x => x.FindUserByEmailAsync(command.Email))
            .ReturnsAsync(userId);

        _identityService
            .Setup(x => x.HasPasswordAsync(userId))
            .ReturnsAsync(true);

        _identityService
            .Setup(x => x.GeneratePasswordResetTokenAsync(userId))
            .ReturnsAsync(token);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _emailSender.Verify(
            x => x.SendPasswordResetEmailAsync(
                command.Email,
                token,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_ShouldNotSendEmail_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new ForgotPasswordCommand
        {
            Email = "nonexistent@example.com"
        };

        _identityService
            .Setup(x => x.FindUserByEmailAsync(command.Email))
            .ReturnsAsync((string?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _identityService.Verify(
            x => x.HasPasswordAsync(It.IsAny<string>()),
            Times.Never);

        _identityService.Verify(
            x => x.GeneratePasswordResetTokenAsync(It.IsAny<string>()),
            Times.Never);

        _emailSender.Verify(
            x => x.SendPasswordResetEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Handle_ShouldNotSendEmail_WhenUserIsOAuthOnly()
    {
        // Arrange
        var command = new ForgotPasswordCommand
        {
            Email = "oauth@example.com"
        };

        var userId = "user-456";

        _identityService
            .Setup(x => x.FindUserByEmailAsync(command.Email))
            .ReturnsAsync(userId);

        _identityService
            .Setup(x => x.HasPasswordAsync(userId))
            .ReturnsAsync(false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _identityService.Verify(
            x => x.GeneratePasswordResetTokenAsync(It.IsAny<string>()),
            Times.Never);

        _emailSender.Verify(
            x => x.SendPasswordResetEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Handle_ShouldCompleteWithoutError_WhenUserNotFound()
    {
        // Arrange
        var command = new ForgotPasswordCommand
        {
            Email = "unknown@example.com"
        };

        _identityService
            .Setup(x => x.FindUserByEmailAsync(command.Email))
            .ReturnsAsync((string?)null);

        // Act & Assert - Should not throw (prevents user enumeration)
        await _handler.Handle(command, CancellationToken.None);
    }

    [Test]
    public async Task Handle_ShouldCompleteWithoutError_WhenUserHasNoPassword()
    {
        // Arrange
        var command = new ForgotPasswordCommand
        {
            Email = "googleuser@example.com"
        };

        var userId = "user-789";

        _identityService
            .Setup(x => x.FindUserByEmailAsync(command.Email))
            .ReturnsAsync(userId);

        _identityService
            .Setup(x => x.HasPasswordAsync(userId))
            .ReturnsAsync(false);

        // Act & Assert - Should not throw (prevents user enumeration)
        await _handler.Handle(command, CancellationToken.None);
    }
}
