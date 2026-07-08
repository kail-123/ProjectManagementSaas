namespace ProjectManagementSaaS.Application.Common.Models;

public sealed record PagedResult<TItem>(
    IReadOnlyCollection<TItem> Items,
    int PageNumber,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}
