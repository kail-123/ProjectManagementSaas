using ProjectManagementSaaS.Application.Common.Models;

namespace ProjectManagementSaaS.Application.Abstractions.Persistence;

public interface IPaginationService
{
    Task<PagedResult<TItem>> CreateAsync<TItem>(
        IQueryable<TItem> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task<TItem?> SingleOrDefaultAsync<TItem>(
        IQueryable<TItem> query,
        CancellationToken cancellationToken);

    Task<List<TItem>> ToListAsync<TItem>(
        IQueryable<TItem> query,
        CancellationToken cancellationToken);
}
