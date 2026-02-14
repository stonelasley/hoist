using Hoist.Application.Common.Interfaces;

namespace Hoist.Application.Identity.Commands.ForgotPassword;

public record ForgotPasswordCommand : IRequest
{
    public required string Email { get; init; }
}

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailSender _emailSender;

    public ForgotPasswordCommandHandler(IIdentityService identityService, IEmailSender emailSender)
    {
        _identityService = identityService;
        _emailSender = emailSender;
    }

    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = await _identityService.FindUserByEmailAsync(request.Email);

        // Only send reset email if user exists and has a password (not OAuth-only)
        if (userId != null)
        {
            var hasPassword = await _identityService.HasPasswordAsync(userId);

            if (hasPassword)
            {
                var token = await _identityService.GeneratePasswordResetTokenAsync(userId);
                await _emailSender.SendPasswordResetEmailAsync(request.Email, token, cancellationToken);
            }
        }

        // Always return without error to prevent user enumeration
    }
}
