using System;

namespace FoodPower.Domain.Entities;

public class Vote
{
    public int Id { get; set; }
    public int PollId { get; set; }
    public int PollOptionId { get; set; }
    public int UserId { get; set; }
    public bool IsManual { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int CreatedById { get; set; }

    public Poll? Poll { get; set; }
    public PollOption? Option { get; set; }
    public AppUser? User { get; set; }

    public Vote()
    {
    }

    public Vote(int pollId, int pollOptionId, int userId, bool isManual, int createdById)
    {
        PollId = pollId;
        PollOptionId = pollOptionId;
        UserId = userId;
        IsManual = isManual;
        CreatedById = createdById;
        CreatedAt = DateTime.UtcNow;
    }
}
