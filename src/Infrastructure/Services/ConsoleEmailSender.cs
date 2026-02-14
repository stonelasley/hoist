using Hoist.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Hoist.Infrastructure.Services;

public class ConsoleEmailSender : IEmailSender
{
    private readonly ILogger<ConsoleEmailSender> _logger;

    public ConsoleEmailSender(ILogger<ConsoleEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendVerificationEmailAsync(string email, string userId, string token, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[DEV EMAIL] Verification email to {Email} — userId={UserId}, token={Token}", email, userId, token);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetEmailAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[DEV EMAIL] Password reset email to {Email} — token={Token}", email, token);
        return Task.CompletedTask;
    }
}
