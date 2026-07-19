using System.Linq;
using FoodPower.Contracts.Requests.Payments;
using FoodPower.Features.Payments.PaymentHandlers;
using Mapster;

namespace FoodPower.Presentation.Mappings;

public class PaymentMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreatePaymentRequest, CreatePaymentCommand>()
            .ConstructUsing(src => new CreatePaymentCommand(
                src.screenshot,
                src.note,
                src.payment_method,
                src.allocations.Select(a => new PaymentAllocationInput(a.beneficiary_user_id, a.days)).ToList(),
                0
            ));
    }
}
