namespace Hoist.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendVerificationEmailAsync(string email, string userId, string token, CancellationToken cancellationToken = default);
    Task SendPasswordResetEmailAsync(string email, string token, CancellationToken cancellationToken = default);
}
