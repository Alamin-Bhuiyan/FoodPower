using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Application.Interfaces.Services;
using FoodPower.BuildingBlocks.Constants;
using FoodPower.BuildingBlocks.Extensions;
using FoodPower.Contracts.Responses.Payments;
using FoodPower.Data;
using FoodPower.Domain.Entities;
using FoodPower.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FoodPower.Features.Payments.PaymentHandlers;

public record PaymentAllocationInput(int BeneficiaryUserId, int Days);

public record CreatePaymentCommand(
    string? Screenshot,
    string? Note,
    string? PaymentMethod,
    List<PaymentAllocationInput> Allocations,
    int UserId
) : IRequest<ErrorOr<PaymentResponse>>;

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    private static readonly string[] AllowedTypes = ["png", "jpeg", "webp"];

    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.PaymentMethod)
            .Must(value => PaymentMethodParser.TryParse(value, out _))
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("payment_method is invalid. Use 'cash', 'bank_transfer' or 'bkash'.");

        // Screenshot is mandatory proof for bKash / bank transfer, optional for cash.
        When(x => RequiresScreenshot(x.PaymentMethod), () =>
        {
            RuleFor(x => x.Screenshot)
                .NotEmpty()
                .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
                .WithMessage("screenshot is required for bkash and bank_transfer payments.");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Screenshot), () =>
        {
            RuleFor(x => x.Screenshot)
                .Must(value => Base64FileValidator.IsValidBase64File(value!, AllowedTypes))
                .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
                .WithMessage("screenshot must be a valid base64 encoded png, jpeg or webp image.");
        });

        RuleFor(x => x.Allocations)
            .NotEmpty()
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("at least one allocation is required.");

        RuleForEach(x => x.Allocations).ChildRules(allocation =>
        {
            allocation.RuleFor(a => a.BeneficiaryUserId)
                .GreaterThan(0)
                .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
                .WithMessage("beneficiary_user_id is required.");

            allocation.RuleFor(a => a.Days)
                .GreaterThan(0)
                .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
                .WithMessage("days must be greater than 0.");
        });
    }

    private static bool RequiresScreenshot(string? paymentMethod)
        => !PaymentMethodParser.TryParse(paymentMethod, out var method) || method != PaymentMethod.Cash;
}

/// <summary>
/// Parses snake_case wire values ("cash", "bank_transfer", "bkash") into
/// <see cref="PaymentMethod"/>. Null/blank falls back to bKash so stale PWA
/// clients that predate payment methods keep working.
/// </summary>
public static class PaymentMethodParser
{
    public static bool TryParse(string? value, out PaymentMethod method)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            method = PaymentMethod.Bkash;
            return true;
        }

        var normalized = value.Replace("_", string.Empty).Trim();
        return Enum.TryParse(normalized, ignoreCase: true, out method) && Enum.IsDefined(method);
    }

    public static string ToWire(PaymentMethod method) => method switch
    {
        PaymentMethod.Cash => "cash",
        PaymentMethod.BankTransfer => "bank_transfer",
        _ => "bkash"
    };
}

public class CreatePaymentCommandHandler(
    IPaymentRepository paymentRepository,
    ISettingsRepository settingsRepository,
    IFileService fileService,
    INotificationService notificationService,
    IPushService pushService,
    ApplicationDbContext dbContext)
    : IRequestHandler<CreatePaymentCommand, ErrorOr<PaymentResponse>>
{
    public async Task<ErrorOr<PaymentResponse>> Handle(
        CreatePaymentCommand command, CancellationToken cancellationToken)
    {
        var beneficiaryIds = command.Allocations
            .Select(a => a.BeneficiaryUserId)
            .Distinct()
            .ToList();

        if (beneficiaryIds.Count != command.Allocations.Count)
        {
            return Error.Validation(
                code: StatusCodes.Status400BadRequest.ToString(),
                description: "duplicate beneficiaries in allocations.");
        }

        var existingUserCount = await dbContext.Users
            .CountAsync(u => beneficiaryIds.Contains(u.Id), cancellationToken);

        if (existingUserCount != beneficiaryIds.Count)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "one or more beneficiaries were not found.");
        }

        // Amount is computed server-side: days x current price per lunch.
        var pricePerLunch = await settingsRepository.GetPricePerLunchAsync(cancellationToken);

        PaymentMethodParser.TryParse(command.PaymentMethod, out var paymentMethod);

        // Screenshot is optional for cash payments; validated as required otherwise.
        var screenshotPath = string.IsNullOrWhiteSpace(command.Screenshot)
            ? string.Empty
            : await fileService.SaveBase64FileAsync(command.Screenshot, "screenshots", cancellationToken);

        var allocations = command.Allocations
            .Select(a => new PaymentAllocation(a.BeneficiaryUserId, a.Days, a.Days * pricePerLunch))
            .ToList();

        var payment = new Payment(
            command.UserId,
            allocations.Sum(a => a.Amount),
            screenshotPath,
            command.Note,
            paymentMethod)
        {
            Allocations = allocations
        };

        await paymentRepository.AddAsync(payment, cancellationToken);

        // Notify every admin that a payment is waiting for approval (best-effort).
        try
        {
            var submitterName = await dbContext.Users
                .Where(u => u.Id == command.UserId)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync(cancellationToken) ?? "A user";

            var adminRoleId = await dbContext.Roles
                .Where(r => r.Name == PermissionRole.Admin)
                .Select(r => r.Id)
                .FirstOrDefaultAsync(cancellationToken);

            var adminIds = await dbContext.UserRoles
                .Where(ur => ur.RoleId == adminRoleId && ur.UserId != command.UserId)
                .Select(ur => ur.UserId)
                .ToListAsync(cancellationToken);

            var amountText = payment.TotalAmount.ToString("0.##", CultureInfo.InvariantCulture);
            foreach (var adminId in adminIds)
            {
                await notificationService.CreateForUserAsync(
                    userId: adminId,
                    title: "Payment awaiting approval",
                    body: $"{submitterName} submitted a payment of {amountText} BDT for approval.",
                    type: NotificationType.PaymentSubmitted,
                    refId: payment.Id,
                    cancellationToken: cancellationToken);
            }

            // Best-effort browser push to every admin (excluding the submitter).
            // Delivery is fire-and-forget inside the push service.
            await pushService.SendToUsersAsync(
                adminIds,
                "Payment awaiting approval",
                $"{submitterName} submitted {amountText} BDT",
                "/payments",
                cancellationToken);
        }
        catch
        {
            // Notification failure must not block the payment submission.
        }

        var created = await paymentRepository.GetByIdWithDetailsAsync(payment.Id, cancellationToken);

        return PaymentResponseFactory.From(created ?? payment);
    }
}
