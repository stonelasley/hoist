using Hoist.Application.Common.Interfaces;
using Hoist.Application.Common.Models;
using Hoist.Application.Identity.Commands.VerifyEmail;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Hoist.Application.UnitTests.Identity.Commands;

public class VerifyEmailCommandHandlerTests
{
    private Mock<IIdentityService> _identityService;
    private VerifyEmailCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _identityService = new Mock<IIdentityService>();
        _handler = new VerifyEmailCommandHandler(_identityService.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnSucceeded_WhenTokenIsValid()
    {
        // Arrange
        var command = new VerifyEmailCommand
        {
            UserId = "user-123",
            Token = "valid-token"
        };

        _identityService
            .Setup(x => x.GetUserNameAsync(command.UserId))
            .ReturnsAsync("test@example.com");

        _identityService
            .Setup(x => x.IsEmailConfirmedAsync(command.UserId))
            .ReturnsAsync(false);

        _identityService
            .Setup(x => x.ConfirmEmailAsync(command.UserId, command.Token))
            .ReturnsAsync(Result.Success());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeTrue();
        result.AlreadyVerified.ShouldBeFalse();
    }

    [Test]
    public async Task Handle_ShouldReturnAlreadyVerified_WhenEmailAlreadyConfirmed()
    {
        // Arrange
        var command = new VerifyEmailCommand
        {
            UserId = "user-123",
            Token = "any-token"
        };

        _identityService
            .Setup(x => x.GetUserNameAsync(command.UserId))
            .ReturnsAsync("test@example.com");

        _identityService
            .Setup(x => x.IsEmailConfirmedAsync(command.UserId))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeTrue();
        result.AlreadyVerified.ShouldBeTrue();

        _identityService.Verify(
            x => x.ConfirmEmailAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task Handle_ShouldReturnFailed_WhenUserNotFound()
    {
        // Arrange
        var command = new VerifyEmailCommand
        {
            UserId = "non-existent-user",
            Token = "token"
        };

        _identityService
            .Setup(x => x.GetUserNameAsync(command.UserId))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.AlreadyVerified.ShouldBeFalse();

        _identityService.Verify(
            x => x.IsEmailConfirmedAsync(It.IsAny<string>()),
            Times.Never);

        _identityService.Verify(
            x => x.ConfirmEmailAsync(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public async Task Handle_ShouldReturnFailed_WhenTokenIsInvalid()
    {
        // Arrange
        var command = new VerifyEmailCommand
        {
            UserId = "user-123",
            Token = "invalid-token"
        };

        _identityService
            .Setup(x => x.GetUserNameAsync(command.UserId))
            .ReturnsAsync("test@example.com");

        _identityService
            .Setup(x => x.IsEmailConfirmedAsync(command.UserId))
            .ReturnsAsync(false);

        _identityService
            .Setup(x => x.ConfirmEmailAsync(command.UserId, command.Token))
            .ReturnsAsync(Result.Failure(new[] { "Invalid token" }));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.AlreadyVerified.ShouldBeFalse();
    }
}
