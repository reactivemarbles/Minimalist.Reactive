// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Minimalist.Reactive
{
    /// <summary>
    /// ISubject.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <seealso cref="System.IObserver&lt;TSource&gt;" />
    /// <seealso cref="System.IObservable&lt;TResult&gt;" />
    public interface ISignal<in TSource, out TResult> : IObserver<TSource>, IObservable<TResult>, IsDisposed
    {
        /// <summary>
        /// Gets a value indicating whether this instance has observers.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has observers; otherwise, <c>false</c>.
        /// </value>
        bool HasObservers { get; }
    }
}
