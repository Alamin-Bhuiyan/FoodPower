using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Services;
using FoodPower.BuildingBlocks.Extensions;
using FoodPower.Contracts.Responses.Users;
using FoodPower.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace FoodPower.Features.Users.UserHandlers;

public record UploadPhotoCommand(
    string Image,
    int UserId
) : IRequest<ErrorOr<UserResponse>>;

public class UploadPhotoCommandValidator : AbstractValidator<UploadPhotoCommand>
{
    private static readonly string[] AllowedTypes = ["png", "jpeg", "webp"];
    private const long MaxSizeInBytes = 5 * 1024 * 1024;

    public UploadPhotoCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithErrorCode(StatusCodes.Status401Unauthorized.ToString())
            .WithMessage("user is not authenticated.");

        RuleFor(x => x.Image)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("image is required.")
            .Must(value => Base64FileValidator.IsValidBase64File(value, AllowedTypes))
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("image must be a valid base64 encoded png, jpeg or webp image.")
            .Must(value => Base64FileValidator.IsWithinSizeLimit(value, MaxSizeInBytes))
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("image must be 5 MB or smaller.");
    }
}

public class UploadPhotoCommandHandler(
    UserManager<AppUser> userManager,
    IFileService fileService)
    : IRequestHandler<UploadPhotoCommand, ErrorOr<UserResponse>>
{
    public async Task<ErrorOr<UserResponse>> Handle(
        UploadPhotoCommand command, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId.ToString());
        if (user == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "user not found.");
        }

        var previousPath = user.ProfilePicturePath;

        var savedPath = await fileService.SaveBase64FileAsync(command.Image, "avatars", cancellationToken);

        user.ProfilePicturePath = savedPath;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            fileService.DeleteFile(savedPath);

            return Error.Failure(
                code: StatusCodes.Status400BadRequest.ToString(),
                description: "failed to update profile picture.");
        }

        // Best-effort removal of the previous file.
        fileService.DeleteFile(previousPath);

        var roles = await userManager.GetRolesAsync(user);

        return UserResponseFactory.From(user, roles);
    }
}
