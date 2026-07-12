using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Application.Interfaces.Services;
using FoodPower.BuildingBlocks.Utilities;
using FoodPower.Contracts.Responses.Polls;
using FoodPower.Domain.Entities;
using FoodPower.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodPower.Features.Polls.PollHandlers;

public record PollOptionInput(int? MenuItemId, string? CustomName);

public record CreatePollCommand(
    string LunchDate,
    int? CatererId,
    string? Question,
    string? CutoffAt,
    List<PollOptionInput> Options,
    int UserId
) : IRequest<ErrorOr<PollResponse>>;

public class CreatePollCommandValidator : AbstractValidator<CreatePollCommand>
{
    public CreatePollCommandValidator()
    {
        RuleFor(x => x.LunchDate)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("lunch_date is required.")
            .Must(value => DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("lunch_date is invalid. Use ISO format (yyyy-MM-dd).");

        RuleFor(x => x.Options)
            .NotEmpty()
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("at least one option is required.");

        RuleForEach(x => x.Options).ChildRules(option =>
        {
            option.RuleFor(o => o)
                .Must(o => o.MenuItemId.HasValue || !string.IsNullOrWhiteSpace(o.CustomName))
                .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
                .WithMessage("each option needs a menu_item_id or a custom_name.");
        });
    }
}

public class CreatePollCommandHandler(
    IPollRepository pollRepository,
    IMenuItemRepository menuItemRepository,
    ICatererRepository catererRepository,
    ISettingsRepository settingsRepository,
    INotificationService notificationService)
    : IRequestHandler<CreatePollCommand, ErrorOr<PollResponse>>
{
    public async Task<ErrorOr<PollResponse>> Handle(
        CreatePollCommand command, CancellationToken cancellationToken)
    {
        var lunchDate = DateTime.Parse(command.LunchDate, CultureInfo.InvariantCulture, DateTimeStyles.None).Date;

        if (await pollRepository.AnyOpenForDateAsync(lunchDate, cancellationToken))
        {
            return Error.Conflict(
                code: StatusCodes.Status409Conflict.ToString(),
                description: "an open poll already exists for this lunch date.");
        }

        // Price snapshot: caterer price if set, otherwise the global setting.
        var pricePerLunch = await settingsRepository.GetPricePerLunchAsync(cancellationToken);

        Caterer? caterer = null;
        if (command.CatererId.HasValue)
        {
            caterer = await catererRepository.GetByIdAsync(command.CatererId.Value, cancellationToken);
            if (caterer == null)
            {
                return Error.NotFound(
                    code: StatusCodes.Status404NotFound.ToString(),
                    description: "caterer not found.");
            }

            if (caterer.PricePerLunch > 0)
            {
                pricePerLunch = caterer.PricePerLunch;
            }
        }

        // Cutoff: local wall time in the configured timezone, stored as UTC.
        var timeZone = await settingsRepository.GetTimeZoneAsync(cancellationToken);
        DateTime cutoffLocal;

        if (!string.IsNullOrWhiteSpace(command.CutoffAt))
        {
            if (TimeSpan.TryParse(command.CutoffAt, CultureInfo.InvariantCulture, out var cutoffTime))
            {
                cutoffLocal = lunchDate.Add(cutoffTime);
            }
            else if (DateTime.TryParse(command.CutoffAt, CultureInfo.InvariantCulture, DateTimeStyles.None, out var cutoffDateTime))
            {
                cutoffLocal = cutoffDateTime;
            }
            else
            {
                return Error.Validation(
                    code: StatusCodes.Status400BadRequest.ToString(),
                    description: "cutoff_at is invalid. Use 'HH:mm' or an ISO datetime.");
            }
        }
        else
        {
            var defaultCutoff = await settingsRepository.GetDefaultCutoffTimeAsync(cancellationToken);
            cutoffLocal = lunchDate.Add(defaultCutoff);
        }

        var cutoffUtc = TimeZoneHelper.ToUtc(cutoffLocal, timeZone);

        // Resolve options.
        var menuItemIds = command.Options
            .Where(o => o.MenuItemId.HasValue)
            .Select(o => o.MenuItemId!.Value)
            .Distinct()
            .ToList();

        var menuItems = await menuItemRepository.GetByIdsAsync(menuItemIds, cancellationToken);
        var menuItemsById = menuItems.ToDictionary(m => m.Id);

        var pollOptions = new List<PollOption>();
        var sortOrder = 0;

        foreach (var option in command.Options)
        {
            string name;
            int? menuItemId = null;

            if (option.MenuItemId.HasValue)
            {
                if (!menuItemsById.TryGetValue(option.MenuItemId.Value, out var menuItem))
                {
                    return Error.NotFound(
                        code: StatusCodes.Status404NotFound.ToString(),
                        description: $"menu item {option.MenuItemId.Value} not found.");
                }

                menuItemId = menuItem.Id;
                name = menuItem.Name;
            }
            else
            {
                name = option.CustomName!.Trim();
            }

            pollOptions.Add(new PollOption(menuItemId, name, sortOrder++));
        }

        var question = string.IsNullOrWhiteSpace(command.Question)
            ? $"Who's in for lunch on {lunchDate:dd MMM yyyy}?"
            : command.Question.Trim();

        var poll = new Poll(lunchDate, caterer?.Id, pricePerLunch, cutoffUtc, question, command.UserId)
        {
            Options = pollOptions,
            Caterer = caterer
        };

        await pollRepository.AddAsync(poll, cancellationToken);

        await notificationService.CreateForAllActiveUsersAsync(
            title: "Lunch poll published",
            body: $"{question} Vote before the cutoff.",
            type: NotificationType.PollPublished,
            refId: poll.Id,
            cancellationToken: cancellationToken);

        return PollResponseFactory.From(poll, [], null);
    }
}
