// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Minimalist.Reactive.Disposables;

/// <summary>
/// SingleReplaceableDisposable.
/// </summary>
public class SingleReplaceableDisposable : IsDisposed
{
    private readonly object _gate = new();
    private readonly Action? _action;
    private IDisposable? _disposable;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleReplaceableDisposable"/> class.
    /// </summary>
    /// <param name="action">The action.</param>
    public SingleReplaceableDisposable(Action? action = null) =>
        _action = action;

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleReplaceableDisposable"/> class.
    /// </summary>
    /// <param name="disposable">The disposable.</param>
    /// <param name="action">The action to call before disposal.</param>
    public SingleReplaceableDisposable(IDisposable disposable, Action? action = null)
    {
        Create(disposable);
        _action = action;
    }

    /// <summary>
    /// Gets a value indicating whether this instance is disposed.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
    /// </value>
    public bool IsDisposed
    {
        get
        {
            lock (_gate)
            {
                return _disposed;
            }
        }
    }

    /// <summary>
    /// Creates the specified disposable.
    /// </summary>
    /// <param name="disposable">The disposable.</param>
    public void Create(IDisposable disposable)
    {
        var shouldDispose = false;
        var old = default(IDisposable);
        lock (_gate)
        {
            shouldDispose = _disposed;
            if (!shouldDispose)
            {
                old = _disposable;
                _disposable = disposable;
            }
        }

        old?.Dispose();

        if (shouldDispose && disposable != null)
        {
            disposable.Dispose();
            _action?.Invoke();
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
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
        var old = default(IDisposable);

        lock (_gate)
        {
            if (!_disposed)
            {
                _disposed = true;
                old = _disposable;
                _disposable = null;
            }
        }

        old?.Dispose();
        _action?.Invoke();
    }
}
