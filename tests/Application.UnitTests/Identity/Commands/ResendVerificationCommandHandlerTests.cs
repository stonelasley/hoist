using Hoist.Application.Common.Interfaces;
using Hoist.Application.Identity.Commands.ResendVerification;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Hoist.Application.UnitTests.Identity.Commands;

public class ResendVerificationCommandHandlerTests
{
    private Mock<IIdentityService> _identityService;
    private Mock<IEmailSender> _emailSender;
    private ResendVerificationCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _identityService = new Mock<IIdentityService>();
        _emailSender = new Mock<IEmailSender>();
        _handler = new ResendVerificationCommandHandler(_identityService.Object, _emailSender.Object);
    }

    [Test]
    public async Task Handle_ShouldSendVerificationEmail_WhenUserExistsAndUnverified()
    {
        // Arrange
        var command = new ResendVerificationCommand
        {
            Email = "test@example.com"
        };

        var userId = "user-123";
        var token = "verification-token";

        _identityService
            .Setup(x => x.FindUserByEmailAsync(command.Email))
            .ReturnsAsync(userId);

        _identityService
            .Setup(x => x.IsEmailConfirmedAsync(userId))
            .ReturnsAsync(false);

        _identityService
            .Setup(x => x.GenerateEmailConfirmationTokenAsync(userId))
            .ReturnsAsync(token);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _emailSender.Verify(
            x => x.SendVerificationEmailAsync(
                command.Email,
                userId,
                token,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task Handle_ShouldNotSendEmail_WhenUserNotFound()
    {
        // Arrange
        var command = new ResendVerificationCommand
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
            x => x.IsEmailConfirmedAsync(It.IsAny<string>()),
            Times.Never);

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

    [Test]
    public async Task Handle_ShouldNotSendEmail_WhenAlreadyVerified()
    {
        // Arrange
        var command = new ResendVerificationCommand
        {
            Email = "verified@example.com"
        };

        var userId = "user-456";

        _identityService
            .Setup(x => x.FindUserByEmailAsync(command.Email))
            .ReturnsAsync(userId);

        _identityService
            .Setup(x => x.IsEmailConfirmedAsync(userId))
            .ReturnsAsync(true);

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

    [Test]
    public async Task Handle_ShouldCompleteWithoutError_WhenUserNotFound()
    {
        // Arrange
        var command = new ResendVerificationCommand
        {
            Email = "unknown@example.com"
        };

        _identityService
            .Setup(x => x.FindUserByEmailAsync(command.Email))
            .ReturnsAsync((string?)null);

        // Act & Assert - Should not throw
        await _handler.Handle(command, CancellationToken.None);
    }

    [Test]
    public async Task Handle_ShouldCompleteWithoutError_WhenEmailAlreadyVerified()
    {
        // Arrange
        var command = new ResendVerificationCommand
        {
            Email = "verified@example.com"
        };

        var userId = "user-789";

        _identityService
            .Setup(x => x.FindUserByEmailAsync(command.Email))
            .ReturnsAsync(userId);

        _identityService
            .Setup(x => x.IsEmailConfirmedAsync(userId))
            .ReturnsAsync(true);

        // Act & Assert - Should not throw
        await _handler.Handle(command, CancellationToken.None);
    }
}
