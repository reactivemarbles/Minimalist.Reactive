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
    /// Empty Observable. Returns only OnCompleted on specified scheduler.
    /// </summary>
    /// <typeparam name="T">The Type.</typeparam>
    /// <param name="scheduler">The scheduler.</param>
    /// <returns>An Observable.</returns>
    public static IObservable<T> Empty<T>(IScheduler scheduler)
    {
        if (scheduler == Scheduler.Immediate)
        {
            return ImmutableEmptyObservable<T>.Instance;
        }

        return new EmptyObservable<T>(scheduler);
    }

    /// <summary>
    /// Empty Observable. Returns only OnCompleted on specified scheduler. witness is for type inference.
    /// </summary>
    /// <typeparam name="T">The Type.</typeparam>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="witness">The witness.</param>
    /// <returns>An Observable.</returns>
    public static IObservable<T> Empty<T>(IScheduler scheduler, T witness) =>
        Empty<T>(scheduler);

    /// <summary>
    /// Empty Observable. Returns only OnCompleted.
    /// </summary>
    /// <typeparam name="T">The Type.</typeparam>
    /// <returns>An Observable.</returns>
    public static IObservable<T> Empty<T>() =>
        Empty<T>(Scheduler.Immediate);

    /// <summary>
    /// Empty Observable. Returns only OnCompleted. witness is for type inference.
    /// </summary>
    /// <typeparam name="T">The Type.</typeparam>
    /// <param name="witness">The witness.</param>
    /// <returns>An Observable.</returns>
    public static IObservable<T> Empty<T>(T witness) =>
        Empty<T>(Scheduler.Immediate);
}
