// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Minimalist.Reactive.Disposables;

/// <summary>
/// Single-assignment disposable slot.
/// </summary>
[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public class SingleDisposable : IsDisposed
{
    private readonly object _gate = new();
    private readonly Action? _action;
    private IDisposable? _disposable;
    private bool _assigned;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleDisposable"/> class.
    /// </summary>
    /// <param name="action">Action to invoke before the assigned disposable is disposed.</param>
    public SingleDisposable(Action? action = null) => _action = action;

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleDisposable"/> class.
    /// </summary>
    /// <param name="disposable">The disposable.</param>
    /// <param name="action">Action to invoke before the assigned disposable is disposed.</param>
    public SingleDisposable(IDisposable disposable, Action? action = null)
        : this(action) => Create(disposable);

    /// <summary>
    /// Gets a value indicating whether this instance is disposed.
    /// </summary>
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
    /// Assigns the disposable held by this slot.
    /// </summary>
    /// <param name="disposable">The disposable.</param>
    /// <exception cref="ArgumentNullException"><paramref name="disposable"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">The slot already has an assignment.</exception>
    public void Create(IDisposable disposable)
    {
        if (disposable == null)
        {
            throw new ArgumentNullException(nameof(disposable));
        }

        var disposeNow = false;
        lock (_gate)
        {
            if (_disposed)
            {
                disposeNow = true;
            }
            else
            {
                if (_assigned)
                {
                    throw new InvalidOperationException("The disposable slot has already been assigned.");
                }

                _assigned = true;
                _disposable = disposable;
            }
        }

        if (disposeNow)
        {
            _action?.Invoke();
            disposable.Dispose();
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
        if (!disposing)
        {
            return;
        }

        IDisposable? disposable;
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            disposable = _disposable;
            _disposable = null;
        }

        if (disposable != null)
        {
            _action?.Invoke();
            disposable.Dispose();
        }
    }
}
