// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;

namespace Minimalist.Reactive.Concurrency;

internal sealed class EmptyScheduler : IScheduler
{
    public static readonly EmptyScheduler Instance = new();

    public DateTimeOffset Now => DateTimeOffset.MinValue;

    public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action) =>
        throw new NotImplementedException();

    public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action) =>
        throw new NotImplementedException();

    public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action) =>
        throw new NotImplementedException();
}
