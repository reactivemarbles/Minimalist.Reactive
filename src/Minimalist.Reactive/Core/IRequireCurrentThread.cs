﻿// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Minimalist.Reactive.Core;

/// <summary>
/// IRequireCurrentThread.
/// </summary>
/// <typeparam name="T">The Type.</typeparam>
public interface IRequireCurrentThread<T> : IObservable<T>
{
    /// <summary>
    /// Determines whether [is required subscribe on current thread].
    /// </summary>
    /// <returns>
    ///   <c>true</c> if [is required subscribe on current thread]; otherwise, <c>false</c>.
    /// </returns>
    bool IsRequiredSubscribeOnCurrentThread();
}
