﻿// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Minimalist.Reactive
{
    /// <summary>
    /// ISubject.
    /// </summary>
    /// <typeparam name="T">The Type.</typeparam>
    public interface ISignal<T> : ISignal<T, T>
    {
    }
}
