// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Collections.Concurrent;

namespace Minimalist.Reactive.Disposables;

/// <summary>
/// A CompositeDisposable is a disposable that contains a list of disposables.
/// </summary>
public class MultipleDisposable : IsDisposed
{
    private readonly ConcurrentBag<IDisposable> _disposables;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleDisposable"/> class from a group of disposables.
    /// </summary>
    /// <param name="disposables">Disposables that will be disposed together.</param>
    /// <exception cref="ArgumentNullException"><paramref name="disposables"/> is <see langword="null"/>.</exception>
    public MultipleDisposable(params IDisposable[] disposables) =>
        _disposables = new ConcurrentBag<IDisposable>(disposables);

    /// <summary>
    /// Gets a value indicating whether gets a value that indicates whether the object is disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Creates a new group of disposable resources that are disposed together.
    /// </summary>
    /// <param name="disposables">Disposable resources to add to the group.</param>
    /// <returns>Group of disposable resources that are disposed together.</returns>
    public static IDisposable Create(params IDisposable[] disposables) => new MultipleDisposableBase(disposables);

    /// <summary>
    /// Adds a disposable to the <see cref="MultipleDisposable"/> or disposes the disposable if the <see cref="MultipleDisposable"/> is disposed.
    /// </summary>
    /// <param name="disposable">Disposable to add.</param>
    public void Add(IDisposable disposable)
    {
        if (IsDisposed)
        {
            disposable?.Dispose();
        }
        else
        {
            _disposables.Add(disposable);
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            IsDisposed = true;
            while (_disposables.TryTake(out var disposable))
            {
                disposable?.Dispose();
            }
        }
    }

    private sealed class MultipleDisposableBase : IDisposable
    {
        private IDisposable[]? _disposables;

        public MultipleDisposableBase(IDisposable[] disposables) =>
            Volatile.Write(ref _disposables, disposables ?? throw new ArgumentNullException(nameof(disposables)));

        public void Dispose()
        {
            var disopsables = Interlocked.Exchange(ref _disposables, null);
            if (disopsables != null)
            {
                foreach (var disposable in disopsables)
                {
                    disposable?.Dispose();
                }
            }
        }
    }
}
