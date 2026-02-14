using System.Net;
using Hoist.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Hoist.Infrastructure.Services;

public class SendGridEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public SendGridEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendVerificationEmailAsync(string email, string userId, string token, CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["SendGrid:ApiKey"];
        var fromEmail = _configuration["SendGrid:FromEmail"];
        var fromName = _configuration["SendGrid:FromName"];
        var baseUrl = _configuration["App:BaseUrl"];

        Guard.Against.NullOrEmpty(apiKey, message: "SendGrid:ApiKey not configured.");
        Guard.Against.NullOrEmpty(fromEmail, message: "SendGrid:FromEmail not configured.");
        Guard.Against.NullOrEmpty(fromName, message: "SendGrid:FromName not configured.");
        Guard.Against.NullOrEmpty(baseUrl, message: "App:BaseUrl not configured.");

        var encodedToken = WebUtility.UrlEncode(token);
        var verificationUrl = $"{baseUrl}/api/users/verify-email-redirect?userId={userId}&token={encodedToken}";

        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(fromEmail, fromName);
        var to = new EmailAddress(email);
        var subject = "Verify your Hoist account";
        var htmlContent = $@"
            <h2>Welcome to Hoist!</h2>
            <p>Please verify your email address by clicking the link below:</p>
            <p><a href=""{verificationUrl}"">Verify Email Address</a></p>
            <p>If you did not create an account, please ignore this email.</p>
        ";

        var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
        await client.SendEmailAsync(msg, cancellationToken);
    }

    public async Task SendPasswordResetEmailAsync(string email, string token, CancellationToken cancellationToken = default)
    {
        var apiKey = _configuration["SendGrid:ApiKey"];
        var fromEmail = _configuration["SendGrid:FromEmail"];
        var fromName = _configuration["SendGrid:FromName"];
        var baseUrl = _configuration["App:BaseUrl"];

        Guard.Against.NullOrEmpty(apiKey, message: "SendGrid:ApiKey not configured.");
        Guard.Against.NullOrEmpty(fromEmail, message: "SendGrid:FromEmail not configured.");
        Guard.Against.NullOrEmpty(fromName, message: "SendGrid:FromName not configured.");
        Guard.Against.NullOrEmpty(baseUrl, message: "App:BaseUrl not configured.");

        var encodedToken = WebUtility.UrlEncode(token);
        var encodedEmail = WebUtility.UrlEncode(email);
        var resetUrl = $"{baseUrl}/api/users/reset-password-redirect?email={encodedEmail}&token={encodedToken}";

        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(fromEmail, fromName);
        var to = new EmailAddress(email);
        var subject = "Reset your Hoist password";
        var htmlContent = $@"
            <h2>Password Reset Request</h2>
            <p>You requested to reset your password. Click the link below to proceed:</p>
            <p><a href=""{resetUrl}"">Reset Password</a></p>
            <p>If you did not request this, please ignore this email and your password will remain unchanged.</p>
        ";

        var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
        await client.SendEmailAsync(msg, cancellationToken);
    }
}
