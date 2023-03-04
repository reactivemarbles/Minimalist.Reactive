// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Minimalist.Reactive.Core;
using Minimalist.Reactive.Disposables;

namespace Minimalist.Reactive.Signals.Core;

internal sealed class ImmutableReturnTrueSignal : IObservable<bool>, IRequireCurrentThread<bool>
{
#pragma warning disable SA1401 // Fields should be private
    internal static ImmutableReturnTrueSignal Instance = new ImmutableReturnTrueSignal();
#pragma warning restore SA1401 // Fields should be private

    private ImmutableReturnTrueSignal()
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
