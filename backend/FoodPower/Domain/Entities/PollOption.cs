namespace FoodPower.Domain.Entities;

public class PollOption
{
    public int Id { get; set; }
    public int PollId { get; set; }
    public int? MenuItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public Poll? Poll { get; set; }
    public MenuItem? MenuItem { get; set; }

    public PollOption()
    {
    }

    public PollOption(int? menuItemId, string name, int sortOrder)
    {
        MenuItemId = menuItemId;
        Name = name;
        SortOrder = sortOrder;
    }
}
