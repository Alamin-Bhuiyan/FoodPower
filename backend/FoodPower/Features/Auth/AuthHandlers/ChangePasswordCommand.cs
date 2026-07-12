using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Contracts.Responses;
using FoodPower.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace FoodPower.Features.Auth.AuthHandlers;

public record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword,
    int UserId
) : IRequest<ErrorOr<MessageResponse>>;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("old_password is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("new_password is required.")
            .MinimumLength(6)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("new_password must be at least 6 characters.");
    }
}

public class ChangePasswordCommandHandler(
    UserManager<AppUser> userManager)
    : IRequestHandler<ChangePasswordCommand, ErrorOr<MessageResponse>>
{
    public async Task<ErrorOr<MessageResponse>> Handle(
        ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "user not found.");
        }

        var result = await userManager.ChangePasswordAsync(user, command.CurrentPassword, command.NewPassword);
        if (!result.Succeeded)
        {
            return Error.Validation(
                code: StatusCodes.Status400BadRequest.ToString(),
                description: string.Join(" ", result.Errors.Select(e => e.Description)));
        }

        return new MessageResponse("Password changed successfully.");
    }
}
