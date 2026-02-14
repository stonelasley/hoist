using Google.Apis.Auth;
using Hoist.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Hoist.Application.Identity.Commands.GoogleLogin;

public record GoogleLoginCommand : IRequest<GoogleLoginResult>
{
    public required string IdToken { get; init; }
}

public record GoogleLoginResult
{
    public bool Succeeded { get; init; }
    public string? UserId { get; init; }
    public bool IsNewUser { get; init; }
}

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, GoogleLoginResult>
{
    private readonly IIdentityService _identityService;
    private readonly IConfiguration _configuration;

    public GoogleLoginCommandHandler(IIdentityService identityService, IConfiguration configuration)
    {
        _identityService = identityService;
        _configuration = configuration;
    }

    public async Task<GoogleLoginResult> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var clientId = _configuration["Google:ClientId"];

            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);

            var email = payload.Email;
            var firstName = payload.GivenName ?? string.Empty;
            var lastName = payload.FamilyName ?? string.Empty;
            var googleId = payload.Subject;

            // Check if user exists by Google ID
            var existingUserId = await _identityService.FindUserByGoogleIdAsync(googleId);

            if (existingUserId != null)
            {
                return new GoogleLoginResult
                {
                    Succeeded = true,
                    UserId = existingUserId,
                    IsNewUser = false
                };
            }

            // Check if user exists by email
            var userIdByEmail = await _identityService.FindUserByEmailAsync(email);

            if (userIdByEmail != null)
            {
                // Link Google account to existing user
                var hasGoogleLogin = await _identityService.HasGoogleLoginAsync(userIdByEmail);

                if (!hasGoogleLogin)
                {
                    await _identityService.AddGoogleLoginAsync(userIdByEmail, googleId);
                }

                return new GoogleLoginResult
                {
                    Succeeded = true,
                    UserId = userIdByEmail,
                    IsNewUser = false
                };
            }

            // Create new user
            var (result, userId) = await _identityService.CreateGoogleUserAsync(firstName, lastName, email, googleId);

            if (result.Succeeded)
            {
                return new GoogleLoginResult
                {
                    Succeeded = true,
                    UserId = userId,
                    IsNewUser = true
                };
            }

            return new GoogleLoginResult { Succeeded = false };
        }
        catch (Exception)
        {
            return new GoogleLoginResult { Succeeded = false };
        }
    }
}
