using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Application.Interfaces.Services;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Contracts.Responses.Auth;
using FoodPower.Domain.Entities;
using FoodPower.Domain.Enums;
using FoodPower.Features.Auth.Services.Emails;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Auth.AuthHandlers;

public record RegisterCommand(
    string FullName,
    string Email,
    string Password
) : IRequest<ErrorOr<RegisterResponse>>;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("full_name is required.")
            .MaximumLength(150)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("full_name can be max of 150 characters.");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("email is required.")
            .EmailAddress()
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("password is required.")
            .MinimumLength(6)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("password must be at least 6 characters.");
    }
}

public class RegisterCommandHandler(
    UserManager<AppUser> userManager,
    IOtpTokenRepository otpTokenRepository,
    IAuthService authService,
    IEmailService emailService,
    ILogger<RegisterCommandHandler> logger)
    : IRequestHandler<RegisterCommand, ErrorOr<RegisterResponse>>
{
    public async Task<ErrorOr<RegisterResponse>> Handle(
        RegisterCommand command, CancellationToken cancellationToken)
    {
        var existing = await userManager.FindByEmailAsync(command.Email);

        if (existing is { EmailConfirmed: true })
        {
            return Error.Conflict(
                code: StatusCodes.Status409Conflict.ToString(),
                description: "email already exists!");
        }

        if (existing != null)
        {
            // Unverified leftover registration - replace it.
            await userManager.DeleteAsync(existing);
        }

        var user = new AppUser
        {
            UserName = command.Email,
            Email = command.Email,
            FullName = command.FullName,
            IsActive = true,
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
        {
            return Error.Validation(
                code: StatusCodes.Status400BadRequest.ToString(),
                description: string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        await userManager.AddToRoleAsync(user, PermissionRole.User);

        var otp = authService.GenerateOtp();
        await otpTokenRepository.AddAsync(new OtpToken(user.Id, otp, OtpPurpose.Register), cancellationToken);

        try
        {
            await emailService.SendAsync(
                user.Email!,
                EmailTemplates.OtpEmailSubject("Registration"),
                EmailTemplates.OtpEmailBody(user.FullName, otp, "Registration"),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send registration OTP email to {Email}", user.Email);
        }

        return new RegisterResponse(
            user.FullName,
            user.Email!,
            "Registration successful. Please check your email for the verification code.");
    }
}
