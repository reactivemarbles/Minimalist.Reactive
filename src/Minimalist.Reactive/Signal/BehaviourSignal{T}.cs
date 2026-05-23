// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Minimalist.Reactive.Core;
using Minimalist.Reactive.Disposables;

namespace Minimalist.Reactive.Signals;

/// <summary>
/// BehaviourSignal.
/// </summary>
/// <typeparam name="T">The Type.</typeparam>
public class BehaviourSignal<T> : ISignal<T>
{
    private readonly object _observerLock = new();
    private bool _isStopped;
    private T? _lastValue;
    private Exception? _lastError;
    private IObserver<T> _outObserver = EmptyWitness<T>.Instance;

    /// <summary>
    /// Initializes a new instance of the <see cref="BehaviourSignal{T}"/> class.
    /// </summary>
    /// <param name="defaultValue">The default value.</param>
    public BehaviourSignal(T defaultValue) => _lastValue = defaultValue;

    /// <summary>
    /// Gets the current value or throws an exception.
    /// </summary>
    /// <value>The initial value passed to the constructor until <see cref="OnNext"/> is called; after which, the last value passed to <see cref="OnNext"/>.</value>
    /// <remarks>
    /// <para><see cref="Value"/> is frozen after <see cref="OnCompleted"/> is called.</para>
    /// <para>After <see cref="OnError"/> is called, <see cref="Value"/> always throws the specified exception.</para>
    /// <para>An exception is always thrown after <see cref="Dispose()"/> is called.</para>
    /// <alert type="caller">
    /// Reading <see cref="Value"/> is a thread-safe operation, though there's a potential race condition when <see cref="OnNext"/> or <see cref="OnError"/> are being invoked concurrently.
    /// In some cases, it may be necessary for a caller to use external synchronization to avoid race conditions.
    /// </alert>
    /// </remarks>
    public T Value
    {
        get
        {
            ThrowIfDisposed();
            _lastError.Rethrow();

            return _lastValue!;
        }
    }

    /// <summary>
    /// Gets a value indicating whether this instance has observers.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance has observers; otherwise, <c>false</c>.
    /// </value>
    public bool HasObservers => _outObserver is not EmptyWitness<T> && !_isStopped && !IsDisposed;

    /// <summary>
    /// Gets a value indicating whether this instance is disposed.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
    /// </value>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Tries to get the current value or throws an exception.
    /// </summary>
    /// <param name="value">The initial value passed to the constructor until <see cref="OnNext"/> is called; after which, the last value passed to <see cref="OnNext"/>.</param>
    /// <returns>true if a value is available; false if the subject was disposed.</returns>
    /// <remarks>
    /// <para>The value returned from <see cref="TryGetValue"/> is frozen after <see cref="OnCompleted"/> is called.</para>
    /// <para>After <see cref="OnError"/> is called, <see cref="TryGetValue"/> always throws the specified exception.</para>
    /// <alert type="caller">
    /// Calling <see cref="TryGetValue"/> is a thread-safe operation, though there's a potential race condition when <see cref="OnNext"/> or <see cref="OnError"/> are being invoked concurrently.
    /// In some cases, it may be necessary for a caller to use external synchronization to avoid race conditions.
    /// </alert>
    /// </remarks>
    public bool TryGetValue(out T? value)
    {
        lock (_observerLock)
        {
            if (IsDisposed)
            {
                value = default;
                return false;
            }

            _lastError.Rethrow();

            value = _lastValue!;
            return true;
        }
    }

    /// <summary>
    /// Notifies all subscribed observers about the end of the sequence.
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
        }

        old.OnCompleted();
    }

    /// <summary>
    /// Notifies all subscribed observers about the exception.
    /// </summary>
    /// <param name="error">The exception to send to all observers.</param>
    /// <exception cref="ArgumentNullException"><paramref name="error"/> is <c>null</c>.</exception>
    public void OnError(Exception error)
    {
        if (error == null)
        {
            throw new ArgumentNullException(nameof(error));
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
            _lastError = error;
        }

        old.OnError(error);
    }

    /// <summary>
    /// Notifies all subscribed observers about the arrival of the specified element in the sequence.
    /// </summary>
    /// <param name="value">The value to send to all observers.</param>
    public void OnNext(T value)
    {
        IObserver<T> current;
        lock (_observerLock)
        {
            if (_isStopped)
            {
                return;
            }

            _lastValue = value;
            current = _outObserver;
        }

        current.OnNext(value);
    }

    /// <summary>
    /// Subscribes an observer to the subject.
    /// </summary>
    /// <param name="observer">Observer to subscribe to the subject.</param>
    /// <returns>Disposable object that can be used to unsubscribe the observer from the subject.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="observer"/> is <c>null</c>.</exception>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        if (observer == null)
        {
            throw new ArgumentNullException(nameof(observer));
        }

        var ex = default(Exception);
        var v = default(T);
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

                v = _lastValue;
                subscription = new ObserverHandler(this, observer);
            }
            else
            {
                ex = _lastError;
            }
        }

        if (subscription != null)
        {
            observer.OnNext(v!);
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
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
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
                    _lastValue = default;
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

    private class ObserverHandler : IDisposable
    {
        private readonly object _lock = new();
        private BehaviourSignal<T>? _subject;
        private IObserver<T>? _observer;

        public ObserverHandler(BehaviourSignal<T> subject, IObserver<T> observer)
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
