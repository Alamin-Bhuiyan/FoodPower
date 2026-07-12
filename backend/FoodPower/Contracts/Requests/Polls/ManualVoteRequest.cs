namespace FoodPower.Contracts.Requests.Polls;

public class ManualVoteRequest
{
    public int user_id { get; set; }
    public int poll_option_id { get; set; }
}
