// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Minimalist.Reactive.Core;
using Minimalist.Reactive.Disposables;

namespace Minimalist.Reactive.Observables.Core;

internal sealed class ImmutableReturnRxVoidObservable : IObservable<RxVoid>, IRequireCurrentThread<RxVoid>
{
#pragma warning disable SA1401 // Fields should be private
    internal static ImmutableReturnRxVoidObservable Instance = new();
#pragma warning restore SA1401 // Fields should be private

    private ImmutableReturnRxVoidObservable()
    {
    }

    public bool IsRequiredSubscribeOnCurrentThread() => false;

    public IDisposable Subscribe(IObserver<RxVoid> observer)
    {
        observer.OnNext(RxVoid.Default);
        observer.OnCompleted();
        return Disposable.Empty;
    }
}
