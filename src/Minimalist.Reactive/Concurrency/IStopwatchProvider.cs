﻿// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Minimalist.Reactive.Concurrency;

/// <summary>
/// Provider for <see cref="IStopwatch"/> objects.
/// </summary>
public interface IStopwatchProvider
{
    /// <summary>
    /// Starts a new stopwatch object.
    /// </summary>
    /// <returns>New stopwatch object; started at the time of the request.</returns>
    IStopwatch StartStopwatch();
}
