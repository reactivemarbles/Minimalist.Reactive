// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Minimalist.Reactive.Disposables;

/// <summary>
/// CancellationDisposable.
/// </summary>
/// <seealso cref="System.IDisposable" />
public sealed class CancellationDisposable : IDisposable
{
    private CancellationTokenSource _cts;

    /// <summary>
    /// Initializes a new instance of the <see cref="CancellationDisposable"/> class.
    /// </summary>
    /// <param name="cts">The CTS.</param>
    /// <exception cref="System.ArgumentNullException">cts.</exception>
    public CancellationDisposable(CancellationTokenSource cts) => _cts = cts ?? throw new ArgumentNullException(nameof(cts));

    /// <summary>
    /// Initializes a new instance of the <see cref="CancellationDisposable"/> class.
    /// </summary>
    public CancellationDisposable()
      : this(new CancellationTokenSource())
    {
    }

    /// <summary>
    /// Gets the token.
    /// </summary>
    /// <value>
    /// The token.
    /// </value>
    public CancellationToken Token => _cts.Token;

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose() => _cts.Cancel();
}
