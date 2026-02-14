using Hoist.Application.Common.Models;

namespace Hoist.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

    Task<Result> DeleteUserAsync(string userId);

    // Authentication methods
    Task<string?> FindUserByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(string userId, string password);
    Task<bool> IsEmailConfirmedAsync(string userId);
    Task<string> GenerateEmailConfirmationTokenAsync(string userId);
    Task<Result> ConfirmEmailAsync(string userId, string token);
    Task<(Result Result, string UserId)> CreateUserWithDetailsAsync(string firstName, string lastName, string email, string password, int? age = null);
    Task<(Result Result, string UserId)> CreateGoogleUserAsync(string firstName, string lastName, string email, string googleId);
    Task AddGoogleLoginAsync(string userId, string googleId);
    Task<string?> FindUserByGoogleIdAsync(string googleId);
    Task<bool> HasPasswordAsync(string userId);
    Task<string> GeneratePasswordResetTokenAsync(string userId);
    Task<Result> ResetPasswordAsync(string userId, string token, string newPassword);
    Task<bool> HasGoogleLoginAsync(string userId);
}
