// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Minimalist.Reactive.Core;
using Minimalist.Reactive.Disposables;

namespace Minimalist.Reactive.Observables.Core;

internal class ImmutableReturnInt32Observable : IObservable<int>, IRequireCurrentThread<int>
{
    private static readonly ImmutableReturnInt32Observable[] Caches = new ImmutableReturnInt32Observable[]
    {
            new ImmutableReturnInt32Observable(-1),
            new ImmutableReturnInt32Observable(0),
            new ImmutableReturnInt32Observable(1),
            new ImmutableReturnInt32Observable(2),
            new ImmutableReturnInt32Observable(3),
            new ImmutableReturnInt32Observable(4),
            new ImmutableReturnInt32Observable(5),
            new ImmutableReturnInt32Observable(6),
            new ImmutableReturnInt32Observable(7),
            new ImmutableReturnInt32Observable(8),
            new ImmutableReturnInt32Observable(9),
    };

    private readonly int _x;

    internal ImmutableReturnInt32Observable(int x) => _x = x;

    public static IObservable<int> GetInt32Observable(int x)
    {
        if (x >= -1 && x <= 9)
        {
            return Caches[x + 1];
        }

        return new ImmediateReturnObservable<int>(x);
    }

    public bool IsRequiredSubscribeOnCurrentThread() => false;

    public IDisposable Subscribe(IObserver<int> observer)
    {
        observer.OnNext(_x);
        observer.OnCompleted();
        return Disposable.Empty;
    }
}
