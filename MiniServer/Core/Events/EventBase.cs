namespace MiniServer.Core.Events;

// public abstract class AbstractEvent {
//     public abstract Task Execute(TaskCompletionSource<object> taskCompletionSource);
// }

public abstract class EventBase<T> 
{
    private readonly Action _sideEffect;

    protected EventBase(Action sideEffect)
    {
        _sideEffect = sideEffect;
    }

    protected abstract Task<T> ExecuteAsync();

    public async Task Execute(TaskCompletionSource<T> taskCompletionSource)
    {
        try
        {
            _sideEffect?.Invoke();

            // Execute asynchronously and set the result or handle exceptions
            await ExecuteAsync().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    taskCompletionSource.SetException(task.Exception);
                }
                else if (task.IsCanceled)
                {
                    taskCompletionSource.SetCanceled();
                }
                else
                {
                    taskCompletionSource.SetResult(task.Result);
                }
            });
        }
        catch (Exception e)
        {
            taskCompletionSource.SetException(e);
        }
    }
    
}