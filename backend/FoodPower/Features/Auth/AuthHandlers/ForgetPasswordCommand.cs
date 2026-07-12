using System;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Application.Interfaces.Services;
using FoodPower.Contracts.Responses;
using FoodPower.Domain.Entities;
using FoodPower.Domain.Enums;
using FoodPower.Features.Auth.Services.Emails;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Auth.AuthHandlers;

public record ForgetPasswordCommand(
    string Email
) : IRequest<ErrorOr<MessageResponse>>;

public class ForgetPasswordCommandValidator : AbstractValidator<ForgetPasswordCommand>
{
    public ForgetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("email is required.")
            .EmailAddress()
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("invalid email format.");
    }
}

public class ForgetPasswordCommandHandler(
    UserManager<AppUser> userManager,
    IOtpTokenRepository otpTokenRepository,
    IAuthService authService,
    IEmailService emailService,
    ILogger<ForgetPasswordCommandHandler> logger)
    : IRequestHandler<ForgetPasswordCommand, ErrorOr<MessageResponse>>
{
    public async Task<ErrorOr<MessageResponse>> Handle(
        ForgetPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "user not found.");
        }

        await otpTokenRepository.InvalidateAllAsync(user.Id, OtpPurpose.ResetPassword, cancellationToken);

        var otp = authService.GenerateOtp();
        await otpTokenRepository.AddAsync(new OtpToken(user.Id, otp, OtpPurpose.ResetPassword), cancellationToken);

        try
        {
            await emailService.SendAsync(
                user.Email!,
                EmailTemplates.OtpEmailSubject("Password reset"),
                EmailTemplates.OtpEmailBody(user.FullName, otp, "Password reset"),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send password reset OTP email to {Email}", user.Email);
        }

        return new MessageResponse("A password reset code has been sent to your email.");
    }
}
