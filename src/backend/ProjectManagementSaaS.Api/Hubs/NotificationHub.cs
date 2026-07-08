using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ProjectManagementSaaS.Application.Common.Security;

namespace ProjectManagementSaaS.Api.Hubs;

[Authorize(Policy = ApplicationPermissions.NotificationsView)]
public sealed class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue(ApplicationClaimTypes.UserId);

        if (Guid.TryParse(userId, out var parsedUserId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetUserGroup(parsedUserId));
        }

        await base.OnConnectedAsync();
    }

    public static string GetUserGroup(Guid userId) => $"user:{userId}";
}
