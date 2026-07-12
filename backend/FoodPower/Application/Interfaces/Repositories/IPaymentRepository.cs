using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Models;
using FoodPower.Domain.Entities;
using FoodPower.Domain.Enums;

namespace FoodPower.Application.Interfaces.Repositories;

public interface IPaymentRepository : IEfRepository<Payment>
{
    Task<Payment?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Payment>> GetBySubmitterAsync(int userId, CancellationToken cancellationToken = default);
    Task<PaginatedList<Payment>> GetPagedAsync(PaymentStatus? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}
