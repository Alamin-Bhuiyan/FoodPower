using System.Linq;
using FoodPower.Contracts.Responses.Payments;
using FoodPower.Domain.Entities;

namespace FoodPower.Features.Payments.PaymentHandlers;

public static class PaymentResponseFactory
{
    public static PaymentResponse From(Payment payment) =>
        new(
            id: payment.Id,
            submitted_by_id: payment.SubmittedById,
            submitted_by_name: payment.SubmittedBy?.FullName,
            total_amount: payment.TotalAmount,
            screenshot_path: payment.ScreenshotPath,
            note: payment.Note,
            payment_method: PaymentMethodParser.ToWire(payment.Method),
            status: payment.Status.ToString(),
            reviewed_by_id: payment.ReviewedById,
            reviewed_by_name: payment.ReviewedBy?.FullName,
            reviewed_at: payment.ReviewedAt,
            created_at: payment.CreatedAt,
            allocations: payment.Allocations
                .Select(a => new PaymentAllocationResponse(
                    a.Id,
                    a.BeneficiaryUserId,
                    a.Beneficiary?.FullName,
                    a.Days,
                    a.Amount))
                .ToList());
}
