namespace ProjectManagementSaaS.Application.Common.Models;

public sealed record BulkDeleteRequest(IReadOnlyCollection<Guid> Ids);
