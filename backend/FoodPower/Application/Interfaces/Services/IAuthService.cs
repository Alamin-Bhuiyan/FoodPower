using System.Collections.Generic;
using FoodPower.Domain.Entities;

namespace FoodPower.Application.Interfaces.Services;

public interface IAuthService
{
    string CreateToken(AppUser user, IList<string> roles);
    string GenerateOtp(int length = 6);
}
