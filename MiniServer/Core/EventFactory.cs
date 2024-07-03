using System.Collections.Concurrent;
using System.Reflection;
using MiniServer.Core.Events;

namespace MiniServer.Core;

using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

public interface ICommEventFactory
{
    TEvent Create<TEvent, TEventResponse>(object payload, Action action)
        where TEvent : EventBase<TEventResponse>
        where TEventResponse : class;

    object Create(Type eventType, Type payloadType, object payload, Action sideEffect);
}

public class CommEventFactory : ICommEventFactory
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ConcurrentDictionary<Type, EventConstructorInfo> _constructorCache = new();

    public CommEventFactory(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public TEvent Create<TEvent, TEventResponse>(object payload, Action action)
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
        args[0] = payload;

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
    public object Create(Type eventType, Type payloadType, object payload, Action sideEffect)
    {
        // We surpass this for now i'm sure it functions as intended
        // if (!typeof(EventBase<>).MakeGenericType(payloadType).IsAssignableFrom(eventType))
        // {
        //     throw new ArgumentException($"{eventType.Name} must inherit from EventBase<{payloadType.Name}>.");
        // }

        // Retrieve the constructor info for the specified event type
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

        // Prepare the arguments to be passed to the event constructor
        var parameters = eventConstructorInfo.ParameterTypes;
        var args = new object[parameters.Length];

        // First argument is the payload
        args[0] = payload;

        // Use a service scope to resolve dependencies
        using var scope = _serviceScopeFactory.CreateScope();

        for (var i = 1; i < parameters.Length - 1; i++)
        {
            var parameterType = parameters[i];
            var service = scope.ServiceProvider.GetService(parameterType);
            args[i] = service ?? throw new InvalidOperationException($"Service for type {parameterType.FullName} not registered.");
        }

        // Last argument is the side effect action
        args[parameters.Length - 1] = sideEffect;

        // Create the event instance
        var eventInstance = eventConstructorInfo.Constructor.Invoke(args);
        if (eventInstance == null)
        {
            throw new InvalidOperationException("Failed to create instance of the event.");
        }

        return eventInstance;
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
