using System.Threading.Tasks;

namespace Census.Statistics.Application
{
    public interface INotificationSender 
    {
        Task NotifyAll();
    }
}
