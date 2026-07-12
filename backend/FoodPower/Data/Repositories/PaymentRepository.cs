using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoodPower.Application.Interfaces.Repositories;
using FoodPower.Application.Models;
using FoodPower.Domain.Entities;
using FoodPower.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FoodPower.Data.Repositories;

public class PaymentRepository(ApplicationDbContext dbContext)
    : EfRepository<Payment>(dbContext), IPaymentRepository
{
    public async Task<Payment?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
        => await DbContext.Payments
            .Include(p => p.SubmittedBy)
            .Include(p => p.ReviewedBy)
            .Include(p => p.Allocations)
            .ThenInclude(a => a.Beneficiary)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<List<Payment>> GetBySubmitterAsync(int userId, CancellationToken cancellationToken = default)
        => await DbContext.Payments
            .Include(p => p.SubmittedBy)
            .Include(p => p.ReviewedBy)
            .Include(p => p.Allocations)
            .ThenInclude(a => a.Beneficiary)
            .Where(p => p.SubmittedById == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<PaginatedList<Payment>> GetPagedAsync(PaymentStatus? status, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = DbContext.Payments
            .Include(p => p.SubmittedBy)
            .Include(p => p.ReviewedBy)
            .Include(p => p.Allocations)
            .ThenInclude(a => a.Beneficiary)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        query = query.OrderByDescending(p => p.CreatedAt);

        return await PaginatedListAsync(pageNumber, pageSize, cancellationToken, query);
    }
}
