// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using Minimalist.Reactive.Disposables;
using static Minimalist.Reactive.Disposables.Disposable;

namespace Minimalist.Reactive.Concurrency;

/// <summary>
/// TaskPoolScheduler.
/// </summary>
/// <seealso cref="Minimalist.Reactive.Concurrency.IScheduler" />
public sealed class TaskPoolScheduler : IScheduler
{
    private readonly TaskFactory _taskFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskPoolScheduler"/> class.
    /// </summary>
    /// <param name="taskFactory">The task factory.</param>
    public TaskPoolScheduler(TaskFactory taskFactory) => _taskFactory = taskFactory;

    /// <summary>
    /// Gets the instance.
    /// </summary>
    /// <value>
    /// The instance.
    /// </value>
    public static TaskPoolScheduler Instance { get; } = new(Task.Factory);

    /// <summary>
    /// Gets the scheduler's notion of current time.
    /// </summary>
    public DateTimeOffset Now => Scheduler.Now;

    /// <summary>
    /// Schedules an action to be executed.
    /// </summary>
    /// <typeparam name="TState">The type of the state passed to the scheduled action.</typeparam>
    /// <param name="state">State passed to the action to be executed.</param>
    /// <param name="action">Action to be executed.</param>
    /// <returns>
    /// The disposable object used to cancel the scheduled action (best effort).
    /// </returns>
    public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var cancellationDisposable = new CancellationDisposable();
#pragma warning disable CA2008 // Do not create tasks without passing a TaskScheduler
        _taskFactory.StartNew(
             (_) =>
         {
             try
             {
                 return action(this, state);
             }
             catch (Exception ex)
             {
                 var thread = new Thread(() => throw ex.Rethrow());
                 thread.Start();
                 thread.Join();
                 return Disposable.Empty;
             }
         },
             cancellationDisposable.Token);
#pragma warning restore CA2008 // Do not create tasks without passing a TaskScheduler

        return cancellationDisposable;
    }

    /// <summary>
    /// Schedules an action to be executed after dueTime.
    /// </summary>
    /// <typeparam name="TState">The type of the state passed to the scheduled action.</typeparam>
    /// <param name="state">State passed to the action to be executed.</param>
    /// <param name="dueTime">Relative time after which to execute the action.</param>
    /// <param name="action">Action to be executed.</param>
    /// <returns>
    /// The disposable object used to cancel the scheduled action (best effort).
    /// </returns>
    public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var g = new MultipleDisposable(new IDisposable[0]);
        g.Add(ThreadPoolScheduler.Instance.Schedule(state, Scheduler.Normalize(dueTime), action));
        return g;
    }

    /// <summary>
    /// Schedules an action to be executed at dueTime.
    /// </summary>
    /// <typeparam name="TState">The type of the state passed to the scheduled action.</typeparam>
    /// <param name="state">State passed to the action to be executed.</param>
    /// <param name="dueTime">Absolute time at which to execute the action.</param>
    /// <param name="action">Action to be executed.</param>
    /// <returns>
    /// The disposable object used to cancel the scheduled action (best effort).
    /// </returns>
    public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action) =>
        Schedule(state, Scheduler.Normalize(dueTime - Now), action);
}
