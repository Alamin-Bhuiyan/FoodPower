using FoodPower.Application.Models;
using FoodPower.BuildingBlocks.Constants;
using Microsoft.AspNetCore.Http;

namespace FoodPower.BuildingBlocks.Extensions;

public static class HttpResponseExtension
{
    public static void AddPaginationHeaders<T>(this HttpResponse response, PaginatedList<T> paginatedList)
    {
        response.Headers.Append(HttpHeaders.XTotalCount, paginatedList.TotalCount.ToString());
        response.Headers.Append(HttpHeaders.XTotalPages, paginatedList.TotalPages.ToString());
        response.Headers.Append(HttpHeaders.XCurrentPage, paginatedList.PageNumber.ToString());
        response.Headers.Append(HttpHeaders.XHasPreviousPage, paginatedList.HasPreviousPage.ToString());
        response.Headers.Append(HttpHeaders.XHasNextPage, paginatedList.HasNextPage.ToString());
    }
}
