// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Minimalist.Reactive.Core;

internal sealed class ThrowObserver<T> : IObserver<T>
{
    public static readonly ThrowObserver<T> Instance = new();

    private ThrowObserver()
    {
    }

    public void OnCompleted()
    {
    }

    public void OnError(Exception error) => error.Rethrow();

    public void OnNext(T value)
    {
    }
}
