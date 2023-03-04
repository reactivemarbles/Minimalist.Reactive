﻿// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Minimalist.Reactive.Disposables;

namespace Minimalist.Reactive.Signals.Core;
internal class CreateSafeSignal<T> : SignalsBase<T>
{
    private readonly Func<IObserver<T>, IDisposable> _subscribe;

    public CreateSafeSignal(Func<IObserver<T>, IDisposable> subscribe)
        : base(true) => _subscribe = subscribe; // fail safe

    public CreateSafeSignal(Func<IObserver<T>, IDisposable> subscribe, bool isRequiredSubscribeOnCurrentThread)
        : base(isRequiredSubscribeOnCurrentThread) => _subscribe = subscribe;

    protected override IDisposable SubscribeCore(IObserver<T> observer, IDisposable cancel)
    {
        observer = new CreateSafe(observer, cancel);
        return _subscribe(observer) ?? Disposable.Empty;
    }

    private class CreateSafe : WitnessBase<T, T>
    {
        public CreateSafe(IObserver<T> observer, IDisposable cancel)
            : base(observer, cancel)
        {
        }

        public override void OnNext(T value)
        {
            try
            {
                Observer.OnNext(value);
            }
            catch
            {
                Dispose(); // safe
                throw;
            }
        }

        public override void OnError(Exception error)
        {
            try
            {
                Observer.OnError(error);
            }
            finally
            {
                Dispose();
            }
        }

        public override void OnCompleted()
        {
            try
            {
                Observer.OnCompleted();
            }
            finally
            {
                Dispose();
            }
        }
    }
}
