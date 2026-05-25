// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Runtime.ExceptionServices;
using Minimalist.Reactive.Core;
using Minimalist.Reactive.Disposables;

namespace Minimalist.Reactive.Signals;

/// <summary>
/// Subject.
/// </summary>
/// <typeparam name="T">The Type.</typeparam>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class Signal<T> : ISignal<T>
{
    private static readonly Action<T> NoopOnNext = static _ => { };
    private static readonly Action<T> ThrowDisposedOnNext = static _ => ThrowDisposed();

    private readonly object _observerLock = new();
    private Exception? _exception;
    private IObserver<T> _outObserver = EmptyWitness<T>.Instance;
    private Action<T> _onNext = NoopOnNext;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2213:Disposable fields should be disposed", Justification = "The subscription is owned by the caller; Signal only keeps a removal token and clears it on disposal.")]
    private ActionSubscription? _singleSubscription;

    private bool _isDisposed;
    private bool _isStopped;

    /// <summary>
    /// Gets a value indicating whether indicates whether the subject has observers subscribed to it.
    /// </summary>
    public virtual bool HasObservers => (_singleSubscription != null || (!ReferenceEquals(_outObserver, EmptyWitness<T>.Instance) && !ReferenceEquals(_outObserver, DisposedWitness<T>.Instance))) && !_isStopped;

    /// <summary>
    /// Gets a value indicating whether indicates whether the subject has been disposed.
    /// </summary>
    public virtual bool IsDisposed => _isDisposed;

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Called when [completed].
    /// </summary>
    public void OnCompleted()
    {
        IObserver<T> old;
        var direct = false;
        lock (_observerLock)
        {
            ThrowIfDisposed();
            if (_isStopped)
            {
                return;
            }

            if (_singleSubscription != null)
            {
                ClearSingleActionObserver();
                old = EmptyWitness<T>.Instance;
                direct = true;
            }
            else
            {
                old = _outObserver;
                _outObserver = EmptyWitness<T>.Instance;
                _onNext = NoopOnNext;
            }

            _isStopped = true;
        }

        if (!direct)
        {
            old.OnCompleted();
        }
    }

    /// <summary>
    /// Called when [error].
    /// </summary>
    /// <param name="error">The error.</param>
    public void OnError(Exception error)
    {
        if (error == null)
        {
            throw new ArgumentNullException(nameof(error));
        }

        IObserver<T> old;
        var direct = false;
        lock (_observerLock)
        {
            ThrowIfDisposed();
            if (_isStopped)
            {
                return;
            }

            _exception = error;
            if (_singleSubscription != null)
            {
                ClearSingleActionObserver();
                old = EmptyWitness<T>.Instance;
                direct = true;
            }
            else
            {
                old = _outObserver;
                _outObserver = EmptyWitness<T>.Instance;
                _onNext = NoopOnNext;
            }

            _isStopped = true;
        }

        if (direct)
        {
            ExceptionDispatchInfo.Capture(error).Throw();
            return;
        }

        old.OnError(error);
    }

    /// <summary>
    /// Called when [next].
    /// </summary>
    /// <param name="value">The value.</param>
    public void OnNext(T value) => _onNext(value);

    /// <summary>
    /// Subscribes the specified observer.
    /// </summary>
    /// <param name="observer">The observer.</param>
    /// <returns>
    /// A IDisposable.
    /// </returns>
    public IDisposable Subscribe(IObserver<T> observer)
    {
        if (observer == null)
        {
            throw new ArgumentNullException(nameof(observer));
        }

        Exception? ex;
        bool stopped;
        ObserverHandler? subscription = null;

        lock (_observerLock)
        {
            ThrowIfDisposed();
            stopped = _isStopped;
            ex = _exception;
            if (!stopped)
            {
                subscription = new ObserverHandler(this, observer);
                AddObserver(observer);
            }
        }

        if (subscription != null)
        {
            return subscription;
        }

        if (ex != null)
        {
            observer.OnError(ex);
        }
        else
        {
            observer.OnCompleted();
        }

        return Disposable.Empty;
    }

    internal IDisposable SubscribeAction(Action<T> onNext)
    {
        Exception? ex;
        bool stopped;
        IDisposable? subscription = null;

        lock (_observerLock)
        {
            ThrowIfDisposed();
            stopped = _isStopped;
            ex = _exception;
            if (!stopped)
            {
                if (ReferenceEquals(_outObserver, EmptyWitness<T>.Instance) && _singleSubscription == null)
                {
                    var directSubscription = new ActionSubscription(this);
                    _onNext = onNext!;
                    _singleSubscription = directSubscription;
                    subscription = directSubscription;
                }
                else
                {
                    var observer = new DirectActionObserver(onNext!);
                    subscription = new ObserverHandler(this, observer);
                    AddObserver(observer);
                }
            }
        }

        if (subscription != null)
        {
            return subscription;
        }

        if (ex != null)
        {
            ExceptionDispatchInfo.Capture(ex).Throw();
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
                    _exception = null;
                    ClearSingleActionObserver();
                    _onNext = ThrowDisposedOnNext;
                    _isDisposed = true;
                }
            }
        }
    }

    private static void ThrowDisposed() => throw new ObjectDisposedException(string.Empty);

    private void ThrowIfDisposed()
    {
        if (IsDisposed)
        {
            ThrowDisposed();
        }
    }

    private void AddObserver(IObserver<T> observer)
    {
        PromoteSingleActionObserver();
        if (_outObserver is ListWitness<T> listObserver)
        {
            _outObserver = listObserver.Add(observer);
        }
        else
        {
            var current = _outObserver;
            if (ReferenceEquals(current, EmptyWitness<T>.Instance))
            {
                _outObserver = observer;
            }
            else
            {
                _outObserver = new ListWitness<T>(new ImmutableList<IObserver<T>>(new[] { current, observer }));
            }
        }

        _onNext = DispatchObserver;
    }

    private void RemoveActionObserver(ActionSubscription subscription)
    {
        lock (_observerLock)
        {
            if (ReferenceEquals(_singleSubscription, subscription))
            {
                ClearSingleActionObserver();
            }
        }
    }

    private void RemoveObserver(IObserver<T> observer)
    {
        lock (_observerLock)
        {
            if (ReferenceEquals(_outObserver, observer))
            {
                _outObserver = EmptyWitness<T>.Instance;
                _onNext = NoopOnNext;
            }
            else if (_outObserver is ListWitness<T> listObserver)
            {
                _outObserver = listObserver.Remove(observer);
                _onNext = ReferenceEquals(_outObserver, EmptyWitness<T>.Instance) ? NoopOnNext : DispatchObserver;
            }
        }
    }

    private void ClearSingleActionObserver()
    {
        _singleSubscription = null;
        _onNext = NoopOnNext;
    }

    private void PromoteSingleActionObserver()
    {
        if (_singleSubscription == null)
        {
            return;
        }

        var singleObserver = new DirectActionObserver(_onNext);
        _singleSubscription.Observer = singleObserver;
        _singleSubscription = null;
        _outObserver = singleObserver;
    }

    private void DispatchObserver(T value) => _outObserver.OnNext(value);

    private sealed class DirectActionObserver : IObserver<T>
    {
        private readonly Action<T> _onNext;

        public DirectActionObserver(Action<T> onNext) => _onNext = onNext;

        public void OnCompleted()
        {
        }

        public void OnError(Exception error) => ExceptionDispatchInfo.Capture(error).Throw();

        public void OnNext(T value) => _onNext(value);
    }

    private sealed class ActionSubscription : IDisposable
    {
        private Signal<T>? _subject;

        public ActionSubscription(Signal<T> subject) => _subject = subject;

        public IObserver<T>? Observer { get; set; }

        public void Dispose()
        {
            var subject = Interlocked.Exchange(ref _subject, null);
            if (subject == null)
            {
                return;
            }

            var observer = Observer;
            if (observer == null)
            {
                subject.RemoveActionObserver(this);
            }
            else
            {
                subject.RemoveObserver(observer);
                Observer = null;
            }
        }
    }

    private sealed class ObserverHandler : IDisposable
    {
        private IObserver<T>? _observer;
        private Signal<T>? _subject;

        public ObserverHandler(Signal<T> subject, IObserver<T> observer)
        {
            _subject = subject;
            _observer = observer;
        }

        public void Dispose()
        {
            var observer = Interlocked.Exchange(ref _observer, null);
            if (observer == null)
            {
                return;
            }

            var subject = Interlocked.Exchange(ref _subject, null);
            subject?.RemoveObserver(observer);
        }
    }
}
