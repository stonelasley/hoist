using Hoist.Application.Identity.Commands.ForgotPassword;
using Hoist.Application.Identity.Commands.GoogleLogin;
using Hoist.Application.Identity.Commands.Login;
using Hoist.Application.Identity.Commands.Register;
using Hoist.Application.Identity.Commands.ResendVerification;
using Hoist.Application.Identity.Commands.ResetPassword;
using Hoist.Application.Identity.Commands.VerifyEmail;
using Hoist.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;

namespace Hoist.Web.Endpoints;

public class Users : EndpointGroupBase
{
    public override void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(LoginUser, "login").AllowAnonymous();
        groupBuilder.MapPost(RegisterUser, "register").AllowAnonymous();
        groupBuilder.MapPost(VerifyUserEmail, "verify-email").AllowAnonymous();
        groupBuilder.MapPost(ResendUserVerification, "resend-verification").AllowAnonymous();
        groupBuilder.MapPost(GoogleLoginUser, "google-login").AllowAnonymous();
        groupBuilder.MapPost(ForgotUserPassword, "forgot-password").AllowAnonymous();
        groupBuilder.MapPost(ResetUserPassword, "reset-password").AllowAnonymous();

        groupBuilder.MapGet(VerifyEmailRedirect, "verify-email-redirect").AllowAnonymous();
        groupBuilder.MapGet(ResetPasswordRedirect, "reset-password-redirect").AllowAnonymous();
    }

    public async Task<IResult> LoginUser(
        ISender sender,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        LoginCommand command)
    {
        var result = await sender.Send(command);

        if (result.EmailNotVerified)
        {
            return Results.Json(new { detail = "email_not_verified" }, statusCode: 401);
        }

        if (!result.Succeeded)
        {
            return Results.Unauthorized();
        }

        var user = await userManager.FindByIdAsync(result.UserId!);

        if (user == null)
        {
            return Results.Unauthorized();
        }

        signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
        await signInManager.SignInAsync(user, isPersistent: false);

        return Results.Empty;
    }

    public async Task<IResult> RegisterUser(ISender sender, RegisterCommand command)
    {
        var result = await sender.Send(command);

        if (result.Succeeded)
        {
            return Results.Ok();
        }

        var errors = result.Errors.ToDictionary(
            error => "RegistrationError",
            error => new[] { error }
        );

        return Results.ValidationProblem(errors);
    }

    public async Task<IResult> VerifyUserEmail(ISender sender, VerifyEmailCommand command)
    {
        var result = await sender.Send(command);

        if (result.Succeeded)
        {
            if (result.AlreadyVerified)
            {
                return Results.Ok(new { message = "Email already verified" });
            }

            return Results.Ok(new { message = "Email verified successfully" });
        }

        return Results.BadRequest(new { message = "Email verification failed" });
    }

    public async Task<IResult> ResendUserVerification(ISender sender, ResendVerificationCommand command)
    {
        await sender.Send(command);
        return Results.Ok(new { message = "If the email exists and is unverified, a verification email has been sent" });
    }

    public async Task<IResult> GoogleLoginUser(
        ISender sender,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        GoogleLoginCommand command)
    {
        var result = await sender.Send(command);

        if (!result.Succeeded)
        {
            return Results.Unauthorized();
        }

        var user = await userManager.FindByIdAsync(result.UserId!);

        if (user == null)
        {
            return Results.Unauthorized();
        }

        signInManager.AuthenticationScheme = IdentityConstants.BearerScheme;
        await signInManager.SignInAsync(user, isPersistent: false);

        return Results.Empty;
    }

    public async Task<IResult> ForgotUserPassword(ISender sender, ForgotPasswordCommand command)
    {
        await sender.Send(command);
        return Results.Ok(new { message = "If the email exists, a password reset link has been sent" });
    }

    public async Task<IResult> ResetUserPassword(ISender sender, ResetPasswordCommand command)
    {
        var result = await sender.Send(command);

        if (result.Succeeded)
        {
            return Results.Ok(new { message = "Password reset successfully" });
        }

        var errors = result.Errors.ToDictionary(
            error => "PasswordResetError",
            error => new[] { error }
        );

        return Results.ValidationProblem(errors);
    }

    public IResult VerifyEmailRedirect(string userId, string token)
    {
        var encodedUserId = Uri.EscapeDataString(userId);
        var encodedToken = Uri.EscapeDataString(token);
        return Results.Redirect($"hoist://verify-email?userId={encodedUserId}&token={encodedToken}");
    }

    public IResult ResetPasswordRedirect(string email, string token)
    {
        var encodedEmail = Uri.EscapeDataString(email);
        var encodedToken = Uri.EscapeDataString(token);
        return Results.Redirect($"hoist://reset-password?email={encodedEmail}&token={encodedToken}");
    }
}
