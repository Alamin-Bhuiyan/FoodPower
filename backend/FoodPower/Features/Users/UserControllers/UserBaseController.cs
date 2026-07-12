using FoodPower.Presentation;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Users.UserControllers;

public class UserBaseController : ApiControllerBase
{
    protected UserBaseController(ILogger<UserBaseController> logger) : base(logger)
    {
    }
}
