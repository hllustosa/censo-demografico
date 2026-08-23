using Census.Statistics.Api.Hubs;
using Census.Statistics.Application;
using Microsoft.AspNetCore.SignalR;

namespace Census.Statistics.Api.Services
{
    public class SignalRNotificationSender : INotificationSender
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotificationSender(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public Task NotifyAll() => _hubContext.Clients.All.SendAsync("Notify");
    }
}
