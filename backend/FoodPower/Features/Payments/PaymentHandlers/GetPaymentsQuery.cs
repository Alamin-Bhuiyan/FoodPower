using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Application.Models;
using FoodPower.Contracts.Responses.Payments;
using FoodPower.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodPower.Features.Payments.PaymentHandlers;

public record GetPaymentsQuery(
    PaymentStatus? Status,
    int PageNumber,
    int PageSize
) : IRequest<ErrorOr<PaginatedList<PaymentResponse>>>;

public class GetPaymentsQueryValidator : AbstractValidator<GetPaymentsQuery>
{
    public GetPaymentsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .When(x => x.PageNumber != 0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("page_number is invalid!");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .When(x => x.PageSize != 0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("page_size is invalid!");
    }
}

public class GetPaymentsQueryHandler(IPaymentRepository paymentRepository)
    : IRequestHandler<GetPaymentsQuery, ErrorOr<PaginatedList<PaymentResponse>>>
{
    public async Task<ErrorOr<PaginatedList<PaymentResponse>>> Handle(
        GetPaymentsQuery request, CancellationToken cancellationToken)
    {
        var payments = await paymentRepository.GetPagedAsync(
            request.Status, request.PageNumber, request.PageSize, cancellationToken);

        var responses = payments.Items.Select(PaymentResponseFactory.From).ToList();

        return new PaginatedList<PaymentResponse>(
            responses, payments.TotalCount, payments.PageNumber, request.PageSize <= 0 ? responses.Count : request.PageSize);
    }
}
