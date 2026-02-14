using Hoist.Application.Common.Interfaces;

namespace Hoist.Application.Identity.Commands.ResetPassword;

public record ResetPasswordCommand : IRequest<ResetPasswordResult>
{
    public required string Email { get; init; }
    public required string Token { get; init; }
    public required string NewPassword { get; init; }
}

public record ResetPasswordResult
{
    public bool Succeeded { get; init; }
    public string[] Errors { get; init; } = [];
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResult>
{
    private readonly IIdentityService _identityService;

    public ResetPasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<ResetPasswordResult> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = await _identityService.FindUserByEmailAsync(request.Email);

        if (userId == null)
        {
            return new ResetPasswordResult { Succeeded = false };
        }

        var result = await _identityService.ResetPasswordAsync(userId, request.Token, request.NewPassword);

        if (result.Succeeded)
        {
            return new ResetPasswordResult { Succeeded = true };
        }

        return new ResetPasswordResult
        {
            Succeeded = false,
            Errors = result.Errors
        };
    }
}
