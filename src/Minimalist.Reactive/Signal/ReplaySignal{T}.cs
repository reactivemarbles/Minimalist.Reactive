// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Minimalist.Reactive.Concurrency;
using Minimalist.Reactive.Core;
using Minimalist.Reactive.Disposables;

namespace Minimalist.Reactive.Signals;

/// <summary>
/// ReplaySignal.
/// </summary>
/// <typeparam name="T">The Type.</typeparam>
public class ReplaySignal<T> : ISignal<T>
{
    private readonly int _bufferSize;
    private readonly TimeSpan _window;
    private readonly DateTimeOffset _startTime;
    private readonly IScheduler _scheduler;
    private readonly object _observerLock = new();
    private bool _isStopped;
    private Exception? _lastError;
    private IObserver<T> _outObserver = EmptyWitness<T>.Instance;
    private Queue<TimeInterval<T>>? _queue = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ReplaySignal{T}"/> class.
    /// </summary>
    /// <param name="bufferSize">Size of the buffer.</param>
    /// <param name="window">The window.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// bufferSize
    /// or
    /// window.
    /// </exception>
    /// <exception cref="ArgumentNullException">scheduler.</exception>
    public ReplaySignal(int bufferSize, TimeSpan window, IScheduler scheduler)
    {
        if (bufferSize < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bufferSize));
        }

        if (window < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(window));
        }

        _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
        _bufferSize = bufferSize;
        _window = window;
        _startTime = scheduler.Now;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReplaySignal{T}"/> class.
    /// </summary>
    /// <param name="bufferSize">Size of the buffer.</param>
    /// <param name="window">The window.</param>
    public ReplaySignal(int bufferSize, TimeSpan window)
      : this(bufferSize, window, Scheduler.CurrentThread)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReplaySignal{T}"/> class.
    /// </summary>
    public ReplaySignal()
      : this(int.MaxValue, TimeSpan.MaxValue, Scheduler.CurrentThread)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReplaySignal{T}"/> class.
    /// </summary>
    /// <param name="scheduler">The scheduler.</param>
    public ReplaySignal(IScheduler scheduler)
      : this(int.MaxValue, TimeSpan.MaxValue, scheduler)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReplaySignal{T}"/> class.
    /// </summary>
    /// <param name="bufferSize">Size of the buffer.</param>
    /// <param name="scheduler">The scheduler.</param>
    public ReplaySignal(int bufferSize, IScheduler scheduler)
      : this(bufferSize, TimeSpan.MaxValue, scheduler)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReplaySignal{T}"/> class.
    /// </summary>
    /// <param name="bufferSize">Size of the buffer.</param>
    public ReplaySignal(int bufferSize)
      : this(bufferSize, TimeSpan.MaxValue, Scheduler.CurrentThread)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ReplaySignal{T}"/> class.
    /// </summary>
    /// <param name="window">The window.</param>
    /// <param name="scheduler">The scheduler.</param>
    public ReplaySignal(TimeSpan window, IScheduler scheduler)
      : this(int.MaxValue, window, scheduler) => _window = window;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReplaySignal{T}"/> class.
    /// </summary>
    /// <param name="window">The window.</param>
    public ReplaySignal(TimeSpan window)
      : this(int.MaxValue, window, Scheduler.CurrentThread)
    {
    }

    /// <summary>
    /// Gets a value indicating whether this instance has observers.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance has observers; otherwise, <c>false</c>.
    /// </value>
    public bool HasObservers => (_outObserver as ListWitness<T>)?.HasObservers ?? false;

    /// <summary>
    /// Gets a value indicating whether this instance is disposed.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
    /// </value>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Called when [completed].
    /// </summary>
    public void OnCompleted()
    {
        IObserver<T> old;
        lock (_observerLock)
        {
            ThrowIfDisposed();
            if (_isStopped)
            {
                return;
            }

            old = _outObserver;
            _outObserver = EmptyWitness<T>.Instance;
            _isStopped = true;
            Trim();
        }

        old.OnCompleted();
    }

    /// <summary>
    /// Called when [error].
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <exception cref="ArgumentNullException">exception.</exception>
    public void OnError(Exception exception)
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        IObserver<T> old;
        lock (_observerLock)
        {
            ThrowIfDisposed();
            if (_isStopped)
            {
                return;
            }

            old = _outObserver;
            _outObserver = EmptyWitness<T>.Instance;
            _isStopped = true;
            _lastError = exception;
            Trim();
        }

        old.OnError(exception);
    }

    /// <summary>
    /// Called when [next].
    /// </summary>
    /// <param name="value">The value.</param>
    public void OnNext(T value)
    {
        IObserver<T> current;
        lock (_observerLock)
        {
            ThrowIfDisposed();
            if (_isStopped)
            {
                return;
            }

            _queue?.Enqueue(new TimeInterval<T>(value, _scheduler.Now - _startTime));
            Trim();

            current = _outObserver;
        }

        current.OnNext(value);
    }

    /// <summary>
    /// Subscribes the specified observer.
    /// </summary>
    /// <param name="observer">The observer.</param>
    /// <returns>A Disposable.</returns>
    /// <exception cref="ArgumentNullException">observer.</exception>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        if (observer == null)
        {
            throw new ArgumentNullException(nameof(observer));
        }

        var ex = default(Exception);
        var subscription = default(ObserverHandler);

        lock (_observerLock)
        {
            ThrowIfDisposed();
            if (!_isStopped)
            {
                if (_outObserver is ListWitness<T> listObserver)
                {
                    _outObserver = listObserver.Add(observer);
                }
                else
                {
                    var current = _outObserver;
                    if (current is EmptyWitness<T>)
                    {
                        _outObserver = new ListWitness<T>(new ImmutableList<IObserver<T>>(new[] { observer }));
                    }
                    else
                    {
                        _outObserver = new ListWitness<T>(new ImmutableList<IObserver<T>>(new[] { current, observer }));
                    }
                }

                subscription = new ObserverHandler(this, observer);
            }

            ex = _lastError;
            Trim();
            foreach (var item in _queue!)
            {
                observer.OnNext(item.Value);
            }
        }

        if (subscription != null)
        {
            return subscription;
        }
        else if (ex != null)
        {
            observer.OnError(ex);
        }
        else
        {
            observer.OnCompleted();
        }

        return Disposable.Empty;
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                lock (_observerLock)
                {
                    _outObserver = DisposedWitness<T>.Instance;
                    _lastError = null;
                    _queue = null;
                }
            }

            IsDisposed = true;
        }
    }

    private void ThrowIfDisposed()
    {
        if (IsDisposed)
        {
            throw new ObjectDisposedException(string.Empty);
        }
    }

    private void Trim()
    {
        var elapsedTime = Scheduler.Normalize(_scheduler.Now - _startTime);

        while (_queue!.Count > _bufferSize)
        {
            _queue.Dequeue();
        }

        while (_queue.Count > 0 && elapsedTime.Subtract(_queue.Peek().Interval).CompareTo(_window) > 0)
        {
            _queue.Dequeue();
        }
    }

    private class ObserverHandler : IDisposable
    {
        private readonly object _lock = new();
        private ReplaySignal<T>? _subject;
        private IObserver<T>? _observer;

        public ObserverHandler(ReplaySignal<T> subject, IObserver<T> observer)
        {
            _subject = subject;
            _observer = observer;
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_subject != null)
                {
                    lock (_subject._observerLock)
                    {
                        _subject._outObserver = _subject._outObserver is ListWitness<T> listObserver ? listObserver.Remove(_observer!) : EmptyWitness<T>.Instance;

                        _observer = null;
                        _subject = null;
                    }
                }
            }
        }
    }
}
