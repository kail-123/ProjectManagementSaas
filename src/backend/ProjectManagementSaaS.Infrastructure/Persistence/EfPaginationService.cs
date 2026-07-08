using Microsoft.EntityFrameworkCore;
using ProjectManagementSaaS.Application.Abstractions.Persistence;
using ProjectManagementSaaS.Application.Common.Models;

namespace ProjectManagementSaaS.Infrastructure.Persistence;

internal sealed class EfPaginationService : IPaginationService
{
    public async Task<PagedResult<TItem>> CreateAsync<TItem>(
        IQueryable<TItem> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var safePageNumber = Math.Max(1, pageNumber);
        var safePageSize = Math.Clamp(pageSize, 1, 200);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((safePageNumber - 1) * safePageSize)
            .Take(safePageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TItem>(items, safePageNumber, safePageSize, totalCount);
    }

    public Task<TItem?> SingleOrDefaultAsync<TItem>(IQueryable<TItem> query, CancellationToken cancellationToken)
    {
        return query.SingleOrDefaultAsync(cancellationToken);
    }

    public Task<List<TItem>> ToListAsync<TItem>(IQueryable<TItem> query, CancellationToken cancellationToken)
    {
        return query.ToListAsync(cancellationToken);
    }
}
