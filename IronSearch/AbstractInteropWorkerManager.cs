using Il2CppInterop.Runtime;
using System.Threading;

namespace IronSearch
{
    public abstract class AbstractInteropWorkerManager<T, TState>
    {
        private static int _globalIdCounter = 0;
        private readonly int _id = _globalIdCounter++;

        private readonly Thread[] _threads;
        protected readonly int _workerCount;

        private volatile bool _disposed;

        private int _stop;
        private int _nextIndex;

        private T[]? _data;
        private int _count;

        private int _generation; // 🔥 key fix

        private readonly ManualResetEventSlim _startEvent = new(false);
        private readonly CountdownEvent _doneEvent;

        protected AbstractInteropWorkerManager(int workerCount)
        {
            _workerCount = workerCount;
            _threads = new Thread[workerCount];
            _doneEvent = new CountdownEvent(workerCount);

            for (int i = 0; i < workerCount; i++)
            {
                int workerIndex = i;

                _threads[i] = new Thread(() => WorkerLoop(workerIndex))
                {
                    IsBackground = true,
                    Name = $"InteropWorker_{_id}_{i}"
                };

                _threads[i].Start();
            }
        }

        protected abstract TState? ProcessItem(T item, TState? state);
        protected virtual TState? OnWorkerStart() => default;
        protected virtual TState? OnWorkerIteration(TState? state) => state;
        protected virtual void OnWorkerEnd(TState? state) { }

        private void WorkerLoop(int workerIndex)
        {
            nint threadPtr = 0;
            TState? state = default;
            int lastSeenGeneration = -1;

            try
            {
                threadPtr = IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());
                state = OnWorkerStart();

                while (true)
                {
                    _startEvent.Wait();

                    if (_disposed)
                        return;

                    int currentGen = Volatile.Read(ref _generation);

                    // 🔥 prevent double execution
                    if (currentGen == lastSeenGeneration)
                        continue;

                    lastSeenGeneration = currentGen;

                    var data = Volatile.Read(ref _data)!;
                    int count = Volatile.Read(ref _count);

                    while (true)
                    {
                        if (Volatile.Read(ref _stop) != 0)
                            break;

                        int index = Interlocked.Increment(ref _nextIndex) - 1;

                        if (index >= count)
                            break;

                        state = ProcessItem(data[index], state);
                    }

                    state = OnWorkerIteration(state);

                    if (_doneEvent.Signal())
                    {
                        _startEvent.Reset();
                    }
                }
            }
            finally
            {
                OnWorkerEnd(state);

                if (threadPtr != 0)
                    IL2CPP.il2cpp_thread_detach(threadPtr);
            }
        }

        public void Execute(T[] data, int count)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AbstractInteropWorkerManager<T, TState>));

            _data = data;
            _count = count;

            _stop = 0;
            _nextIndex = 0;

            _doneEvent.Reset(_workerCount);

            // 🔥 advance generation
            Interlocked.Increment(ref _generation);

            _startEvent.Set();

            _doneEvent.Wait();
        }

        public void Stop()
        {
            Volatile.Write(ref _stop, 1);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _startEvent.Set();

            foreach (var t in _threads)
            {
                if (!t.Join(2000))
                {
                    try { t.Interrupt(); } catch { }
                }
            }

            _startEvent.Dispose();
            _doneEvent.Dispose();
        }
    }
}