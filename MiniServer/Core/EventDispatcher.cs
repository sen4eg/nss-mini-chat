using System.Collections.Concurrent;
using MiniServer.Core.Events;

namespace MiniServer.Core {
    public class EventDispatcher {
        private readonly ILogger<EventDispatcher> _logger;
        private readonly BlockingCollection<Func<Task>> _eventQueue = new BlockingCollection<Func<Task>>();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Thread[] _threads;

        public int GetLoad() {
            return _eventQueue.Count;
        }
        public EventDispatcher(ILogger<EventDispatcher> logger, int threadCount = 8) {
            _logger = logger;
            _threads = new Thread[threadCount];
            for (int i = 0; i < threadCount; i++) {
                _threads[i] = new Thread(DispatchLoop) {
                    Name = $"EventDispatcherThread-{i}"
                };
                // _threads[i].Start();
            }
        }

        public void EnqueueEvent(Func<Task> eventTask) {
            _eventQueue.Add(eventTask);
        }

        private void DispatchLoop() {
            while (!_cancellationTokenSource.Token.IsCancellationRequested) {
                try {
                    var eventTask = _eventQueue.Take(_cancellationTokenSource.Token);
                    eventTask.Invoke().Wait(); // Wait for event execution
                    continue;
                }
                catch (OperationCanceledException) {
                    // Thread exiting
                }
                catch (InvalidOperationException) {
                    // Thrown by Take() when _eventQueue is marked as complete for adding
                    // No need to handle this specifically unless necessary
                }
                catch (Exception ex) {
                    // Handle any other unexpected exceptions
                    _logger.LogError($"Exception in DispatchLoop: {ex}");
                }
        
                Thread.Sleep(100); 
            }
        }

        public void Start() {
            // Start the dispatcher threads
            foreach (var thread in _threads) {
                thread.Start();
            }
        }

        public void Stop() {
            // Stop the dispatcher threads
            _cancellationTokenSource.Cancel();
            foreach (var thread in _threads) {
                thread.Join();
            }
        }

        public void Fire<T>(EventBase<T> @event, TaskCompletionSource<T> tcs) {
            EnqueueEvent(async () => await @event.Execute(tcs));
        }
    }
    
}