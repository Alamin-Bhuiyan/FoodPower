using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Contracts.Responses.Payments;
using MediatR;

namespace FoodPower.Features.Payments.PaymentHandlers;

public record GetMyPaymentsQuery(int UserId) : IRequest<ErrorOr<List<PaymentResponse>>>;

public class GetMyPaymentsQueryHandler(IPaymentRepository paymentRepository)
    : IRequestHandler<GetMyPaymentsQuery, ErrorOr<List<PaymentResponse>>>
{
    public async Task<ErrorOr<List<PaymentResponse>>> Handle(
        GetMyPaymentsQuery request, CancellationToken cancellationToken)
    {
        var payments = await paymentRepository.GetBySubmitterAsync(request.UserId, cancellationToken);

        return payments.Select(PaymentResponseFactory.From).ToList();
    }
}
