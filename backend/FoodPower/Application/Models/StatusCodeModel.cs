using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace FoodPower.Application.Models;

public class StatusCodeModel
{
    public StatusCodeModel()
    {
    }

    [SetsRequiredMembers]
    public StatusCodeModel(string code, string message)
    {
        Code = code;
        Message = message;
    }

    [JsonProperty("code")]
    [JsonPropertyName("code")]
    public required string Code { get; set; }

    [JsonProperty("message")]
    [JsonPropertyName("message")]
    public required string Message { get; set; }
}
