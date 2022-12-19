// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace Minimalist.Reactive.Disposables;

/// <summary>
/// SingleDisposable.
/// </summary>
public class SingleDisposable : IsDisposed
{
    private readonly Action? _action;
    private IDisposable? _disposable;

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleDisposable"/> class.
    /// </summary>
    /// <param name="action">The action.</param>
    public SingleDisposable(Action? action = null) =>
        _action = action;

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleDisposable"/> class.
    /// </summary>
    /// <param name="disposable">The disposable.</param>
    /// <param name="action">The action to call before disposal.</param>
    public SingleDisposable(IDisposable disposable, Action? action = null) =>
        _disposable = Disposable.Create(() =>
        {
            action?.Invoke();
            disposable.Dispose();
            IsDisposed = true;
        });

    /// <summary>
    /// Gets a value indicating whether this instance is disposed.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
    /// </value>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Creates the specified disposable.
    /// </summary>
    /// <param name="disposable">The disposable.</param>
    public void Create(IDisposable disposable) =>
        _disposable = Disposable.Create(() =>
        {
            _action?.Invoke();
            disposable.Dispose();
            IsDisposed = true;
        });

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
        if (!IsDisposed && disposing)
        {
            if (_disposable == null)
            {
                IsDisposed = true;
            }

            _disposable?.Dispose();
        }
    }
}
