using System;
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

public record VerifyOtpCommand(
    string Email,
    string Otp
) : IRequest<ErrorOr<MessageResponse>>;

public class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator()
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
            .WithMessage("otp is required.")
            .Length(6)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("otp must be 6 digits.");
    }
}

public class VerifyOtpCommandHandler(
    UserManager<AppUser> userManager,
    IOtpTokenRepository otpTokenRepository)
    : IRequestHandler<VerifyOtpCommand, ErrorOr<MessageResponse>>
{
    public async Task<ErrorOr<MessageResponse>> Handle(
        VerifyOtpCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "user not found.");
        }

        if (user.EmailConfirmed)
        {
            return new MessageResponse("Email is already verified. You can log in.");
        }

        var token = await otpTokenRepository.GetLatestValidAsync(user.Id, OtpPurpose.Register, cancellationToken);
        if (token == null || token.Code != command.Otp)
        {
            return Error.Validation(
                code: StatusCodes.Status400BadRequest.ToString(),
                description: "invalid or expired otp.");
        }

        token.ConsumedAt = DateTime.UtcNow;
        await otpTokenRepository.UpdateAsync(token, cancellationToken);

        user.EmailConfirmed = true;
        await userManager.UpdateAsync(user);

        return new MessageResponse("Email verified successfully. You can now log in.");
    }
}
