using Hoist.Application.Common.Interfaces;
using Hoist.Application.Identity.Commands.Login;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace Hoist.Application.UnitTests.Identity.Commands;

public class LoginCommandHandlerTests
{
    private Mock<IIdentityService> _identityService;
    private LoginCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _identityService = new Mock<IIdentityService>();
        _handler = new LoginCommandHandler(_identityService.Object);
    }

    [Test]
    public async Task Handle_ShouldReturnFailed_WhenUserNotFound()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        _identityService
            .Setup(x => x.FindUserByEmailAsync(command.Email))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.EmailNotVerified.ShouldBeFalse();
        result.UserId.ShouldBeNull();
    }

    [Test]
    public async Task Handle_ShouldReturnFailed_WhenPasswordIsWrong()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        var userId = "user-123";

        _identityService
            .Setup(x => x.FindUserByEmailAsync(command.Email))
            .ReturnsAsync(userId);

        _identityService
            .Setup(x => x.CheckPasswordAsync(userId, command.Password))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.EmailNotVerified.ShouldBeFalse();
        result.UserId.ShouldBeNull();
    }

    [Test]
    public async Task Handle_ShouldReturnEmailNotVerified_WhenEmailNotConfirmed()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var userId = "user-123";

        _identityService
            .Setup(x => x.FindUserByEmailAsync(command.Email))
            .ReturnsAsync(userId);

        _identityService
            .Setup(x => x.CheckPasswordAsync(userId, command.Password))
            .ReturnsAsync(true);

        _identityService
            .Setup(x => x.IsEmailConfirmedAsync(userId))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeFalse();
        result.EmailNotVerified.ShouldBeTrue();
        result.UserId.ShouldBeNull();
    }

    [Test]
    public async Task Handle_ShouldReturnSucceeded_WhenCredentialsValidAndEmailConfirmed()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var userId = "user-123";

        _identityService
            .Setup(x => x.FindUserByEmailAsync(command.Email))
            .ReturnsAsync(userId);

        _identityService
            .Setup(x => x.CheckPasswordAsync(userId, command.Password))
            .ReturnsAsync(true);

        _identityService
            .Setup(x => x.IsEmailConfirmedAsync(userId))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.ShouldBeTrue();
        result.EmailNotVerified.ShouldBeFalse();
        result.UserId.ShouldBe(userId);
    }
}
