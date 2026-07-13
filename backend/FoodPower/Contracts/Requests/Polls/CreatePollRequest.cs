using System.Collections.Generic;

namespace FoodPower.Contracts.Requests.Polls;

public class CreatePollOptionRequest
{
    public int? menu_item_id { get; set; }
    public string? custom_name { get; set; }
}

public class CreatePollRequest
{
    public string lunch_date { get; set; } = string.Empty;
    public int? caterer_id { get; set; }
    public string? question { get; set; }
    public string? cutoff_at { get; set; }
    public string? poll_type { get; set; }
    public List<CreatePollOptionRequest> options { get; set; } = [];
}
