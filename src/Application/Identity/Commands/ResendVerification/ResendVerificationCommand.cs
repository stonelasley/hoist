using Hoist.Application.Common.Interfaces;

namespace Hoist.Application.Identity.Commands.ResendVerification;

public record ResendVerificationCommand : IRequest
{
    public required string Email { get; init; }
}

public class ResendVerificationCommandHandler : IRequestHandler<ResendVerificationCommand>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailSender _emailSender;

    public ResendVerificationCommandHandler(IIdentityService identityService, IEmailSender emailSender)
    {
        _identityService = identityService;
        _emailSender = emailSender;
    }

    public async Task Handle(ResendVerificationCommand request, CancellationToken cancellationToken)
    {
        var userId = await _identityService.FindUserByEmailAsync(request.Email);

        if (userId != null)
        {
            var isEmailConfirmed = await _identityService.IsEmailConfirmedAsync(userId);

            if (!isEmailConfirmed)
            {
                var token = await _identityService.GenerateEmailConfirmationTokenAsync(userId);
                await _emailSender.SendVerificationEmailAsync(request.Email, userId, token, cancellationToken);
            }
        }

        // Always return without error to prevent user enumeration
    }
}
