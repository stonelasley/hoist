using Hoist.Application.Common.Interfaces;

namespace Hoist.Application.Identity.Commands.Register;

public record RegisterCommand : IRequest<RegisterResult>
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
    public int? Age { get; init; }
}

public record RegisterResult
{
    public bool Succeeded { get; init; }
    public string[] Errors { get; init; } = [];
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResult>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailSender _emailSender;

    public RegisterCommandHandler(IIdentityService identityService, IEmailSender emailSender)
    {
        _identityService = identityService;
        _emailSender = emailSender;
    }

    public async Task<RegisterResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var (result, userId) = await _identityService.CreateUserWithDetailsAsync(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            request.Age);

        if (result.Succeeded)
        {
            var token = await _identityService.GenerateEmailConfirmationTokenAsync(userId);
            await _emailSender.SendVerificationEmailAsync(request.Email, userId, token, cancellationToken);

            return new RegisterResult { Succeeded = true };
        }

        return new RegisterResult
        {
            Succeeded = false,
            Errors = result.Errors
        };
    }
}
