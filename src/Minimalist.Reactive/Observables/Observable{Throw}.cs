// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Minimalist.Reactive.Concurrency;
using Minimalist.Reactive.Observables.Core;

namespace Minimalist.Reactive.Observables;

/// <summary>
/// Observable.
/// </summary>
public static partial class Observable
{
    /// <summary>
    /// Empty Observable. Returns only onError on specified scheduler.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="error">The error.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <returns>An Observable.</returns>
    public static IObservable<T> Throw<T>(Exception error, IScheduler scheduler) =>
        new ThrowObservable<T>(error, scheduler);

    /// <summary>
    /// Empty Observable. Returns only onError.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="error">The error.</param>
    /// <returns>An Observable.</returns>
    public static IObservable<T> Throw<T>(Exception error) =>
        Throw<T>(error, Scheduler.Immediate);

    /// <summary>
    /// Empty Observable. Returns only onError. witness if for Type inference.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="error">The error.</param>
    /// <param name="witness">The witness.</param>
    /// <returns>An Observable.</returns>
    public static IObservable<T> Throw<T>(Exception error, T witness) =>
        Throw<T>(error, Scheduler.Immediate);

    /// <summary>
    /// Empty Observable. Returns only onError on specified scheduler. witness if for Type inference.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="error">The error.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="witness">The witness.</param>
    /// <returns>An Observable.</returns>
    public static IObservable<T> Throw<T>(Exception error, IScheduler scheduler, T witness) =>
        Throw<T>(error, scheduler);
}
