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

public record ResendOtpCommand(
    string Email,
    string? Purpose
) : IRequest<ErrorOr<MessageResponse>>;

public class ResendOtpCommandValidator : AbstractValidator<ResendOtpCommand>
{
    public ResendOtpCommandValidator()
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

public class ResendOtpCommandHandler(
    UserManager<AppUser> userManager,
    IOtpTokenRepository otpTokenRepository,
    IAuthService authService,
    IEmailService emailService,
    ILogger<ResendOtpCommandHandler> logger)
    : IRequestHandler<ResendOtpCommand, ErrorOr<MessageResponse>>
{
    public async Task<ErrorOr<MessageResponse>> Handle(
        ResendOtpCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "user not found.");
        }

        var purpose = string.Equals(command.Purpose, "reset_password", StringComparison.OrdinalIgnoreCase)
            ? OtpPurpose.ResetPassword
            : OtpPurpose.Register;

        if (purpose == OtpPurpose.Register && user.EmailConfirmed)
        {
            return Error.Conflict(
                code: StatusCodes.Status409Conflict.ToString(),
                description: "email is already verified.");
        }

        await otpTokenRepository.InvalidateAllAsync(user.Id, purpose, cancellationToken);

        var otp = authService.GenerateOtp();
        await otpTokenRepository.AddAsync(new OtpToken(user.Id, otp, purpose), cancellationToken);

        var purposeLabel = purpose == OtpPurpose.Register ? "Registration" : "Password reset";

        try
        {
            await emailService.SendAsync(
                user.Email!,
                EmailTemplates.OtpEmailSubject(purposeLabel),
                EmailTemplates.OtpEmailBody(user.FullName, otp, purposeLabel),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to resend OTP email to {Email}", user.Email);
        }

        return new MessageResponse("A new verification code has been sent to your email.");
    }
}
