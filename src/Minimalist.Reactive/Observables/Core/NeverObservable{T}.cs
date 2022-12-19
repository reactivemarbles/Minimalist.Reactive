// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Minimalist.Reactive.Disposables;

namespace Minimalist.Reactive.Observables.Core;

internal class NeverObservable<T> : ObservableBase<T>
{
    public NeverObservable()
        : base(false)
    {
    }

    protected override IDisposable SubscribeCore(IObserver<T> observer, IDisposable cancel) =>
        Disposable.Empty;
}
