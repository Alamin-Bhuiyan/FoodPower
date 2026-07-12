using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.BuildingBlocks.Utilities;
using FoodPower.Contracts.Responses.Settings;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodPower.Features.Settings.SettingsHandlers;

public record UpdateSettingsCommand(
    string? PricePerLunch,
    string? DefaultCutoffTime,
    string? TimeZone,
    string? BkashNumber,
    string? BankAccount
) : IRequest<ErrorOr<SettingsResponse>>;

public class UpdateSettingsCommandValidator : AbstractValidator<UpdateSettingsCommand>
{
    public UpdateSettingsCommandValidator()
    {
        RuleFor(x => x.PricePerLunch)
            .Must(value => decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var price) && price > 0)
            .When(x => !string.IsNullOrWhiteSpace(x.PricePerLunch))
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("price_per_lunch must be a number greater than 0.");

        RuleFor(x => x.DefaultCutoffTime)
            .Must(value => TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.DefaultCutoffTime))
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("default_cutoff_time must be in HH:mm format.");
    }
}

public class UpdateSettingsCommandHandler(ISettingsRepository settingsRepository)
    : IRequestHandler<UpdateSettingsCommand, ErrorOr<SettingsResponse>>
{
    public async Task<ErrorOr<SettingsResponse>> Handle(
        UpdateSettingsCommand command, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(command.PricePerLunch))
        {
            var price = decimal.Parse(command.PricePerLunch, NumberStyles.Number, CultureInfo.InvariantCulture);
            await settingsRepository.UpsertAsync(
                SettingKeys.PricePerLunch,
                price.ToString(CultureInfo.InvariantCulture),
                cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(command.DefaultCutoffTime))
        {
            var cutoff = TimeSpan.Parse(command.DefaultCutoffTime, CultureInfo.InvariantCulture);
            await settingsRepository.UpsertAsync(
                SettingKeys.DefaultCutoffTime,
                cutoff.ToString(@"hh\:mm", CultureInfo.InvariantCulture),
                cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(command.TimeZone))
        {
            var timeZone = TimeZoneHelper.Resolve(command.TimeZone);
            if (timeZone.Equals(TimeZoneInfo.Utc) && !string.Equals(command.TimeZone.Trim(), "UTC", StringComparison.OrdinalIgnoreCase))
            {
                return Error.Validation(
                    code: StatusCodes.Status400BadRequest.ToString(),
                    description: "time_zone is not a valid IANA or Windows timezone id.");
            }

            await settingsRepository.UpsertAsync(SettingKeys.TimeZone, command.TimeZone.Trim(), cancellationToken);
        }

        // Payment details are plain strings; an explicit empty string clears them.
        if (command.BkashNumber != null)
        {
            await settingsRepository.UpsertAsync(SettingKeys.BkashNumber, command.BkashNumber.Trim(), cancellationToken);
        }

        if (command.BankAccount != null)
        {
            await settingsRepository.UpsertAsync(SettingKeys.BankAccount, command.BankAccount.Trim(), cancellationToken);
        }

        var settings = await settingsRepository.GetAllAsync(cancellationToken);

        var priceValue = settings.GetValueOrDefault(SettingKeys.PricePerLunch, SettingKeys.DefaultPricePerLunch);
        if (!decimal.TryParse(priceValue, NumberStyles.Number, CultureInfo.InvariantCulture, out _))
        {
            priceValue = SettingKeys.DefaultPricePerLunch;
        }

        return new SettingsResponse(
            price_per_lunch: priceValue,
            default_cutoff_time: settings.GetValueOrDefault(SettingKeys.DefaultCutoffTime, SettingKeys.DefaultCutoffTimeValue),
            time_zone: settings.GetValueOrDefault(SettingKeys.TimeZone, SettingKeys.DefaultTimeZone),
            bkash_number: settings.GetValueOrDefault(SettingKeys.BkashNumber, SettingKeys.DefaultBkashNumber),
            bank_account: settings.GetValueOrDefault(SettingKeys.BankAccount, SettingKeys.DefaultBankAccount));
    }
}
