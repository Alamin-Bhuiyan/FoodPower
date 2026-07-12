using System.Collections.Generic;

namespace FoodPower.Domain.Entities;

public class Caterer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public decimal PricePerLunch { get; set; }
    public bool IsActive { get; set; } = true;

    public List<MenuItem> MenuItems { get; set; } = [];

    public Caterer()
    {
    }

    public Caterer(string name, string? phone, decimal pricePerLunch)
    {
        Name = name;
        Phone = phone;
        PricePerLunch = pricePerLunch;
        IsActive = true;
    }
}
