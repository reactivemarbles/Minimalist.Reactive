﻿// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Minimalist.Reactive.Core;

namespace Minimalist.Reactive.Observables.Core
{
    internal abstract class ObserverBase<TSource, TResult> : IDisposable, IObserver<TSource>
    {
#pragma warning disable SA1401 // Fields should be private
        protected internal volatile IObserver<TResult> Observer;
#pragma warning restore SA1401 // Fields should be private
        private IDisposable? _cancel;

        internal ObserverBase(IObserver<TResult> observer, IDisposable cancel)
        {
            _cancel = cancel ?? throw new ArgumentNullException(nameof(cancel));
            Observer = observer;
        }

        public abstract void OnNext(TSource value);

        public abstract void OnError(Exception error);

        public abstract void OnCompleted();

        public void Dispose()
        {
            Observer = EmptyObserver<TResult>.Instance;
            var target = Interlocked.Exchange(ref _cancel, null);
            target?.Dispose();
        }
    }
}
