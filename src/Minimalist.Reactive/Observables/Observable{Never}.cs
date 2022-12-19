// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Minimalist.Reactive.Observables.Core;

namespace Minimalist.Reactive.Observables;

/// <summary>
/// Observable.
/// </summary>
public static partial class Observable
{
    /// <summary>
    /// Non-Terminating Observable. It's no returns, never finish.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <returns>An Observable.</returns>
    public static IObservable<T> Never<T>() => ImmutableNeverObservable<T>.Instance;

    /// <summary>
    /// Non-Terminating Observable. It's no returns, never finish. witness is for type inference.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="witness">The witness.</param>
    /// <returns>An Observable.</returns>
    public static IObservable<T> Never<T>(T witness) => ImmutableNeverObservable<T>.Instance;
}
