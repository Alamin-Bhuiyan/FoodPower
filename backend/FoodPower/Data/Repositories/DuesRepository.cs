using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Contracts.Responses.Dues;
using FoodPower.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FoodPower.Data.Repositories;

public class DuesRepository(ApplicationDbContext dbContext) : IDuesRepository
{
    public async Task<MyDuesResponse> GetMyDuesAsync(int userId, CancellationToken cancellationToken = default)
    {
        var lunches = await dbContext.Votes
            .Where(v => v.UserId == userId && v.Poll!.Type == PollType.Lunch)
            .Select(v => new
            {
                v.Poll!.LunchDate,
                v.Poll.PricePerLunch,
                OptionName = v.Option!.Name,
                v.IsManual
            })
            .ToListAsync(cancellationToken);

        var payments = await dbContext.PaymentAllocations
            .Where(a => a.BeneficiaryUserId == userId && a.Payment!.Status == PaymentStatus.Approved)
            .Select(a => new
            {
                Date = a.Payment!.ReviewedAt ?? a.Payment.CreatedAt,
                a.Amount,
                a.Days,
                SubmitterName = a.Payment.SubmittedBy!.FullName
            })
            .ToListAsync(cancellationToken);

        var totalConsumed = lunches.Sum(l => l.PricePerLunch);
        var totalPaid = payments.Sum(p => p.Amount);

        var history = new List<DueHistoryItemResponse>();

        history.AddRange(lunches.Select(l => new DueHistoryItemResponse(
            l.LunchDate,
            "lunch",
            l.IsManual ? $"{l.OptionName} (added by admin)" : l.OptionName,
            -l.PricePerLunch)));

        history.AddRange(payments.Select(p => new DueHistoryItemResponse(
            p.Date,
            "payment",
            $"Payment approved ({p.Days} day(s), submitted by {p.SubmitterName})",
            p.Amount)));

        return new MyDuesResponse(
            balance: totalPaid - totalConsumed,
            lunch_count: lunches.Count,
            total_paid: totalPaid,
            total_consumed: totalConsumed,
            history: history.OrderByDescending(h => h.date).ToList());
    }

    public async Task<List<UserDueResponse>> GetAllDuesAsync(CancellationToken cancellationToken = default)
    {
        var users = await dbContext.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.FullName)
            .Select(u => new { u.Id, u.FullName, u.Email })
            .ToListAsync(cancellationToken);

        var lunchTotals = await dbContext.Votes
            .Where(v => v.Poll!.Type == PollType.Lunch)
            .GroupBy(v => v.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                Count = g.Count(),
                Amount = g.Sum(v => v.Poll!.PricePerLunch)
            })
            .ToListAsync(cancellationToken);

        var paidTotals = await dbContext.PaymentAllocations
            .Where(a => a.Payment!.Status == PaymentStatus.Approved)
            .GroupBy(a => a.BeneficiaryUserId)
            .Select(g => new { UserId = g.Key, Amount = g.Sum(a => a.Amount) })
            .ToListAsync(cancellationToken);

        var lunchByUser = lunchTotals.ToDictionary(l => l.UserId);
        var paidByUser = paidTotals.ToDictionary(p => p.UserId, p => p.Amount);

        return users.Select(u =>
        {
            var lunches = lunchByUser.TryGetValue(u.Id, out var l) ? l.Count : 0;
            var consumed = lunchByUser.TryGetValue(u.Id, out var lc) ? lc.Amount : 0m;
            var paid = paidByUser.GetValueOrDefault(u.Id, 0m);

            return new UserDueResponse(u.Id, u.FullName, u.Email, lunches, paid, paid - consumed);
        }).ToList();
    }

    public async Task<WeeklySummaryResponse> GetWeeklySummaryAsync(DateTime weekStart, CancellationToken cancellationToken = default)
    {
        // weekStart is the Monday of the target week. The range covers the five
        // lunch days Monday-Friday: from Monday 00:00 up to but excluding the
        // following Saturday 00:00. LunchDate is a calendar date, so this
        // includes Mon/Tue/Wed/Thu/Fri.
        var start = weekStart.Date;
        var end = start.AddDays(5);

        var rows = await dbContext.Votes
            .Where(v => v.Poll!.Type == PollType.Lunch && v.Poll.LunchDate >= start && v.Poll.LunchDate < end)
            .Select(v => new
            {
                v.UserId,
                UserName = v.User!.FullName,
                v.Poll!.PricePerLunch
            })
            .ToListAsync(cancellationToken);

        var userIds = rows.Select(r => r.UserId).Distinct().ToList();

        var consumedTotals = await dbContext.Votes
            .Where(v => userIds.Contains(v.UserId) && v.Poll!.Type == PollType.Lunch)
            .GroupBy(v => v.UserId)
            .Select(g => new { UserId = g.Key, Amount = g.Sum(v => v.Poll!.PricePerLunch) })
            .ToDictionaryAsync(x => x.UserId, x => x.Amount, cancellationToken);

        var paidTotals = await dbContext.PaymentAllocations
            .Where(a => userIds.Contains(a.BeneficiaryUserId) && a.Payment!.Status == PaymentStatus.Approved)
            .GroupBy(a => a.BeneficiaryUserId)
            .Select(g => new { UserId = g.Key, Amount = g.Sum(a => a.Amount) })
            .ToDictionaryAsync(x => x.UserId, x => x.Amount, cancellationToken);

        var summaryRows = rows
            .GroupBy(r => new { r.UserId, r.UserName })
            .Select(g => new WeeklyUserSummaryResponse(
                g.Key.UserId,
                g.Key.UserName,
                g.Count(),
                g.Sum(r => r.PricePerLunch),
                paidTotals.GetValueOrDefault(g.Key.UserId, 0m)
                    - consumedTotals.GetValueOrDefault(g.Key.UserId, 0m) >= 0))
            .OrderBy(u => u.full_name)
            .ToList();

        return new WeeklySummaryResponse(
            week_start: start,
            week_end: end.AddDays(-1),
            total_lunches: summaryRows.Sum(u => u.lunch_count),
            total_amount: summaryRows.Sum(u => u.amount),
            rows: summaryRows);
    }
}
