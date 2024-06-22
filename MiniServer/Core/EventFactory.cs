using System.Collections.Concurrent;
using System.Reflection;
using MiniServer.Core.Events;

namespace MiniServer.Core;

public interface ICommEventFactory {
    TEvent Create<TEvent, TEventResponse>(object request, Action action)
        where TEvent : EventBase<TEventResponse>
        where TEventResponse : class;
}
public class CommEventFactory : ICommEventFactory {
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Type, EventConstructorInfo> _constructorCache = new();

    public CommEventFactory(IServiceProvider serviceProvider) {
        _serviceProvider = serviceProvider;
    }

    public TEvent Create<TEvent, TEventResponse>(object request, Action action)
        where TEvent : EventBase<TEventResponse>
        where TEventResponse : class {
        
        var eventType = typeof(TEvent);
        var eventConstructorInfo = _constructorCache.GetOrAdd(eventType, type => {
            var constructor = type.GetConstructors().FirstOrDefault();
            if (constructor == null) {
                throw new InvalidOperationException($"No constructor found for the event type {type.Name}.");
            }

            var parameterTypes = constructor.GetParameters().Select(p => p.ParameterType).ToArray();
            return new EventConstructorInfo(constructor, parameterTypes);
        });

        var parameters = eventConstructorInfo.ParameterTypes;
        var args = new object[parameters.Length];

        // First argument is the request object
        args[0] = request;

        // Resolve the remaining arguments from the DI container
        for (var i = 1; i < parameters.Length - 1; i++) {
            var parameterType = parameters[i];
            var service = _serviceProvider.GetService(parameterType);
            args[i] = service ?? throw new InvalidOperationException($"Service for type {parameterType.FullName} not registered.");
        }

        // Last argument is the action
        args[parameters.Length - 1] = action;

        return (TEvent)eventConstructorInfo.Constructor.Invoke(args)
               ?? throw new InvalidOperationException("Failed to create instance of the event.");
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