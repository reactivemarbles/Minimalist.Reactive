// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Minimalist.Reactive.Signals;

internal class SelectSignal<TSource, TResult> : Signal<TResult>
{
    private Func<TSource, TResult>? _selector;
    private IDisposable? _subscription;

    public SelectSignal(IObservable<TSource> source, Func<TSource, TResult> selector)
    {
        _selector = selector;
        _subscription = source.Subscribe(
            next =>
            {
                if (IsDisposed)
                {
                    return;
                }

                OnNext(_selector(next));
            },
            OnError,
            OnCompleted);
    }

    protected override void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        Dispose(disposing);
        if (disposing)
        {
            _subscription?.Dispose();
            _subscription = null;
            _selector = null;
        }
    }
}
