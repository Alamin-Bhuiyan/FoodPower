using System.Collections.Generic;
using System.Linq;
using FoodPower.Domain.Entities;

namespace FoodPower.Contracts.Responses.Users;

public static class UserResponseFactory
{
    public static UserResponse From(AppUser user, IEnumerable<string>? roles) =>
        new(
            id: user.Id,
            full_name: user.FullName,
            email: user.Email,
            is_active: user.IsActive,
            email_confirmed: user.EmailConfirmed,
            roles: roles?.ToList() ?? [],
            created_at: user.CreatedAt);
}
