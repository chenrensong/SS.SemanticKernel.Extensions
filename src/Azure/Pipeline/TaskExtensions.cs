// Azure.AI.OpenAI, Version=1.0.0.0, Culture=neutral, PublicKeyToken=92742159e12e44c8
// Azure.Core.Pipeline.TaskExtensions
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core.Pipeline;

internal static class TaskExtensions
{
    public struct Enumerable<T> : IEnumerable<T>, IEnumerable
    {
        private readonly IAsyncEnumerable<T> _asyncEnumerable;

        public Enumerable(IAsyncEnumerable<T> asyncEnumerable)
        {
            _asyncEnumerable = asyncEnumerable;
        }

        public Enumerator<T> GetEnumerator()
        {
            return new Enumerator<T>(_asyncEnumerable.GetAsyncEnumerator());
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new Enumerator<T>(_asyncEnumerable.GetAsyncEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public struct Enumerator<T> : IEnumerator<T>, IEnumerator, IDisposable
    {
        private readonly IAsyncEnumerator<T> _asyncEnumerator;

        public T Current => _asyncEnumerator.Current;

        object IEnumerator.Current => Current;

        public Enumerator(IAsyncEnumerator<T> asyncEnumerator)
        {
            _asyncEnumerator = asyncEnumerator;
        }

        public bool MoveNext()
        {
            return _asyncEnumerator.MoveNextAsync().EnsureCompleted();
        }

        public void Reset()
        {
            throw new NotSupportedException($"{GetType()} is a synchronous wrapper for {_asyncEnumerator.GetType()} async enumerator, which can't be reset, so IEnumerable.Reset() calls aren't supported.");
        }

        public void Dispose()
        {
            ((IAsyncDisposable)_asyncEnumerator).DisposeAsync().EnsureCompleted();
        }
    }

    public struct WithCancellationTaskAwaitable
    {
        private readonly CancellationToken _cancellationToken;

        private readonly ConfiguredTaskAwaitable _awaitable;

        public WithCancellationTaskAwaitable(Task task, CancellationToken cancellationToken)
        {
            _awaitable = task.ConfigureAwait(false);
            _cancellationToken = cancellationToken;
        }

        public WithCancellationTaskAwaiter GetAwaiter()
        {
            return new WithCancellationTaskAwaiter(_awaitable.GetAwaiter(), _cancellationToken);
        }
    }

    public struct WithCancellationTaskAwaitable<T>
    {
        private readonly CancellationToken _cancellationToken;

        private readonly ConfiguredTaskAwaitable<T> _awaitable;

        public WithCancellationTaskAwaitable(Task<T> task, CancellationToken cancellationToken)
        {
            _awaitable = task.ConfigureAwait(false);
            _cancellationToken = cancellationToken;
        }

        public WithCancellationTaskAwaiter<T> GetAwaiter()
        {
            return new WithCancellationTaskAwaiter<T>(_awaitable.GetAwaiter(), _cancellationToken);
        }
    }

    public struct WithCancellationValueTaskAwaitable<T>
    {
        private readonly CancellationToken _cancellationToken;

        private readonly ConfiguredValueTaskAwaitable<T> _awaitable;

        public WithCancellationValueTaskAwaitable(ValueTask<T> task, CancellationToken cancellationToken)
        {
            _awaitable = task.ConfigureAwait(false);
            _cancellationToken = cancellationToken;
        }

        public WithCancellationValueTaskAwaiter<T> GetAwaiter()
        {
            return new WithCancellationValueTaskAwaiter<T>(_awaitable.GetAwaiter(), _cancellationToken);
        }
    }

    public struct WithCancellationTaskAwaiter : ICriticalNotifyCompletion, INotifyCompletion
    {
        private readonly CancellationToken _cancellationToken;

        private readonly ConfiguredTaskAwaitable.ConfiguredTaskAwaiter _taskAwaiter;

        public bool IsCompleted
        {
            get
            {
                if (!_taskAwaiter.IsCompleted)
                {
                    return _cancellationToken.IsCancellationRequested;
                }
                return true;
            }
        }

        public WithCancellationTaskAwaiter(ConfiguredTaskAwaitable.ConfiguredTaskAwaiter awaiter, CancellationToken cancellationToken)
        {
            _taskAwaiter = awaiter;
            _cancellationToken = cancellationToken;
        }

        public void OnCompleted(Action continuation)
        {
            _taskAwaiter.OnCompleted(WrapContinuation(ref continuation));
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            _taskAwaiter.UnsafeOnCompleted(WrapContinuation(ref continuation));
        }

        public void GetResult()
        {
            if (!_taskAwaiter.IsCompleted)
            {
                _cancellationToken.ThrowIfCancellationRequested();
            }
            _taskAwaiter.GetResult();
        }

        private Action WrapContinuation([In] ref Action originalContinuation)
        {
            if (!_cancellationToken.CanBeCanceled)
            {
                return originalContinuation;
            }
            return new WithCancellationContinuationWrapper(originalContinuation, _cancellationToken).Continuation;
        }
    }

    public struct WithCancellationTaskAwaiter<T> : ICriticalNotifyCompletion, INotifyCompletion
    {
        private readonly CancellationToken _cancellationToken;

        private readonly ConfiguredTaskAwaitable<T>.ConfiguredTaskAwaiter _taskAwaiter;

        public bool IsCompleted
        {
            get
            {
                if (!_taskAwaiter.IsCompleted)
                {
                    return _cancellationToken.IsCancellationRequested;
                }
                return true;
            }
        }

        public WithCancellationTaskAwaiter(ConfiguredTaskAwaitable<T>.ConfiguredTaskAwaiter awaiter, CancellationToken cancellationToken)
        {
            _taskAwaiter = awaiter;
            _cancellationToken = cancellationToken;
        }

        public void OnCompleted(Action continuation)
        {
            _taskAwaiter.OnCompleted(WrapContinuation(ref continuation));
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            _taskAwaiter.UnsafeOnCompleted(WrapContinuation(ref continuation));
        }

        public T GetResult()
        {
            if (!_taskAwaiter.IsCompleted)
            {
                _cancellationToken.ThrowIfCancellationRequested();
            }
            return _taskAwaiter.GetResult();
        }

        private Action WrapContinuation([In] ref Action originalContinuation)
        {
            if (!_cancellationToken.CanBeCanceled)
            {
                return originalContinuation;
            }
            return new WithCancellationContinuationWrapper(originalContinuation, _cancellationToken).Continuation;
        }
    }

    public struct WithCancellationValueTaskAwaiter<T> : ICriticalNotifyCompletion, INotifyCompletion
    {
        private readonly CancellationToken _cancellationToken;

        private readonly ConfiguredValueTaskAwaitable<T>.ConfiguredValueTaskAwaiter _taskAwaiter;

        public bool IsCompleted
        {
            get
            {
                if (!_taskAwaiter.IsCompleted)
                {
                    return _cancellationToken.IsCancellationRequested;
                }
                return true;
            }
        }

        public WithCancellationValueTaskAwaiter(ConfiguredValueTaskAwaitable<T>.ConfiguredValueTaskAwaiter awaiter, CancellationToken cancellationToken)
        {
            _taskAwaiter = awaiter;
            _cancellationToken = cancellationToken;
        }

        public void OnCompleted(Action continuation)
        {
            _taskAwaiter.OnCompleted(WrapContinuation(ref continuation));
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            _taskAwaiter.UnsafeOnCompleted(WrapContinuation(ref continuation));
        }

        public T GetResult()
        {
            if (!_taskAwaiter.IsCompleted)
            {
                _cancellationToken.ThrowIfCancellationRequested();
            }
            return _taskAwaiter.GetResult();
        }

        private Action WrapContinuation([In] ref Action originalContinuation)
        {
            if (!_cancellationToken.CanBeCanceled)
            {
                return originalContinuation;
            }
            return new WithCancellationContinuationWrapper(originalContinuation, _cancellationToken).Continuation;
        }
    }

    private class WithCancellationContinuationWrapper
    {
        private Action _originalContinuation;

        private readonly CancellationTokenRegistration _registration;

        public Action Continuation { get; }

        public WithCancellationContinuationWrapper(Action originalContinuation, CancellationToken cancellationToken)
        {
            Action callback = ContinuationImplementation;
            _originalContinuation = originalContinuation;
            _registration = cancellationToken.Register(callback);
            Continuation = callback;
        }

        private void ContinuationImplementation()
        {
            Action action = Interlocked.Exchange(ref _originalContinuation, null);
            if (action != null)
            {
                _registration.Dispose();
                action();
            }
        }
    }

    public static WithCancellationTaskAwaitable AwaitWithCancellation(this Task task, CancellationToken cancellationToken)
    {
        return new WithCancellationTaskAwaitable(task, cancellationToken);
    }

    public static WithCancellationTaskAwaitable<T> AwaitWithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
    {
        return new WithCancellationTaskAwaitable<T>(task, cancellationToken);
    }

    public static WithCancellationValueTaskAwaitable<T> AwaitWithCancellation<T>(this ValueTask<T> task, CancellationToken cancellationToken)
    {
        return new WithCancellationValueTaskAwaitable<T>(task, cancellationToken);
    }

    public static T EnsureCompleted<T>(this Task<T> task)
    {
        return task.GetAwaiter().GetResult();
    }

    public static void EnsureCompleted(this Task task)
    {
        task.GetAwaiter().GetResult();
    }

    public static T EnsureCompleted<T>(this ValueTask<T> task)
    {
        return task.GetAwaiter().GetResult();
    }

    public static void EnsureCompleted(this ValueTask task)
    {
        task.GetAwaiter().GetResult();
    }

    public static Enumerable<T> EnsureSyncEnumerable<T>(this IAsyncEnumerable<T> asyncEnumerable)
    {
        return new Enumerable<T>(asyncEnumerable);
    }

    public static ConfiguredValueTaskAwaitable<T> EnsureCompleted<T>(this ConfiguredValueTaskAwaitable<T> awaitable, bool async)
    {
        return awaitable;
    }

    public static ConfiguredValueTaskAwaitable EnsureCompleted(this ConfiguredValueTaskAwaitable awaitable, bool async)
    {
        return awaitable;
    }

    [Conditional("DEBUG")]
    private static void VerifyTaskCompleted(bool isCompleted)
    {
        if (!isCompleted)
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
            throw new InvalidOperationException("Task is not completed");
        }
    }
}
