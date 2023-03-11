﻿// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Minimalist.Reactive.Signals;

/// <summary>
/// ISubject.
/// </summary>
/// <typeparam name="T">The Type.</typeparam>
public interface ISignal<T> : ISignal<T, T>
{
}
