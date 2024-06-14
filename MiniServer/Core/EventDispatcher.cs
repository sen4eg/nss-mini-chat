using System.Collections.Concurrent;
using MiniServer.Events;

namespace MiniServer.Core {
    public class EventDispatcher {
        private readonly BlockingCollection<Func<Task>> _eventQueue = new BlockingCollection<Func<Task>>();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly Thread[] _threads;

        public EventDispatcher(int threadCount = 4) {
            _threads = new Thread[threadCount];
            for (int i = 0; i < threadCount; i++) {
                _threads[i] = new Thread(DispatchLoop);
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
                }
                catch (OperationCanceledException) {
                    // Thread exiting
                }
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
    }
    
}