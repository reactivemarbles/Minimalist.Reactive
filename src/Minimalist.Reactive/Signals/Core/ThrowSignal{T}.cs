// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Minimalist.Reactive.Concurrency;
using Minimalist.Reactive.Disposables;

namespace Minimalist.Reactive.Signals.Core;

internal class ThrowSignal<T> : SignalsBase<T>
{
    private readonly Exception _error;
    private readonly IScheduler _scheduler;

    public ThrowSignal(Exception error, IScheduler scheduler)
        : base(scheduler == Scheduler.CurrentThread)
    {
        _error = error;
        _scheduler = scheduler;
    }

    protected override IDisposable SubscribeCore(IObserver<T> observer, IDisposable cancel)
    {
        observer = new Throw(observer, cancel);

        if (_scheduler == Scheduler.Immediate)
        {
            observer.OnError(_error);
            return Disposable.Empty;
        }

        return _scheduler.Schedule(() =>
        {
            observer.OnError(_error);
            observer.OnCompleted();
        });
    }

    private class Throw : WitnessBase<T, T>
    {
        public Throw(IObserver<T> observer, IDisposable cancel)
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
                Dispose();
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
