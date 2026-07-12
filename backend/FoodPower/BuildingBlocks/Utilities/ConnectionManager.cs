using System;
using DotNetEnv;
using Microsoft.Extensions.Configuration;

namespace FoodPower.BuildingBlocks.Utilities;

public static class ConnectionManager
{
    public static string GetDbConnectionString(IConfiguration configuration)
    {
        var fromEnv = Env.GetString("DB_CONNECTION_STRING", string.Empty);
        if (!string.IsNullOrWhiteSpace(fromEnv))
        {
            return fromEnv;
        }

        var fromConfig = configuration.GetConnectionString("Database");
        if (!string.IsNullOrWhiteSpace(fromConfig))
        {
            return fromConfig;
        }

        throw new InvalidOperationException(
            "No database connection string configured. Set DB_CONNECTION_STRING or ConnectionStrings:Database.");
    }
}
