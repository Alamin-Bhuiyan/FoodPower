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
    string Screenshot,
    string? Note,
    List<PaymentAllocationInput> Allocations,
    int UserId
) : IRequest<ErrorOr<PaymentResponse>>;

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    private static readonly string[] AllowedTypes = ["png", "jpeg", "webp"];

    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.Screenshot)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("screenshot is required.")
            .Must(value => Base64FileValidator.IsValidBase64File(value, AllowedTypes))
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("screenshot must be a valid base64 encoded png, jpeg or webp image.");

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
}

public class CreatePaymentCommandHandler(
    IPaymentRepository paymentRepository,
    ISettingsRepository settingsRepository,
    IFileService fileService,
    INotificationService notificationService,
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

        var screenshotPath = await fileService.SaveBase64FileAsync(command.Screenshot, "screenshots", cancellationToken);

        var allocations = command.Allocations
            .Select(a => new PaymentAllocation(a.BeneficiaryUserId, a.Days, a.Days * pricePerLunch))
            .ToList();

        var payment = new Payment(
            command.UserId,
            allocations.Sum(a => a.Amount),
            screenshotPath,
            command.Note)
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
        }
        catch
        {
            // Notification failure must not block the payment submission.
        }

        var created = await paymentRepository.GetByIdWithDetailsAsync(payment.Id, cancellationToken);

        return PaymentResponseFactory.From(created ?? payment);
    }
}
