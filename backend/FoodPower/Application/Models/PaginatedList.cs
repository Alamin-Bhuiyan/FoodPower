using System;
using System.Collections.Generic;

namespace FoodPower.Application.Models;

public class PaginatedList<T>(List<T> items, int count, int pageNumber, int pageSize) : List<T>
{
    public List<T> Items { get; } = items;

    public int PageNumber { get; } = pageNumber;

    public int TotalPages { get; } = (count <= 0 || pageSize <= 0) ? 0 : (int)Math.Ceiling(count / (double)pageSize);

    public int TotalCount { get; } = count;

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage => PageNumber < TotalPages;
}
