// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Minimalist.Reactive.Disposables;

/// <summary>
/// Minimalist alias for a group of disposables that are disposed together.
/// </summary>
public sealed class Pocket : MultipleDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Pocket"/> class.
    /// </summary>
    /// <param name="disposables">Initial disposables.</param>
    public Pocket(params IDisposable[] disposables)
        : base(disposables)
    {
    }
}
