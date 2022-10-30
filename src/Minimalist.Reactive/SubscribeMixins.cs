// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Runtime.ExceptionServices;

namespace Minimalist.Reactive
{
    /// <summary>
    /// SubscribeMixins.
    /// </summary>
    public static class SubscribeMixins
    {
        private static readonly Action<Exception> rethrow = e => ExceptionDispatchInfo.Capture(e).Throw();
        private static readonly Action nop = () => { };

        /// <summary>
        /// Subscribes to the observable providing just the <paramref name="onNext" /> delegate.
        /// </summary>
        /// <typeparam name="T">The Type.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="onNext">The on next.</param>
        /// <returns>A IDisposable.</returns>
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext)
            => Subscribe(source, onNext, rethrow, nop);

        /// <summary>
        /// Subscribes to the observable providing both the <paramref name="onNext" /> and
        /// <paramref name="onError" /> delegates.
        /// </summary>
        /// <typeparam name="T">The Type.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="onNext">The on next.</param>
        /// <param name="onError">The on error.</param>
        /// <returns>A IDisposable.</returns>
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError)
            => Subscribe(source, onNext, onError, nop);

        /// <summary>
        /// Subscribes to the observable providing both the <paramref name="onNext" /> and
        /// <paramref name="onCompleted" /> delegates.
        /// </summary>
        /// <typeparam name="T">The Type.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="onNext">The on next.</param>
        /// <param name="onCompleted">The on completed.</param>
        /// <returns>A IDisposable.</returns>
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action onCompleted)
            => Subscribe(source, onNext, rethrow, onCompleted);

        /// <summary>
        /// Subscribes to the observable providing all three <paramref name="onNext" />,
        /// <paramref name="onError" /> and <paramref name="onCompleted" /> delegates.
        /// </summary>
        /// <typeparam name="T">The Type.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="onNext">The on next.</param>
        /// <param name="onError">The on error.</param>
        /// <param name="onCompleted">The on completed.</param>
        /// <returns>A IDisposable.</returns>
        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError, Action onCompleted)
            => source?.Subscribe(new EmptyObserver<T>(onNext, onError, onCompleted))!;
    }
}
