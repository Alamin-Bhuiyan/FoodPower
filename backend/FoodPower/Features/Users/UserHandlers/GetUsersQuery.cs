using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FoodPower.Contracts.Responses.Users;
using FoodPower.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FoodPower.Features.Users.UserHandlers;

public record GetUsersQuery() : IRequest<ErrorOr<List<UserResponse>>>;

public class GetUsersQueryHandler(ApplicationDbContext dbContext)
    : IRequestHandler<GetUsersQuery, ErrorOr<List<UserResponse>>>
{
    public async Task<ErrorOr<List<UserResponse>>> Handle(
        GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await dbContext.Users
            .Where(u => u.IsActive && u.EmailConfirmed)
            .OrderBy(u => u.FullName)
            .ToListAsync(cancellationToken);

        return users
            .Select(u => new UserResponse(
                u.Id,
                u.FullName,
                u.Email,
                u.IsActive,
                u.EmailConfirmed,
                [],
                u.CreatedAt))
            .ToList();
    }
}
