using System.Reflection;
using System.Text;
using DotNetEnv;
using FluentValidation;
using FoodPower.Application.Behaviors;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Application.Interfaces.Services;
using FoodPower.BuildingBlocks.Logging;
using FoodPower.BuildingBlocks.Utilities;
using FoodPower.Data;
using FoodPower.Data.Repositories;
using FoodPower.Domain.Entities;
using FoodPower.Features.Auth.Services;
using FoodPower.Features.Auth.Services.Emails;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace FoodPower;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        Env.Load();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(ConnectionManager.GetDbConnectionString(configuration)));

        services
            .AddIdentityCore<AppUser>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<int>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddSingleton(_ => new EmailSettings
        {
            Host = Env.GetString("EMAIL_SMTP_HOST", "smtp.gmail.com"),
            Port = Env.GetInt("EMAIL_SMTP_PORT", 587),
            SenderName = Env.GetString("EMAIL_SENDER_NAME", "FoodPower"),
            SenderAddress = Env.GetString("EMAIL_SENDER_ADDRESS", string.Empty),
            Password = Env.GetString("EMAIL_SENDER_PASSWORD", string.Empty),
            UseSsl = Env.GetBool("EMAIL_USE_SSL", true)
        });

        services.AddPersistenceServices();

        services.AddHttpContextAccessor();

        return services;
    }

    public static IHostBuilder UseSharedKernel(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog((context, services, configuration) =>
        {
            SeriLogger.Configure()(context, services, configuration);
        });

        return hostBuilder;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddMediatR(options =>
        {
            options.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddCors(options =>
        {
            options.AddPolicy("FoodPowerFrontend",
                builder => builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithExposedHeaders("*"));
        });

        var jwtSettings = new JwtSettings
        {
            Token = Env.GetString("JWT_SECRET", configuration["AppSettings:Token"] ?? string.Empty),
            Issuer = Env.GetString("JWT_ISSUER", configuration["AppSettings:Issuer"] ?? "FoodPower"),
            Audience = Env.GetString("JWT_AUDIENCE", configuration["AppSettings:Audience"] ?? "FoodPower")
        };
        services.AddSingleton(jwtSettings);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Token)),
                    ValidateIssuerSigningKey = true
                };
            });

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuthUser, AuthUser>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPollEmailService, PollEmailService>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<INotificationService, NotificationService>();

        services.AddMappings(typeof(DependencyInjection).Assembly);

        return services;
    }

    private static void AddPersistenceServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(IEfRepository<>), typeof(EfRepository<>));
        services.AddScoped<IOtpTokenRepository, OtpTokenRepository>();
        services.AddScoped<ICatererRepository, CatererRepository>();
        services.AddScoped<IMenuItemRepository, MenuItemRepository>();
        services.AddScoped<IPollRepository, PollRepository>();
        services.AddScoped<IVoteRepository, VoteRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<ISettingsRepository, SettingsRepository>();
        services.AddScoped<IDuesRepository, DuesRepository>();
    }

    private static void AddMappings(this IServiceCollection services, Assembly? assembly = null)
    {
        var config = TypeAdapterConfig.GlobalSettings;
        if (assembly != null)
        {
            config.Scan(assembly);
        }
        else
        {
            config.Scan(Assembly.GetExecutingAssembly());
        }
        services.AddSingleton(config);
    }
}
