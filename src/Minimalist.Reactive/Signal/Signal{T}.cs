// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Minimalist.Reactive.Disposables;

namespace Minimalist.Reactive.Signals;

/// <summary>
/// Subject.
/// </summary>
/// <typeparam name="T">The Type.</typeparam>
public class Signal<T> : ISignal<T>
{
    private static readonly ObserverHandler[] disposedCompare = new ObserverHandler[0];
    private static readonly ObserverHandler[] terminatedCompare = new ObserverHandler[0];
    private ObserverHandler[] _observers = Array.Empty<ObserverHandler>();
    private Exception? _exception;

    /// <summary>
    /// Gets a value indicating whether indicates whether the subject has observers subscribed to it.
    /// </summary>
    public virtual bool HasObservers => Volatile.Read(ref _observers).Length != 0;

    /// <summary>
    /// Gets a value indicating whether indicates whether the subject has been disposed.
    /// </summary>
    public virtual bool IsDisposed => Volatile.Read(ref _observers) == disposedCompare;

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
#pragma warning disable SA1009 // Closing parenthesis should be spaced correctly
        for (; ; )
        {
            var observers = Volatile.Read(ref _observers);
            if (observers == disposedCompare)
            {
                _exception = null;
                ThrowDisposed();
                break;
            }

            if (observers == terminatedCompare)
            {
                break;
            }

            if (Interlocked.CompareExchange(ref _observers, terminatedCompare, observers) == observers)
            {
                foreach (var observer in observers)
                {
                    observer.Observer?.OnCompleted();
                }

                break;
            }
        }
#pragma warning restore SA1009 // Closing parenthesis should be spaced correctly
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

#pragma warning disable SA1009 // Closing parenthesis should be spaced correctly
        for (; ; )
        {
            var observers = Volatile.Read(ref _observers);
            if (observers == disposedCompare)
            {
                _exception = null;
                ThrowDisposed();
                break;
            }

            if (observers == terminatedCompare)
            {
                break;
            }

            _exception = error;
            if (Interlocked.CompareExchange(ref _observers, terminatedCompare, observers) == observers)
            {
                foreach (var observer in observers)
                {
                    observer.Observer?.OnError(error);
                }

                break;
            }
        }
#pragma warning restore SA1009 // Closing parenthesis should be spaced correctly
    }

    /// <summary>
    /// Called when [next].
    /// </summary>
    /// <param name="value">The value.</param>
    public void OnNext(T value)
    {
        var observers = Volatile.Read(ref _observers);
        if (observers == disposedCompare)
        {
            _exception = null;
            ThrowDisposed();
            return;
        }

        foreach (var observer in observers)
        {
            observer.Observer?.OnNext(value);
        }
    }

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

        var disposable = default(ObserverHandler);
#pragma warning disable SA1009 // Closing parenthesis should be spaced correctly
        for (; ; )
        {
            var observers = Volatile.Read(ref _observers);
            if (observers == disposedCompare)
            {
                _exception = null;
                ThrowDisposed();
                break;
            }

            if (observers == terminatedCompare)
            {
                var ex = _exception;
                if (ex != null)
                {
                    observer.OnError(ex);
                }
                else
                {
                    observer.OnCompleted();
                }

                break;
            }

            disposable ??= new ObserverHandler(this, observer);

            var n = observers.Length;
            var b = new ObserverHandler[n + 1];

            Array.Copy(observers, 0, b, 0, n);

            b[n] = disposable;
            if (Interlocked.CompareExchange(ref _observers, b, observers) == observers)
            {
                return disposable;
            }
        }
#pragma warning restore SA1009 // Closing parenthesis should be spaced correctly

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
                Interlocked.Exchange(ref _observers, disposedCompare);
                _exception = null;
            }
        }
    }

    private static void ThrowDisposed() => throw new ObjectDisposedException(string.Empty);

    private void RemoveObserver(ObserverHandler observer)
    {
#pragma warning disable SA1009 // Closing parenthesis should be spaced correctly
        for (; ; )
        {
            var a = Volatile.Read(ref _observers);
            var n = a.Length;
            if (n == 0)
            {
                break;
            }

            var j = Array.IndexOf(a, observer);

            if (j < 0)
            {
                break;
            }

            ObserverHandler[] b;

            if (n == 1)
            {
                b = Array.Empty<ObserverHandler>();
            }
            else
            {
                b = new ObserverHandler[n - 1];
                Array.Copy(a, 0, b, 0, j);
                Array.Copy(a, j + 1, b, j, n - j - 1);
            }

            if (Interlocked.CompareExchange(ref _observers, b, a) == a)
            {
                break;
            }
        }
#pragma warning restore SA1009 // Closing parenthesis should be spaced correctly
    }

    private class ObserverHandler : IDisposable
    {
        private IObserver<T>? _observer;
        private Signal<T> _subject;

        public ObserverHandler(Signal<T> subject, IObserver<T> observer)
        {
            _subject = subject;
            _observer = observer;
        }

        public IObserver<T>? Observer => _observer;

        public void Dispose()
        {
            var observer = Interlocked.Exchange(ref _observer, null);
            if (observer == null)
            {
                return;
            }

            _subject.RemoveObserver(this);
            _subject = null!;
        }
    }
}
