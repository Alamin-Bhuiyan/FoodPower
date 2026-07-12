using FoodPower.Presentation;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Auth.AuthControllers;

public class AuthBaseController : ApiControllerBase
{
    protected AuthBaseController(ILogger<AuthBaseController> logger) : base(logger)
    {
    }
}
