namespace ProjectManagementSaaS.Application.Common.Models;

public sealed record PagedRequest(
    int PageNumber = 1,
    int PageSize = 25,
    string? Search = null,
    string? SortBy = null,
    string? SortDirection = "asc");
