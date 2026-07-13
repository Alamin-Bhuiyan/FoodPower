using System.IO;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FoodPower;
using FoodPower.BuildingBlocks.Json;
using FoodPower.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructureServices(builder.Configuration)
    .AddApplication(builder.Configuration)
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
        options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
    });

builder.Host.UseSharedKernel();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FoodPower API",
        Version = "v1",
        Description = "An API for FoodPower - office lunch management",
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Please enter your JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "FoodPower API V1");
    options.RoutePrefix = string.Empty;
});

var screenshotPath = Path.Combine(Directory.GetCurrentDirectory(), "resources", "screenshots");

if (!Directory.Exists(screenshotPath))
{
    Directory.CreateDirectory(screenshotPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(screenshotPath),
    RequestPath = "/resources/screenshots"
});

app.UseCors("FoodPowerFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await DbInitializer.InitializeAsync(app.Services);

app.Run();
