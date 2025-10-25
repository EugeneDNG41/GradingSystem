using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace GradingSystem.Gateway.Api.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }

            var role = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
            if (!string.IsNullOrEmpty(role))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"role_{role}");
            }

            await base.OnConnectedAsync();
        }

        public async Task SendNotificationToUser(string userId, string message)
        {
            await Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", message);
        }

        public async Task SendNotificationToRole(string role, string message)
        {
            await Clients.Group($"role_{role}").SendAsync("ReceiveNotification", message);
        }

        public async Task NotifySubmissionUploaded(int submissionId, string studentName)
        {
            await Clients.Group("role_Manager").SendAsync("SubmissionUploaded", new
            {
                submissionId,
                studentName,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyGradingComplete(int submissionId, int examinerId)
        {
            await Clients.Group($"user_{examinerId}").SendAsync("GradingComplete", new
            {
                submissionId,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task NotifyViolationDetected(int submissionId, string violationType)
        {
            await Clients.Group("role_Moderator").SendAsync("ViolationDetected", new
            {
                submissionId,
                violationType,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
