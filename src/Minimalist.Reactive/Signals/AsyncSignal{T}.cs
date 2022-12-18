﻿// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Minimalist.Reactive.Core;
using Minimalist.Reactive.Disposables;

namespace Minimalist.Reactive.Signals;

/// <summary>
/// AsyncSignal.
/// </summary>
/// <typeparam name="T">The Type.</typeparam>
/// <seealso cref="Minimalist.Reactive.Signals.ISignal&lt;T&gt;" />
#if NET472 || NETSTANDARD2_0
public class AsyncSignal<T> : ISignal<T>, System.Runtime.CompilerServices.INotifyCompletion
#else
public class AsyncSignal<T> : ISignal<T>
#endif
{
    private readonly object _observerLock = new();
    private T? _lastValue;
    private bool _hasValue;
    private Exception? _lastError;
    private IObserver<T> _outObserver = EmptyObserver<T>.Instance;

    /// <summary>
    /// Gets a value indicating whether this instance is disposed.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
    /// </value>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <value>
    /// The value.
    /// </value>
    /// <exception cref="System.InvalidOperationException">AsyncSubject is not completed yet.</exception>
    public T Value
    {
        get
        {
            ThrowIfDisposed();
            if (!IsCompleted)
            {
                throw new InvalidOperationException("AsyncSubject is not completed yet");
            }

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
    public bool HasObservers => _outObserver is not EmptyObserver<T> && !IsCompleted && !IsDisposed;

    /// <summary>
    /// Gets a value indicating whether this instance is completed.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is completed; otherwise, <c>false</c>.
    /// </value>
    public bool IsCompleted { get; private set; }

    /// <summary>
    /// Called when [completed].
    /// </summary>
    public void OnCompleted()
    {
        IObserver<T> old;
        T? v;
        bool hv;
        lock (_observerLock)
        {
            ThrowIfDisposed();
            if (IsCompleted)
            {
                return;
            }

            old = _outObserver;
            _outObserver = EmptyObserver<T>.Instance;
            IsCompleted = true;
            v = _lastValue;
            hv = _hasValue;
        }

        if (hv)
        {
            old.OnNext(v!);
            old.OnCompleted();
        }
        else
        {
            old.OnCompleted();
        }
    }

    /// <summary>
    /// Called when [error].
    /// </summary>
    /// <param name="error">The error.</param>
    /// <exception cref="System.ArgumentNullException">error.</exception>
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
            if (IsCompleted)
            {
                return;
            }

            old = _outObserver;
            _outObserver = EmptyObserver<T>.Instance;
            IsCompleted = true;
            _lastError = error;
        }

        old.OnError(error);
    }

    /// <summary>
    /// Called when [next].
    /// </summary>
    /// <param name="value">The value.</param>
    public void OnNext(T value)
    {
        lock (_observerLock)
        {
            ThrowIfDisposed();
            if (IsCompleted)
            {
                return;
            }

            _hasValue = true;
            _lastValue = value;
        }
    }

    /// <summary>
    /// Subscribes the specified observer.
    /// </summary>
    /// <param name="observer">The observer.</param>
    /// <returns>A Disposable.</returns>
    /// <exception cref="System.ArgumentNullException">observer.</exception>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        if (observer == null)
        {
            throw new ArgumentNullException(nameof(observer));
        }

        var ex = default(Exception);
        var v = default(T);
        var hv = false;

        lock (_observerLock)
        {
            ThrowIfDisposed();
            if (!IsCompleted)
            {
                if (_outObserver is ListObserver<T> listObserver)
                {
                    _outObserver = listObserver.Add(observer);
                }
                else
                {
                    var current = _outObserver;
                    if (current is EmptyObserver<T>)
                    {
                        _outObserver = new ListObserver<T>(new ImmutableList<IObserver<T>>(new[] { observer }));
                    }
                    else
                    {
                        _outObserver = new ListObserver<T>(new ImmutableList<IObserver<T>>(new[] { current, observer }));
                    }
                }

                return new Subscription(this, observer);
            }

            ex = _lastError;
            v = _lastValue;
            hv = _hasValue;
        }

        if (ex != null)
        {
            observer.OnError(ex);
        }
        else if (hv)
        {
            observer.OnNext(v!);
            observer.OnCompleted();
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
                    _outObserver = DisposedObserver<T>.Instance;
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

#if NET472 || NETSTANDARD2_0

    /// <summary>
    /// Gets an awaitable object for the current AsyncSubject.
    /// </summary>
    /// <returns>Object that can be awaited.</returns>
#pragma warning disable SA1202 // Elements should be ordered by access
    public AsyncSignal<T> GetAwaiter() => this;
#pragma warning restore SA1202 // Elements should be ordered by access

    /// <summary>
    /// Specifies a callback action that will be invoked when the subject completes.
    /// </summary>
    /// <param name="continuation">Callback action that will be invoked when the subject completes.</param>
    /// <exception cref="ArgumentNullException"><paramref name="continuation"/> is null.</exception>
    public void OnCompleted(Action continuation)
    {
        if (continuation == null)
        {
            throw new ArgumentNullException(nameof(continuation));
        }

        OnCompleted(continuation, true);
    }

    /// <summary>
    /// Gets the last element of the subject, potentially blocking until the subject completes successfully or exceptionally.
    /// </summary>
    /// <returns>The last element of the subject. Throws an InvalidOperationException if no element was received.</returns>
    /// <exception cref="InvalidOperationException">The source sequence is empty.</exception>
    public T GetResult()
    {
        if (!IsCompleted)
        {
            var e = new ManualResetEvent(false);
            OnCompleted(() => e.Set(), false);
            e.WaitOne();
        }

        _lastError.Rethrow();

        if (!_hasValue)
        {
            throw new InvalidOperationException("NO_ELEMENTS");
        }

        return _lastValue!;
    }

    private void OnCompleted(Action continuation, bool originalContext) =>
        Subscribe(new AwaitObserver(continuation, originalContext));

    private class AwaitObserver : IObserver<T>
    {
        private readonly SynchronizationContext? _context;
        private readonly Action _callback;

        public AwaitObserver(Action callback, bool originalContext)
        {
            if (originalContext)
            {
                _context = SynchronizationContext.Current;
            }

            _callback = callback;
        }

        public void OnCompleted() => InvokeOnOriginalContext();

        public void OnError(Exception error) => InvokeOnOriginalContext();

        public void OnNext(T value)
        {
        }

        private void InvokeOnOriginalContext()
        {
            if (_context != null)
            {
                _context.Post(c => ((Action)c)(), _callback);
            }
            else
            {
                _callback();
            }
        }
    }

#endif

    private class Subscription : IDisposable
    {
        private readonly object _gate = new();
        private AsyncSignal<T>? _parent;
        private IObserver<T>? _unsubscribeTarget;

        public Subscription(AsyncSignal<T> parent, IObserver<T> unsubscribeTarget)
        {
            _parent = parent;
            _unsubscribeTarget = unsubscribeTarget;
        }

        public void Dispose()
        {
            lock (_gate)
            {
                if (_parent != null)
                {
                    lock (_parent._observerLock)
                    {
                        _parent._outObserver = _parent._outObserver is ListObserver<T> listObserver ? listObserver.Remove(_unsubscribeTarget!) : EmptyObserver<T>.Instance;

                        _unsubscribeTarget = null;
                        _parent = null;
                    }
                }
            }
        }
    }
}
