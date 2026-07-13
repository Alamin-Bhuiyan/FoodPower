using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetEnv;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Application.Interfaces.Services;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.BuildingBlocks.Utilities;
using FoodPower.Data;
using FoodPower.Domain.Entities;
using FoodPower.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoodPower.Features.Auth.Services.Emails;

public class PollEmailService(
    ApplicationDbContext dbContext,
    ISettingsRepository settingsRepository,
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration configuration,
    ILogger<PollEmailService> logger) : IPollEmailService
{
    public async Task<int> QueuePollPublishedEmailsAsync(Poll poll, CancellationToken cancellationToken = default)
    {
        // Admins should not receive the broadcast (they publish the poll themselves).
        var adminRoleId = await dbContext.Roles
            .Where(r => r.Name == PermissionRole.Admin)
            .Select(r => r.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var recipients = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.IsActive && u.EmailConfirmed && u.Email != null && u.Email != "")
            .Where(u => !dbContext.UserRoles.Any(ur => ur.UserId == u.Id && ur.RoleId == adminRoleId))
            .Select(u => u.Email!)
            .ToListAsync(cancellationToken);

        if (recipients.Count == 0)
        {
            return 0;
        }

        var timeZone = await settingsRepository.GetTimeZoneAsync(cancellationToken);
        var bkashNumber = await settingsRepository.GetValueAsync(SettingKeys.BkashNumber, cancellationToken);
        var bankAccount = await settingsRepository.GetValueAsync(SettingKeys.BankAccount, cancellationToken);

        var frontendBaseUrl = Env.GetString(
            "FRONTEND_BASE_URL",
            configuration["AppSettings:FrontendBaseUrl"] ?? "https://food-power.vercel.app").TrimEnd('/');

        var lunchDateText = poll.LunchDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        var cutoffLocalText = TimeZoneHelper.FromUtc(poll.CutoffAt, timeZone)
            .ToString("dd MMM yyyy hh:mm tt", CultureInfo.InvariantCulture);
        var optionNames = poll.Options.OrderBy(o => o.SortOrder).Select(o => o.Name).ToList();
        var pollUrl = $"{frontendBaseUrl}/poll/{poll.ShareToken}";

        // General polls never affect dues, so the weekly-payment reminder is only
        // relevant for Lunch polls.
        string subject;
        string body;

        if (poll.Type == PollType.General)
        {
            subject = EmailTemplates.GeneralPollPublishedSubject(poll.Question);
            body = EmailTemplates.GeneralPollPublishedBody(
                poll.Question, optionNames, cutoffLocalText, pollUrl);
        }
        else
        {
            subject = EmailTemplates.PollPublishedSubject(lunchDateText);
            body = EmailTemplates.PollPublishedBody(
                lunchDateText, optionNames, cutoffLocalText, pollUrl, bkashNumber, bankAccount);
        }

        var pollId = poll.Id;

        // Fire-and-forget: sending emails must never fail or slow down the request,
        // so the SMTP work runs on a fresh DI scope in the background.
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                foreach (var email in recipients)
                {
                    try
                    {
                        await emailService.SendAsync(email, subject, body, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex,
                            "poll-published email failed for {Email} (poll {PollId}).", email, pollId);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "poll-published email broadcast failed for poll {PollId}.", pollId);
            }
        });

        return recipients.Count;
    }
}
