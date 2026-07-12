namespace FoodPower.Contracts.Responses.Settings;

public record SettingsResponse(
    string price_per_lunch,
    string default_cutoff_time,
    string time_zone,
    string bkash_number,
    string bank_account);
