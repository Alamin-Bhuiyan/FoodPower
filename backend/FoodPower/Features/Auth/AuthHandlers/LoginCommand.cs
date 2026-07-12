using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Services;
using FoodPower.Contracts.Responses.Auth;
using FoodPower.Contracts.Responses.Users;
using FoodPower.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace FoodPower.Features.Auth.AuthHandlers;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<ErrorOr<LoginResponse>>;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
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
            .WithMessage("password is required.");
    }
}

public class LoginCommandHandler(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IAuthService authService)
    : IRequestHandler<LoginCommand, ErrorOr<LoginResponse>>
{
    public async Task<ErrorOr<LoginResponse>> Handle(
        LoginCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "user not found.");
        }

        if (!user.EmailConfirmed)
        {
            return Error.Unauthorized(
                code: StatusCodes.Status401Unauthorized.ToString(),
                description: "account is not verified. Please verify your email first.");
        }

        if (!user.IsActive)
        {
            return Error.Forbidden(
                code: StatusCodes.Status403Forbidden.ToString(),
                description: "account is deactivated.");
        }

        var check = await signInManager.CheckPasswordSignInAsync(user, command.Password, lockoutOnFailure: false);
        if (!check.Succeeded)
        {
            return Error.Unauthorized(
                code: StatusCodes.Status401Unauthorized.ToString(),
                description: "password is invalid.");
        }

        var roles = await userManager.GetRolesAsync(user);
        var token = authService.CreateToken(user, roles);

        return new LoginResponse(token, UserResponseFactory.From(user, roles));
    }
}
