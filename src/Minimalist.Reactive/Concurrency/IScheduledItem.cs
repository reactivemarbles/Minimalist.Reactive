﻿// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Minimalist.Reactive.Concurrency;

/// <summary>
/// Represents a work item that has been scheduled.
/// </summary>
/// <typeparam name="TAbsolute">Absolute time representation type.</typeparam>
public interface IScheduledItem<TAbsolute>
{
    /// <summary>
    /// Gets the absolute time at which the item is due for invocation.
    /// </summary>
    TAbsolute DueTime { get; }

    /// <summary>
    /// Invokes the work item.
    /// </summary>
    void Invoke();
}
