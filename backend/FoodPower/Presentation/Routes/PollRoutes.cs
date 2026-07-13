namespace FoodPower.Presentation.Routes;

public class PollRoutes
{
    public const string CreatePollMethod = "POST";
    public const string CreatePollName = "foodpower.polls.create_poll";
    public const string CreatePollTemplate = "/api/polls";

    public const string GetActivePollMethod = "GET";
    public const string GetActivePollName = "foodpower.polls.active";
    public const string GetActivePollTemplate = "/api/polls/active";

    public const string GetActiveGeneralPollsMethod = "GET";
    public const string GetActiveGeneralPollsName = "foodpower.polls.get_active_general_polls";
    public const string GetActiveGeneralPollsTemplate = "/api/polls/general-active";

    public const string GetSharedPollMethod = "GET";
    public const string GetSharedPollName = "foodpower.polls.shared";
    public const string GetSharedPollTemplate = "/api/polls/shared/{shareToken}";

    public const string GetPollsMethod = "GET";
    public const string GetPollsName = "foodpower.polls.list";
    public const string GetPollsTemplate = "/api/polls";

    public const string GetPollResultsMethod = "GET";
    public const string GetPollResultsName = "foodpower.polls.results";
    public const string GetPollResultsTemplate = "/api/polls/{id}/results";

    public const string CastVoteMethod = "POST";
    public const string CastVoteName = "foodpower.polls.cast_vote";
    public const string CastVoteTemplate = "/api/polls/{id}/votes";

    public const string RemoveVoteMethod = "DELETE";
    public const string RemoveVoteName = "foodpower.polls.remove_vote";
    public const string RemoveVoteTemplate = "/api/polls/{id}/votes";

    public const string ManualVoteMethod = "POST";
    public const string ManualVoteName = "foodpower.polls.manual_vote";
    public const string ManualVoteTemplate = "/api/polls/{id}/manual-votes";

    public const string ClosePollMethod = "POST";
    public const string ClosePollName = "foodpower.polls.close";
    public const string ClosePollTemplate = "/api/polls/{id}/close";

    public const string SendPollEmailsMethod = "POST";
    public const string SendPollEmailsName = "foodpower.polls.send_poll_emails";
    public const string SendPollEmailsTemplate = "/api/polls/{id}/send-emails";
}
