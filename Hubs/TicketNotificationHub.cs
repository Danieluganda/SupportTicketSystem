using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace SupportTicketSystem.Hubs
{
    public class TicketNotificationHub : Hub
    {
        // This method will be called from the client to send a message
        public async Task SendNotification(string message)
        {
            try
            {
                await Clients.All.SendAsync("ReceiveNotification", message);
            }
            catch (Exception ex)
            {
                // Handle exception (optional, logging can be done here)
                Console.WriteLine($"Error sending notification: {ex.Message}");
            }
        }

        // Optionally, send a notification to a specific user
        public async Task SendNotificationToUser(string userId, string message)
        {
            try
            {
                await Clients.User(userId).SendAsync("ReceiveNotification", message);
            }
            catch (Exception ex)
            {
                // Handle exception (optional, logging can be done here)
                Console.WriteLine($"Error sending notification to user {userId}: {ex.Message}");
            }
        }

        // Send notification to a group (example: group of agents)
        public async Task SendNotificationToGroup(string groupName, string message)
        {
            try
            {
                await Clients.Group(groupName).SendAsync("ReceiveNotification", message);
            }
            catch (Exception ex)
            {
                // Handle exception (optional, logging can be done here)
                Console.WriteLine($"Error sending notification to group {groupName}: {ex.Message}");
            }
        }

        // This method can be called from the client to add a user to a specific group (e.g., "agents" group)
        public async Task AddToGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        // This method can be called from the client to remove a user from a group
        public async Task RemoveFromGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
