using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Application.Interfaces.Services;
using FoodPower.Contracts.Responses.Payments;
using FoodPower.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodPower.Features.Payments.PaymentHandlers;

public record ApprovePaymentCommand(
    int PaymentId,
    int AdminUserId
) : IRequest<ErrorOr<PaymentResponse>>;

public class ApprovePaymentCommandValidator : AbstractValidator<ApprovePaymentCommand>
{
    public ApprovePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .GreaterThan(0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("payment id is invalid.");
    }
}

public class ApprovePaymentCommandHandler(
    IPaymentRepository paymentRepository,
    INotificationService notificationService)
    : IRequestHandler<ApprovePaymentCommand, ErrorOr<PaymentResponse>>
{
    public async Task<ErrorOr<PaymentResponse>> Handle(
        ApprovePaymentCommand command, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByIdWithDetailsAsync(command.PaymentId, cancellationToken);
        if (payment == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "payment not found.");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            return Error.Conflict(
                code: StatusCodes.Status409Conflict.ToString(),
                description: $"payment is already {payment.Status.ToString().ToLowerInvariant()}.");
        }

        payment.Status = PaymentStatus.Approved;
        payment.ReviewedById = command.AdminUserId;
        payment.ReviewedAt = DateTime.UtcNow;

        await paymentRepository.UpdateAsync(payment, cancellationToken);

        await notificationService.CreateForUserAsync(
            userId: payment.SubmittedById,
            title: "Payment approved",
            body: $"Your payment of {payment.TotalAmount.ToString("0.##", CultureInfo.InvariantCulture)} BDT has been approved.",
            type: NotificationType.PaymentApproved,
            refId: payment.Id,
            cancellationToken: cancellationToken);

        return PaymentResponseFactory.From(payment);
    }
}
