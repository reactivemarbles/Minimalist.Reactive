// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Minimalist.Reactive.Core;

internal sealed class DisposedObserver<T> : IObserver<T>
{
    public static readonly DisposedObserver<T> Instance = new();

    private DisposedObserver()
    {
    }

    public void OnCompleted() => throw new ObjectDisposedException(string.Empty);

    public void OnError(Exception error) => throw new ObjectDisposedException(string.Empty, error);

    public void OnNext(T value) => throw new ObjectDisposedException(string.Empty);
}
