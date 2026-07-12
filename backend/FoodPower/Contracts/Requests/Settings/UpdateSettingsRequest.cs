namespace FoodPower.Contracts.Requests.Settings;

public class UpdateSettingsRequest
{
    public string? price_per_lunch { get; set; }
    public string? default_cutoff_time { get; set; }
    public string? time_zone { get; set; }
    public string? bkash_number { get; set; }
    public string? bank_account { get; set; }
}
