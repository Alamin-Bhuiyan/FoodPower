using System;

namespace FoodPower.Domain.Entities;

public class MenuItem
{
    public int Id { get; set; }
    public int CatererId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public Caterer? Caterer { get; set; }

    public MenuItem()
    {
    }

    public MenuItem(int catererId, DayOfWeek dayOfWeek, string name, string? description)
    {
        CatererId = catererId;
        DayOfWeek = dayOfWeek;
        Name = name;
        Description = description;
        IsActive = true;
    }
}
