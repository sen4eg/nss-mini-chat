using System.Collections.Concurrent;

namespace MiniServer;

public class EventDispatcher
{
    private readonly BlockingCollection<Action> _eventQueue = new BlockingCollection<Action>();
    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
    private readonly Thread[] _threads;

    public EventDispatcher(int threadCount = 4)
    {
        _threads = new Thread[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            _threads[i] = new Thread(DispatchLoop);
            _threads[i].Start();
        }
    }

    public void EnqueueEvent(Action action)
    {
        _eventQueue.Add(action);
    }

    private void DispatchLoop()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                Action action = _eventQueue.Take(_cancellationTokenSource.Token);
                action?.Invoke();
            }
            catch (OperationCanceledException)
            {
                // Thread exiting
            }
        }
    }

    public void Start()
    {
        // Start the dispatcher threads
        foreach (var thread in _threads)
        {
            thread.Start();
        }
    }

    public void Stop()
    {
        // Stop the dispatcher threads
        _cancellationTokenSource.Cancel();
        foreach (var thread in _threads)
        {
            thread.Join();
        }
    }
}
