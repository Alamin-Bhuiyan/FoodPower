using System;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace FoodPower.BuildingBlocks.Logging;

public class SeriLogger
{
    public static Action<HostBuilderContext, IServiceProvider, LoggerConfiguration> Configure() =>
        (context, services, configuration) =>
        {
            configuration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console();
        };
}
