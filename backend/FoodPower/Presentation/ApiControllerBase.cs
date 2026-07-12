using System.Collections.Generic;
using System.Linq;
using ErrorOr;
using FoodPower.Application.Models;
using FoodPower.BuildingBlocks.Extensions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoodPower.Presentation;

[ApiController]
[Route("")]
public partial class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;
    protected readonly ILogger<ApiControllerBase> _logger;

    public ApiControllerBase(ILogger<ApiControllerBase> logger)
    {
        _logger = logger;
    }

    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

    protected ActionResult Problem(List<ErrorOr.Error> errors)
    {
        if (errors.Count is 0)
        {
            return Problem();
        }

        if (errors.All(error => error.Type == ErrorType.Validation))
        {
            return ValidationProblem(errors);
        }

        return new ResponseModel<object>().Problem(errors[0]);
    }

    private ActionResult ValidationProblem(List<ErrorOr.Error> errors)
    {
        var errorEntities = new List<StatusCodeModel>();

        foreach (ErrorOr.Error error in errors)
        {
            errorEntities.Add(new StatusCodeModel() { Code = error.Code, Message = error.Description });
        }

        var code = errorEntities.First().Code;
        if (string.IsNullOrEmpty(code))
        {
            code = StatusCodes.Status400BadRequest.ToString();
        }

        var errorResponse = new ResponseModel<object>()
        {
            Status = new StatusCodeModel() { Code = code, Message = errorEntities.First().Message },
            Data = new object()
        };

        return new ObjectResult(errorResponse)
        {
            StatusCode = StatusCodes.Status400BadRequest
        };
    }

    protected ResponseModel<T> ToSuccess<T>(T data)
    {
        var status = new StatusCodeModel()
        {
            Code = StatusCodes.Status200OK.ToString(),
            Message = "Success",
        };
        return ResponseModel<T>.ToResponse(status, data);
    }

    protected IActionResult OkWithPagination<T>(PaginatedList<T> paginatedList, object statusModelObject)
    {
        Response.AddPaginationHeaders(paginatedList);

        return Ok(statusModelObject);
    }
}
