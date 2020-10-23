using RabbitMQ.Client;

namespace Census.Shared.Bus.Interfaces
{
    public interface IPersistentConnection
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();
    }
}
