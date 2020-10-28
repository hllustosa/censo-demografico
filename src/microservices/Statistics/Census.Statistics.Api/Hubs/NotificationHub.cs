using Census.Statistics.Application;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Census.Statistics.Api.Hubs
{
    public class NotificationHub : Hub, INotificationSender
    {
        IHubContext<NotificationHub> Hub;

        public NotificationHub(IHubContext<NotificationHub> notificationHub)
        {
            Hub = notificationHub;
        }

        public async Task NotifyAll()
        {
            await Hub.Clients.All.SendAsync("Notify");
        }
    }
}
