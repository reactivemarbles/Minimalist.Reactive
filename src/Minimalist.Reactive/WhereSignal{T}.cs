// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Minimalist.Reactive
{
    internal class WhereSignal<T> : Signal<T>
    {
        private Func<T, bool>? _predicate;
        private IDisposable? _subscription;

        public WhereSignal(IObservable<T> source, Func<T, bool> predicate)
        {
            _predicate = predicate;
            _subscription = source.Subscribe(
                next =>
                {
                    if (IsDisposed)
                    {
                        return;
                    }

                    if (_predicate(next))
                    {
                        OnNext(next);
                    }
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
                _predicate = null;
            }
        }
    }
}
