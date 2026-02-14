using Hoist.Application.Common.Interfaces;

namespace Hoist.Application.Identity.Commands.VerifyEmail;

public record VerifyEmailCommand : IRequest<VerifyEmailResult>
{
    public required string UserId { get; init; }
    public required string Token { get; init; }
}

public record VerifyEmailResult
{
    public bool Succeeded { get; init; }
    public bool AlreadyVerified { get; init; }
}

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, VerifyEmailResult>
{
    private readonly IIdentityService _identityService;

    public VerifyEmailCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<VerifyEmailResult> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var userName = await _identityService.GetUserNameAsync(request.UserId);

        if (userName == null)
        {
            return new VerifyEmailResult { Succeeded = false };
        }

        var isEmailConfirmed = await _identityService.IsEmailConfirmedAsync(request.UserId);

        if (isEmailConfirmed)
        {
            return new VerifyEmailResult
            {
                Succeeded = true,
                AlreadyVerified = true
            };
        }

        var result = await _identityService.ConfirmEmailAsync(request.UserId, request.Token);

        return new VerifyEmailResult
        {
            Succeeded = result.Succeeded
        };
    }
}
