using System;
using System.Collections.Generic;
using System.Linq;
using Census.Shared.Bus.Domain;
using Census.Shared.Bus.Interfaces;

namespace Census.Shared.Bus.Implementation
{
    public class RabbitMQSubscriptionManager : IEventBusSubscriptionsManager
    {
        private readonly Dictionary<string, List<SubscriptionInfo>> Handlers;
        private readonly List<Type> EventTypes;

        public event EventHandler<string> OnEventRemoved;

        public RabbitMQSubscriptionManager()
        {
            Handlers = new Dictionary<string, List<SubscriptionInfo>>();
            EventTypes = new List<Type>();
        }

        public bool IsEmpty => !Handlers.Keys.Any();
        public void Clear() => Handlers.Clear();

        public void AddSubscription<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();

            DoAddSubscription(typeof(TH), eventName, isDynamic: false);

            if (!EventTypes.Contains(typeof(T)))
            {
                EventTypes.Add(typeof(T));
            }
        }

        private void DoAddSubscription(Type handlerType, string eventName, bool isDynamic)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                Handlers.Add(eventName, new List<SubscriptionInfo>());
            }

            if (Handlers[eventName].Any(s => s.HandlerType == handlerType))
            {
                throw new ArgumentException(
                    $"Handler do tipo {handlerType.Name} já registrado para o evento '{eventName}'", nameof(handlerType));
            }

            Handlers[eventName].Add(SubscriptionInfo.Typed(handlerType));
        }

        public void RemoveSubscription<T, TH>()
            where TH : IIntegrationEventHandler<T>
            where T : IntegrationEvent
        {
            var handlerToRemove = FindSubscriptionToRemove<T, TH>();
            var eventName = GetEventKey<T>();
            DoRemoveHandler(eventName, handlerToRemove);
        }


        private void DoRemoveHandler(string eventName, SubscriptionInfo subsToRemove)
        {
            if (subsToRemove != null)
            {
                Handlers[eventName].Remove(subsToRemove);
                if (!Handlers[eventName].Any())
                {
                    Handlers.Remove(eventName);
                    var eventType = EventTypes.SingleOrDefault(e => e.Name == eventName);
                    if (eventType != null)
                    {
                        EventTypes.Remove(eventType);
                    }
                    RaiseOnEventRemoved(eventName);
                }

            }
        }

        public IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();
            return GetHandlersForEvent(key);
        }
        public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName) => Handlers[eventName];

        private void RaiseOnEventRemoved(string eventName)
        {
            var handler = OnEventRemoved;
            handler?.Invoke(this, eventName);
        }

        private SubscriptionInfo FindSubscriptionToRemove<T, TH>()
             where T : IntegrationEvent
             where TH : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();
            return DoFindSubscriptionToRemove(eventName, typeof(TH));
        }

        private SubscriptionInfo DoFindSubscriptionToRemove(string eventName, Type handlerType)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                return null;
            }

            return Handlers[eventName].SingleOrDefault(s => s.HandlerType == handlerType);

        }

        public bool HasSubscriptionsForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();
            return HasSubscriptionsForEvent(key);
        }
        public bool HasSubscriptionsForEvent(string eventName) => Handlers.ContainsKey(eventName);

        public Type GetEventTypeByName(string eventName) => EventTypes.SingleOrDefault(t => t.Name == eventName);

        public string GetEventKey<T>()
        {
            return typeof(T).Name;
        }
    }
}
