using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Application.Interfaces.Services;
using FoodPower.Contracts.Responses;
using FoodPower.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FoodPower.Features.Payments.PaymentHandlers;

public record CancelPaymentCommand(
    int PaymentId,
    int UserId
) : IRequest<ErrorOr<MessageResponse>>;

public class CancelPaymentCommandValidator : AbstractValidator<CancelPaymentCommand>
{
    public CancelPaymentCommandValidator()
    {
        RuleFor(x => x.PaymentId)
            .GreaterThan(0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("payment id is invalid.");
    }
}

/// <summary>
/// Lets the submitter cancel their own payment while it is still pending
/// approval. Approved and rejected payments are part of the dues ledger and
/// stay immutable — the user resubmits instead of editing.
/// </summary>
public class CancelPaymentCommandHandler(
    IPaymentRepository paymentRepository,
    IFileService fileService)
    : IRequestHandler<CancelPaymentCommand, ErrorOr<MessageResponse>>
{
    public async Task<ErrorOr<MessageResponse>> Handle(
        CancelPaymentCommand command, CancellationToken cancellationToken)
    {
        var payment = await paymentRepository.GetByIdWithDetailsAsync(command.PaymentId, cancellationToken);
        if (payment == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "payment not found.");
        }

        if (payment.SubmittedById != command.UserId)
        {
            return Error.Forbidden(
                code: StatusCodes.Status403Forbidden.ToString(),
                description: "you can only cancel your own payment.");
        }

        if (payment.Status != PaymentStatus.Pending)
        {
            return Error.Validation(
                code: StatusCodes.Status400BadRequest.ToString(),
                description: "this payment has already been reviewed and can no longer be cancelled.");
        }

        var screenshotPath = payment.ScreenshotPath;

        // Allocations are removed by cascade delete.
        await paymentRepository.DeleteAsync(payment, cancellationToken);

        // Best-effort cleanup; a leftover file must not fail the cancellation.
        try
        {
            fileService.DeleteFile(screenshotPath);
        }
        catch
        {
            // ignored
        }

        return new MessageResponse("Payment cancelled. You can submit a new one anytime.");
    }
}
