// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Minimalist.Reactive.Disposables;

/// <summary>
/// A disposable pocket that contains a set of disposables and disposes them together.
/// </summary>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class MultipleDisposable : IsDisposed
{
    private readonly object _gate = new();
    private List<IDisposable>? _disposables;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleDisposable"/> class from a group of disposables.
    /// </summary>
    /// <param name="disposables">Disposables that will be disposed together.</param>
    /// <exception cref="ArgumentNullException"><paramref name="disposables"/> is <see langword="null"/>.</exception>
    public MultipleDisposable(params IDisposable[] disposables)
    {
        if (disposables == null)
        {
            throw new ArgumentNullException(nameof(disposables));
        }

        _disposables = new List<IDisposable>(disposables.Length);
        for (var i = 0; i < disposables.Length; i++)
        {
            if (disposables[i] != null)
            {
                _disposables.Add(disposables[i]);
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the object is disposed.
    /// </summary>
    public bool IsDisposed
    {
        get
        {
            lock (_gate)
            {
                return _disposables == null;
            }
        }
    }

    /// <summary>
    /// Creates a new group of disposable resources that are disposed together.
    /// </summary>
    /// <param name="disposables">Disposable resources to add to the group.</param>
    /// <returns>Group of disposable resources that are disposed together.</returns>
    public static IDisposable Create(params IDisposable[] disposables) => new MultipleDisposableBase(disposables);

    /// <summary>
    /// Adds a disposable to the <see cref="MultipleDisposable"/> or disposes it immediately if the pocket is already disposed.
    /// </summary>
    /// <param name="disposable">Disposable to add.</param>
    /// <exception cref="ArgumentNullException"><paramref name="disposable"/> is <see langword="null"/>.</exception>
    public void Add(IDisposable disposable)
    {
        if (disposable == null)
        {
            throw new ArgumentNullException(nameof(disposable));
        }

        var shouldDispose = false;
        lock (_gate)
        {
            if (_disposables == null)
            {
                shouldDispose = true;
            }
            else
            {
                _disposables.Add(disposable);
            }
        }

        if (shouldDispose)
        {
            disposable.Dispose();
        }
    }

    /// <summary>
    /// Removes and disposes the requested disposable from the pocket.
    /// </summary>
    /// <param name="item">Disposable to remove.</param>
    /// <returns><see langword="true"/> if the item was found and disposed; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="item"/> is null.</exception>
    public bool Remove(IDisposable? item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item));
        }

        var shouldDispose = false;
        lock (_gate)
        {
            if (_disposables != null)
            {
                shouldDispose = _disposables.Remove(item);
            }
        }

        if (shouldDispose)
        {
            item.Dispose();
        }

        return shouldDispose;
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
        if (!disposing)
        {
            return;
        }

        List<IDisposable>? disposables;
        lock (_gate)
        {
            disposables = _disposables;
            _disposables = null;
        }

        if (disposables == null)
        {
            return;
        }

        for (var i = 0; i < disposables.Count; i++)
        {
            disposables[i].Dispose();
        }
    }

    private sealed class MultipleDisposableBase : IDisposable
    {
        private IDisposable[]? _disposables;

        public MultipleDisposableBase(IDisposable[] disposables) =>
            Volatile.Write(ref _disposables, disposables ?? throw new ArgumentNullException(nameof(disposables)));

        public void Dispose()
        {
            var disposables = Interlocked.Exchange(ref _disposables, null);
            if (disposables != null)
            {
                foreach (var disposable in disposables)
                {
                    disposable?.Dispose();
                }
            }
        }
    }
}
