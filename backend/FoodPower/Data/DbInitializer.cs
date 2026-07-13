using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetEnv;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoodPower.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");

        try
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            if (dbContext.Database.GetMigrations().Any())
            {
                await dbContext.Database.MigrateAsync();
            }
            else
            {
                await dbContext.Database.EnsureCreatedAsync();
            }

            await SeedRolesAsync(scope.ServiceProvider);
            await SeedAdminUserAsync(scope.ServiceProvider, logger);
            await SeedSettingsAsync(dbContext);
            await SeedSampleCatererAsync(dbContext);

            logger.LogInformation("Database initialization completed.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database initialization failed.");
        }
    }

    private static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

        foreach (var role in new[] { PermissionRole.Admin, PermissionRole.User })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<int>(role));
            }
        }
    }

    private static async Task SeedAdminUserAsync(IServiceProvider serviceProvider, ILogger logger)
    {
        var adminEmail = Env.GetString("ADMIN_EMAIL", string.Empty);
        var adminPassword = Env.GetString("ADMIN_PASSWORD", string.Empty);

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            return;
        }

        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();

        var existing = await userManager.FindByEmailAsync(adminEmail);
        if (existing != null)
        {
            if (!await userManager.IsInRoleAsync(existing, PermissionRole.Admin))
            {
                await userManager.AddToRoleAsync(existing, PermissionRole.Admin);
            }
            return;
        }

        var admin = new AppUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FullName = "Md. Mohibur Rahman",
            IsActive = true,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(admin, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, PermissionRole.Admin);
            logger.LogInformation("Seeded admin user {Email}.", adminEmail);
        }
        else
        {
            logger.LogWarning("Failed to seed admin user: {Errors}",
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }

    private static async Task SeedSettingsAsync(ApplicationDbContext dbContext)
    {
        var defaults = new Dictionary<string, string>
        {
            [SettingKeys.PricePerLunch] = SettingKeys.DefaultPricePerLunch,
            [SettingKeys.DefaultCutoffTime] = SettingKeys.DefaultCutoffTimeValue,
            [SettingKeys.TimeZone] = SettingKeys.DefaultTimeZone,
            [SettingKeys.BkashNumber] = SettingKeys.DefaultBkashNumber,
            [SettingKeys.BankAccount] = SettingKeys.DefaultBankAccount
        };

        foreach (var (key, value) in defaults)
        {
            if (!await dbContext.Settings.AnyAsync(s => s.Key == key))
            {
                await dbContext.Settings.AddAsync(new Setting(key, value));
            }
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedSampleCatererAsync(ApplicationDbContext dbContext)
    {
        if (await dbContext.Caterers.AnyAsync())
        {
            return;
        }

        var caterer = new Caterer("Deshi Kitchen Catering", "+8801700000000", 120m);
        await dbContext.Caterers.AddAsync(caterer);
        await dbContext.SaveChangesAsync();

        var menuItems = new List<MenuItem>
        {
            new(caterer.Id, DayOfWeek.Monday, "সাদা ভাত, গরু", null),
            new(caterer.Id, DayOfWeek.Monday, "সাদা ভাত, মুরগী", null),
            new(caterer.Id, DayOfWeek.Tuesday, "সাদা ভাত, মাছ", null),
            new(caterer.Id, DayOfWeek.Tuesday, "সাদা ভাত, মুরগী", null),
            new(caterer.Id, DayOfWeek.Wednesday, "সাদা ভাত, গরু", null),
            new(caterer.Id, DayOfWeek.Wednesday, "সাদা ভাত, মুরগী", null),
            new(caterer.Id, DayOfWeek.Friday, "পোলাও, মুরগী", null)
        };

        await dbContext.MenuItems.AddRangeAsync(menuItems);
        await dbContext.SaveChangesAsync();
    }
}
