// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Minimalist.Reactive.Concurrency;
using Minimalist.Reactive.Disposables;

namespace Minimalist.Reactive.Signals.Core;

internal class EmptySignal<T> : SignalsBase<T>
{
    private readonly IScheduler _scheduler;

    public EmptySignal(IScheduler scheduler)
        : base(false) => _scheduler = scheduler;

    protected override IDisposable SubscribeCore(IObserver<T> observer, IDisposable cancel)
    {
        observer = new Empty(observer, cancel);

        if (_scheduler == Scheduler.Immediate)
        {
            observer.OnCompleted();
            return Disposable.Empty;
        }

        return _scheduler.Schedule(observer.OnCompleted);
    }

    private class Empty : WitnessBase<T, T>
    {
        public Empty(IObserver<T> observer, IDisposable cancel)
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
