using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bloxstrap.Helpers
{
    // https://gist.github.com/dfederm/35c729f6218834b764fa04c219181e4e
    public sealed class AsyncMutex : IAsyncDisposable
    {
        private readonly string _name;
        private Task? _mutexTask;
        private ManualResetEventSlim? _releaseEvent;
        private CancellationTokenSource? _cancellationTokenSource;

        public AsyncMutex(string name)
        {
            _name = name;
        }

        public Task AcquireAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            TaskCompletionSource taskCompletionSource = new();

            _releaseEvent = new ManualResetEventSlim();
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Putting all mutex manipulation in its own task as it doesn't work in async contexts
            // Note: this task should not throw.
            _mutexTask = Task.Factory.StartNew(
                state =>
                {
                    try
                    {
                        CancellationToken cancellationToken = _cancellationTokenSource.Token;
                        using var mutex = new Mutex(false, _name);
                        try
                        {
                            // Wait for either the mutex to be acquired, or cancellation
                            if (WaitHandle.WaitAny(new[] { mutex, cancellationToken.WaitHandle }) != 0)
                            {
                                taskCompletionSource.SetCanceled(cancellationToken);
                                return;
                            }
                        }
                        catch (AbandonedMutexException)
                        {
                            // Abandoned by another process, we acquired it.
                        }

                        taskCompletionSource.SetResult();

                        // Wait until the release call
                        _releaseEvent.Wait();

                        mutex.ReleaseMutex();
                    }
                    catch (OperationCanceledException)
                    {
                        taskCompletionSource.TrySetCanceled(cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        taskCompletionSource.TrySetException(ex);
                    }
                },
                state: null,
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            return taskCompletionSource.Task;
        }

        public async Task ReleaseAsync()
        {
            _releaseEvent?.Set();

            if (_mutexTask != null)
            {
                await _mutexTask;
            }
        }

        public async ValueTask DisposeAsync()
        {
            // Ensure the mutex task stops waiting for any acquire
            _cancellationTokenSource?.Cancel();

            // Ensure the mutex is released
            await ReleaseAsync();

            _releaseEvent?.Dispose();
            _cancellationTokenSource?.Dispose();
        }
    }
}
