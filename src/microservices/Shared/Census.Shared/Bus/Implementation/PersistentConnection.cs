using Census.Shared.Bus.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System;
using System.IO;
using System.Net.Sockets;

namespace Census.Shared.Bus.Implementation
{
    public class PersistentConnection : IPersistentConnection
    {
        private readonly string LOST_CONNECTION_MSG = "Conexão com RabbitMQ interompida. Tentando reconectar...";

        private readonly IConnectionFactory ConnectionFactory;
        private readonly ILogger<PersistentConnection> Logger;
        private readonly int RetryCount;
        readonly object sync_root = new object();

        IConnection Connection;
        bool Disposed;
        
        public PersistentConnection(IConnectionFactory connectionFactory, 
            ILogger<PersistentConnection> logger, 
            int retryCount = 5)
        {
            ConnectionFactory = connectionFactory;
            Logger = logger;
            RetryCount = retryCount;
        }

        public bool IsConnected
        {
            get
            {
                return Connection != null && Connection.IsOpen && !Disposed;
            }
        }

        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Conexões ao RabbitMQ não disponível");
            }

            return Connection.CreateModel();
        }

        public void Dispose()
        {
            if (Disposed) return;

            Disposed = true;

            try
            {
                Connection.Dispose();
            }
            catch (IOException ex)
            {
                Logger.LogCritical(ex.ToString());
            }
        }

        public bool TryConnect()
        {
            Logger.LogInformation("Tentando conectar ao RabbitMQ");

            lock (sync_root)
            {
                var policy = RetryPolicy.Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(RetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {
                        Logger.LogWarning(ex, "Não foi possível conectar ao RabbitMQ após {TimeOut}s ({ExceptionMessage})", $"{time.TotalSeconds:n1}", ex.Message);
                    }
                );

                policy.Execute(() =>
                {
                    Connection = ConnectionFactory
                          .CreateConnection();
                });

                if (IsConnected)
                {
                    Connection.ConnectionShutdown += OnConnectionShutdown;
                    Connection.CallbackException += OnCallbackException;
                    Connection.ConnectionBlocked += OnConnectionBlocked;

                    Logger.LogInformation("Client RabbitMQ estabeleceu conexão com '{HostName}'", Connection.Endpoint.HostName);

                    return true;
                }
                else
                {
                    Logger.LogCritical("ERRO FATAL: Não foi possível conectar ao RabbitMQ");

                    return false;
                }
            }
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (Disposed) return;

            Logger.LogWarning(LOST_CONNECTION_MSG);

            TryConnect();
        }

        void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (Disposed) return;

            Logger.LogWarning(LOST_CONNECTION_MSG);

            TryConnect();
        }

        void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (Disposed) return;

            Logger.LogWarning(LOST_CONNECTION_MSG);

            TryConnect();
        }
    }
}
