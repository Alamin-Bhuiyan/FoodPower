using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ErrorOr;
using FluentValidation;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Application.Interfaces.Services;
using FoodPower.Contracts.Responses;
using FoodPower.Data;
using FoodPower.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FoodPower.Features.Polls.PollHandlers;

public record AnnounceArrivalCommand(int PollId) : IRequest<ErrorOr<MessageResponse>>;

public class AnnounceArrivalCommandValidator : AbstractValidator<AnnounceArrivalCommand>
{
    public AnnounceArrivalCommandValidator()
    {
        RuleFor(x => x.PollId)
            .GreaterThan(0)
            .WithErrorCode(StatusCodes.Status400BadRequest.ToString())
            .WithMessage("poll id is invalid.");
    }
}

public class AnnounceArrivalCommandHandler(
    IPollRepository pollRepository,
    IPushService pushService,
    INotificationService notificationService,
    ApplicationDbContext dbContext)
    : IRequestHandler<AnnounceArrivalCommand, ErrorOr<MessageResponse>>
{
    public async Task<ErrorOr<MessageResponse>> Handle(
        AnnounceArrivalCommand command, CancellationToken cancellationToken)
    {
        var poll = await pollRepository.GetByIdAsync(command.PollId, cancellationToken);
        if (poll == null)
        {
            return Error.NotFound(
                code: StatusCodes.Status404NotFound.ToString(),
                description: "poll not found.");
        }

        // Notify the people who ordered lunch that day (the voters on this poll).
        var voterIds = await dbContext.Votes
            .Where(v => v.PollId == command.PollId)
            .Select(v => v.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (voterIds.Count == 0)
        {
            return new MessageResponse("No one voted on this poll, so there is nobody to notify.");
        }

        var lunchDateText = poll.LunchDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
        const string title = "Lunch has arrived 🍱";
        var body = $"Today's lunch ({lunchDateText}) is here — please collect it.";

        foreach (var userId in voterIds)
        {
            await notificationService.CreateForUserAsync(
                userId: userId,
                title: title,
                body: body,
                type: NotificationType.LunchArrived,
                refId: poll.Id,
                cancellationToken: cancellationToken);
        }

        await pushService.SendToUsersAsync(voterIds, title, body, "/", cancellationToken);

        return new MessageResponse($"Notified {voterIds.Count} people that lunch has arrived.");
    }
}
