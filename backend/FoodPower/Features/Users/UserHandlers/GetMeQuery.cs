using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Contracts.Responses.Users;
using FoodPower.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace FoodPower.Features.Users.UserHandlers;

public record GetMeQuery(int UserId) : IRequest<ErrorOr<UserResponse>>;

public class GetMeQueryValidator : AbstractValidator<GetMeQuery>
{
    public GetMeQueryValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0)
            .WithErrorCode(StatusCodes.Status401Unauthorized.ToString())
            .WithMessage("user is not authenticated.");
    }
}

public class GetMeQueryHandler(UserManager<AppUser> userManager)
    : IRequestHandler<GetMeQuery, ErrorOr<UserResponse>>
{
    public async Task<ErrorOr<UserResponse>> Handle(
        GetMeQuery request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString());
        if (user == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "user not found.");
        }

        var roles = await userManager.GetRolesAsync(user);

        return UserResponseFactory.From(user, roles);
    }
}
