// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Minimalist.Reactive.Concurrency;
using Minimalist.Reactive.Signals.Core;

namespace Minimalist.Reactive.Signals;

/// <summary>
/// Signals.
/// </summary>
public static partial class Signal
{
    /// <summary>
    /// Empty Signals. Returns only OnCompleted on specified scheduler.
    /// </summary>
    /// <typeparam name="T">The Type.</typeparam>
    /// <param name="scheduler">The scheduler.</param>
    /// <returns>An Signals.</returns>
    public static IObservable<T> Empty<T>(IScheduler scheduler)
    {
        if (scheduler == Scheduler.Immediate)
        {
            return ImmutableEmptySignal<T>.Instance;
        }

        return new EmptySignal<T>(scheduler);
    }

    /// <summary>
    /// Empty Signals. Returns only OnCompleted on specified scheduler. witness is for type inference.
    /// </summary>
    /// <typeparam name="T">The Type.</typeparam>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="witness">The witness.</param>
    /// <returns>An Signals.</returns>
#pragma warning disable RCS1163 // Unused parameter.
    public static IObservable<T> Empty<T>(IScheduler scheduler, T witness) =>
        Empty<T>(scheduler);
#pragma warning restore RCS1163 // Unused parameter.

    /// <summary>
    /// Empty Signals. Returns only OnCompleted.
    /// </summary>
    /// <typeparam name="T">The Type.</typeparam>
    /// <returns>An Signals.</returns>
    public static IObservable<T> Empty<T>() =>
        Empty<T>(Scheduler.Immediate);

    /// <summary>
    /// Empty Signals. Returns only OnCompleted. witness is for type inference.
    /// </summary>
    /// <typeparam name="T">The Type.</typeparam>
    /// <param name="witness">The witness.</param>
    /// <returns>An Signals.</returns>
#pragma warning disable RCS1163 // Unused parameter.
    public static IObservable<T> Empty<T>(T witness) =>
        Empty<T>(Scheduler.Immediate);
#pragma warning restore RCS1163 // Unused parameter.
}
