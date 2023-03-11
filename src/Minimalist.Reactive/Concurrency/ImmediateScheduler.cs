// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Minimalist.Reactive.Concurrency;

/// <summary>
/// ImmediateScheduler.
/// </summary>
/// <seealso cref="Minimalist.Reactive.Concurrency.IScheduler" />
public sealed class ImmediateScheduler : IScheduler
{
    private static readonly Lazy<ImmediateScheduler> StaticInstance = new(static () => new ImmediateScheduler());

    private ImmediateScheduler()
    {
    }

    /// <summary>
    /// Gets the singleton instance of the immediate scheduler.
    /// </summary>
    public static ImmediateScheduler Instance => StaticInstance.Value;

    /// <summary>
    /// Gets the scheduler's notion of current time.
    /// </summary>
    public DateTimeOffset Now => DateTimeOffset.UtcNow;

    /// <summary>
    /// Schedules the specified state.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="state">The state.</param>
    /// <param name="action">The action.</param>
    /// <returns>An IDisposable.</returns>
    public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return action(this, state);
    }

    /// <summary>
    /// Schedules the specified state.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="state">The state.</param>
    /// <param name="dueTime">The due time.</param>
    /// <param name="action">The action.</param>
    /// <returns>An IDisposable.</returns>
    public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var dt = Scheduler.Normalize(dueTime);
        if (dt.Ticks > 0)
        {
            Thread.Sleep(dt);
        }

        return action(this, state);
    }

    /// <summary>
    /// Schedules the specified state.
    /// </summary>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="state">The state.</param>
    /// <param name="dueTime">The due time.</param>
    /// <param name="action">The action.</param>
    /// <returns>An IDisposable.</returns>
    public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
    {
        var due = Scheduler.Normalize(dueTime - Now);
        return Schedule(state, TimeSpan.Zero, action);
    }
}
