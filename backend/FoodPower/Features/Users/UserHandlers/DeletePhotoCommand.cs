using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Services;
using FoodPower.Contracts.Responses.Users;
using FoodPower.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace FoodPower.Features.Users.UserHandlers;

public record DeletePhotoCommand(int UserId) : IRequest<ErrorOr<UserResponse>>;

public class DeletePhotoCommandValidator : AbstractValidator<DeletePhotoCommand>
{
    public DeletePhotoCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithErrorCode(StatusCodes.Status401Unauthorized.ToString())
            .WithMessage("user is not authenticated.");
    }
}

public class DeletePhotoCommandHandler(
    UserManager<AppUser> userManager,
    IFileService fileService)
    : IRequestHandler<DeletePhotoCommand, ErrorOr<UserResponse>>
{
    public async Task<ErrorOr<UserResponse>> Handle(
        DeletePhotoCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "user not found.");
        }

        var previousPath = user.ProfilePicturePath;

        user.ProfilePicturePath = null;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return Error.Failure(
                code: StatusCodes.Status400BadRequest.ToString(),
                description: "failed to remove profile picture.");
        }

        // Best-effort removal of the previous file.
        fileService.DeleteFile(previousPath);

        var roles = await userManager.GetRolesAsync(user);

        return UserResponseFactory.From(user, roles);
    }
}
