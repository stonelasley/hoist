using Hoist.Application.Common.Interfaces;

namespace Hoist.Application.Identity.Commands.Login;

public record LoginCommand : IRequest<LoginResult>
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}

public record LoginResult
{
    public bool Succeeded { get; init; }
    public bool EmailNotVerified { get; init; }
    public string? UserId { get; init; }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var userId = await _identityService.FindUserByEmailAsync(request.Email);

        if (userId == null)
        {
            return new LoginResult { Succeeded = false };
        }

        var isPasswordValid = await _identityService.CheckPasswordAsync(userId, request.Password);

        if (!isPasswordValid)
        {
            return new LoginResult { Succeeded = false };
        }

        var isEmailConfirmed = await _identityService.IsEmailConfirmedAsync(userId);

        if (!isEmailConfirmed)
        {
            return new LoginResult
            {
                Succeeded = false,
                EmailNotVerified = true
            };
        }

        return new LoginResult
        {
            Succeeded = true,
            UserId = userId
        };
    }
}
