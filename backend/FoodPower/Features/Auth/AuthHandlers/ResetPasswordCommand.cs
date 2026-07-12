using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Contracts.Responses;
using FoodPower.Domain.Entities;
using FoodPower.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace FoodPower.Features.Auth.AuthHandlers;

public record ResetPasswordCommand(
    string Email,
    string Otp,
    string NewPassword
) : IRequest<ErrorOr<MessageResponse>>;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("email is required.")
            .EmailAddress()
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("invalid email format.");

        RuleFor(x => x.Otp)
            .NotEmpty()
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("otp is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("new_password is required.")
            .MinimumLength(6)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("new_password must be at least 6 characters.");
    }
}

public class ResetPasswordCommandHandler(
    UserManager<AppUser> userManager,
    IOtpTokenRepository otpTokenRepository)
    : IRequestHandler<ResetPasswordCommand, ErrorOr<MessageResponse>>
{
    public async Task<ErrorOr<MessageResponse>> Handle(
        ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "user not found.");
        }

        var token = await otpTokenRepository.GetLatestValidAsync(user.Id, OtpPurpose.ResetPassword, cancellationToken);
        if (token == null || token.Code != command.Otp)
        {
            return Error.Validation(
                code: StatusCodes.Status400BadRequest.ToString(),
                description: "invalid or expired otp.");
        }

        token.ConsumedAt = DateTime.UtcNow;
        await otpTokenRepository.UpdateAsync(token, cancellationToken);

        var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, resetToken, command.NewPassword);

        if (!result.Succeeded)
        {
            return Error.Validation(
                code: StatusCodes.Status400BadRequest.ToString(),
                description: string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        return new MessageResponse("Password has been reset successfully. You can now log in.");
    }
}
