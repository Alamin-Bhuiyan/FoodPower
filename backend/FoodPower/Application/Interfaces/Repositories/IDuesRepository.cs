using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FoodPower.Contracts.Responses.Dues;

namespace FoodPower.Application.Interfaces.Repositories;

public interface IDuesRepository
{
    Task<MyDuesResponse> GetMyDuesAsync(int userId, CancellationToken cancellationToken = default);
    Task<List<UserDueResponse>> GetAllDuesAsync(CancellationToken cancellationToken = default);
    Task<WeeklySummaryResponse> GetWeeklySummaryAsync(DateTime weekStart, CancellationToken cancellationToken = default);
}
