using System.Threading.Tasks;

namespace InternPortalAPI.Hubs;

public class KanbanHub : Microsoft.AspNetCore.SignalR.Hub
{
    public async Task JoinInternBoard(string internId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, internId);
    }

    public async Task LeaveInternBoard(string internId)
    {
       await Groups.RemoveFromGroupAsync(Context.ConnectionId, internId);
    }
}