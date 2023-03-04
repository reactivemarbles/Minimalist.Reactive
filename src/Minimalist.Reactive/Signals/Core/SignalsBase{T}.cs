﻿// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Minimalist.Reactive.Concurrency;
using Minimalist.Reactive.Core;
using Minimalist.Reactive.Disposables;

namespace Minimalist.Reactive.Signals.Core;

internal abstract class SignalsBase<T> : IRequireCurrentThread<T>
{
    private readonly bool _isRequiredSubscribeOnCurrentThread;

    internal SignalsBase(bool isRequiredSubscribeOnCurrentThread) =>
        _isRequiredSubscribeOnCurrentThread = isRequiredSubscribeOnCurrentThread;

    public bool IsRequiredSubscribeOnCurrentThread() => _isRequiredSubscribeOnCurrentThread;

    public IDisposable Subscribe(IObserver<T> observer)
    {
        if (observer == null)
        {
            throw new ArgumentNullException(nameof(observer));
        }

        var subscription = new SingleDisposable();

        if (_isRequiredSubscribeOnCurrentThread && Scheduler.CurrentThread.IsScheduleRequired)
        {
            Scheduler.CurrentThread.Schedule(() => subscription.Create(SubscribeCore(observer, subscription)));
        }
        else
        {
            subscription.Create(SubscribeCore(observer, subscription));
        }

        return subscription;
    }

    protected abstract IDisposable SubscribeCore(IObserver<T> observer, IDisposable cancel);
}
