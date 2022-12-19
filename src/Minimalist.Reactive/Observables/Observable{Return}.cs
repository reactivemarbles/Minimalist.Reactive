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
    /// Return single sequence on specified scheduler.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="value">The value.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <returns>An Observable.</returns>
    public static IObservable<T> Return<T>(T value, IScheduler scheduler)
    {
        if (scheduler == Scheduler.Immediate)
        {
            return new ImmediateReturnObservable<T>(value);
        }

        return new ReturnObservable<T>(value, scheduler);
    }

    /// <summary>
    /// Return single sequence Immediately.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>An Observable.</returns>
    public static IObservable<T> Return<T>(T value) =>
        Return<T>(value, Scheduler.Immediate);

    /// <summary>
    /// Return single sequence Immediately, optimized for RxVoid(no allocate memory).
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>An Observable.</returns>
#pragma warning disable RCS1163 // Unused parameter.
    public static IObservable<RxVoid> Return(RxVoid value) =>
        ImmutableReturnRxVoidObservable.Instance;
#pragma warning restore RCS1163 // Unused parameter.

    /// <summary>
    /// Return single sequence Immediately, optimized for Boolean(no allocate memory).
    /// </summary>
    /// <param name="value">if set to <c>true</c> [value].</param>
    /// <returns>An Observable.</returns>
    public static IObservable<bool> Return(bool value) => value
            ? ImmutableReturnTrueObservable.Instance
            : ImmutableReturnFalseObservable.Instance;

    /// <summary>
    /// Return single sequence Immediately, optimized for Int32.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>An Observable.</returns>
    public static IObservable<int> Return(int value) =>
        ImmutableReturnInt32Observable.GetInt32Observable(value);

    /// <summary>
    /// Same as Observable.Return(RxVoid.Default); but no allocate memory.
    /// </summary>
    /// <returns>An Observable.</returns>
    public static IObservable<RxVoid> ReturnRxVoid() =>
        ImmutableReturnRxVoidObservable.Instance;
}
