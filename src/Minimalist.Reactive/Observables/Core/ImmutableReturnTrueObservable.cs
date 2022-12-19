// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Minimalist.Reactive.Core;
using Minimalist.Reactive.Disposables;

namespace Minimalist.Reactive.Observables.Core;

internal sealed class ImmutableReturnTrueObservable : IObservable<bool>, IRequireCurrentThread<bool>
{
#pragma warning disable SA1401 // Fields should be private
    internal static ImmutableReturnTrueObservable Instance = new ImmutableReturnTrueObservable();
#pragma warning restore SA1401 // Fields should be private

    private ImmutableReturnTrueObservable()
    {
    }

    public bool IsRequiredSubscribeOnCurrentThread() => false;

    public IDisposable Subscribe(IObserver<bool> observer)
    {
        observer.OnNext(true);
        observer.OnCompleted();
        return Disposable.Empty;
    }
}
