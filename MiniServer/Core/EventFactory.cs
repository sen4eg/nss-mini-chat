using System.Collections.Concurrent;
using System.Reflection;
using MiniServer.Core.Events;

namespace MiniServer.Core;

using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

public interface ICommEventFactory
{
    TEvent Create<TEvent, TEventResponse>(object request, Action action)
        where TEvent : EventBase<TEventResponse>
        where TEventResponse : class;
}

public class CommEventFactory : ICommEventFactory
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ConcurrentDictionary<Type, EventConstructorInfo> _constructorCache = new();

    public CommEventFactory(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public TEvent Create<TEvent, TEventResponse>(object request, Action action)
        where TEvent : EventBase<TEventResponse>
        where TEventResponse : class
    {
        var eventType = typeof(TEvent);
        var eventConstructorInfo = _constructorCache.GetOrAdd(eventType, type =>
        {
            var constructor = type.GetConstructors().FirstOrDefault();
            if (constructor == null)
            {
                throw new InvalidOperationException($"No constructor found for the event type {type.Name}.");
            }

            var parameterTypes = constructor.GetParameters().Select(p => p.ParameterType).ToArray();
            return new EventConstructorInfo(constructor, parameterTypes);
        });

        var parameters = eventConstructorInfo.ParameterTypes;
        var args = new object[parameters.Length];

        // First argument is the request object
        args[0] = request;

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            // Resolve the remaining arguments from the DI container within the scope
            for (var i = 1; i < parameters.Length - 1; i++)
            {
                var parameterType = parameters[i];
                var service = scope.ServiceProvider.GetService(parameterType);
                args[i] = service ?? throw new InvalidOperationException($"Service for type {parameterType.FullName} not registered.");
            }

            // Last argument is the action
            args[parameters.Length - 1] = action;

            // Create the event instance within the scope
            return (TEvent)eventConstructorInfo.Constructor.Invoke(args)
                   ?? throw new InvalidOperationException("Failed to create instance of the event.");
        }
    }
}

public class EventConstructorInfo
{
    public ConstructorInfo Constructor { get; }
    public Type[] ParameterTypes { get; }

    public EventConstructorInfo(ConstructorInfo constructor, Type[] parameterTypes)
    {
        Constructor = constructor;
        ParameterTypes = parameterTypes;
    }
}
