using System.Linq;
using FoodPower.Contracts.Requests.Polls;
using FoodPower.Features.Polls.PollHandlers;
using Mapster;

namespace FoodPower.Presentation.Mappings;

public class PollMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreatePollRequest, CreatePollCommand>()
            .ConstructUsing(src => new CreatePollCommand(
                src.lunch_date,
                src.caterer_id,
                src.question,
                src.cutoff_at,
                src.poll_type,
                src.options.Select(o => new PollOptionInput(o.menu_item_id, o.custom_name)).ToList(),
                0
            ));
    }
}
