﻿// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Minimalist.Reactive.Core;
using Minimalist.Reactive.Disposables;

namespace Minimalist.Reactive.Signals.Core;

internal class ImmutableNeverSignal<T> : IRequireCurrentThread<T>
{
#pragma warning disable SA1401 // Fields should be private
    internal static ImmutableNeverSignal<T> Instance = new();
#pragma warning restore SA1401 // Fields should be private

    public bool IsRequiredSubscribeOnCurrentThread() => false;

    public IDisposable Subscribe(IObserver<T> observer) =>
        Disposable.Empty;
}
