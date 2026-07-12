using Microsoft.AspNetCore.Mvc;

namespace FoodPower.Contracts.Requests;

public class PaginatorRequest
{
    [FromQuery(Name = "page")]
    public int page_number { get; set; }

    [FromQuery(Name = "pageSize")]
    public int page_size { get; set; }
}
