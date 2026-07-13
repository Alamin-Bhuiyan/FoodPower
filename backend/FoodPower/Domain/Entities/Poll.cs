using System;
using System.Collections.Generic;
using FoodPower.Domain.Enums;

namespace FoodPower.Domain.Entities;

public class Poll
{
    public int Id { get; set; }
    public DateTime LunchDate { get; set; }
    public int? CatererId { get; set; }
    public decimal PricePerLunch { get; set; }
    public DateTime CutoffAt { get; set; }
    public PollType Type { get; set; } = PollType.Lunch;
    public PollStatus Status { get; set; } = PollStatus.Open;
    public Guid ShareToken { get; set; } = Guid.NewGuid();
    public string Question { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedById { get; set; }

    public Caterer? Caterer { get; set; }
    public List<PollOption> Options { get; set; } = [];

    public Poll()
    {
    }

    public Poll(
        DateTime lunchDate,
        int? catererId,
        decimal pricePerLunch,
        DateTime cutoffAtUtc,
        string question,
        int createdById,
        PollType type = PollType.Lunch)
    {
        LunchDate = lunchDate.Date;
        CatererId = catererId;
        PricePerLunch = pricePerLunch;
        CutoffAt = cutoffAtUtc;
        Type = type;
        Question = question;
        CreatedById = createdById;
        Status = PollStatus.Open;
        ShareToken = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }

    public bool IsCutoffPassed => DateTime.UtcNow >= CutoffAt;
}
