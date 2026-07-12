using System.Text.Json.Serialization;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace FoodPower.Application.Models;

public class ResponseModel<T>
{
    [JsonProperty("status")]
    [JsonPropertyName("status")]
    public StatusCodeModel Status { get; set; } = default!;

    [JsonProperty("data")]
    [JsonPropertyName("data")]
    public T Data { get; set; } = default!;

    public static ResponseModel<TData> ToResponse<TData>(StatusCodeModel status, TData? data = default)
    {
        data ??= default;

        var response = new ResponseModel<TData>()
        {
            Status = status,
            Data = data!
        };
        return response;
    }

    public ObjectResult Problem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };

        var result = Result(error);
        return new ObjectResult(result) { StatusCode = statusCode };
    }

    private static object Result(Error error)
    {
        var errorEntity = new StatusCodeModel() { Code = error.Code, Message = error.Description };
        var result = ToResponse<object>(errorEntity);
        return result;
    }
}
