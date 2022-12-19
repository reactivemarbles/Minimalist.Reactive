// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Minimalist.Reactive.Core;
using Minimalist.Reactive.Disposables;

namespace Minimalist.Reactive.Observables.Core;

internal class ImmediateReturnObservable<T> : IObservable<T>, IRequireCurrentThread<T>
{
    private readonly T _value;

    public ImmediateReturnObservable(T value) => _value = value;

    public bool IsRequiredSubscribeOnCurrentThread()
    {
        return false;
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        observer.OnNext(_value);
        observer.OnCompleted();
        return Disposable.Empty;
    }
}
