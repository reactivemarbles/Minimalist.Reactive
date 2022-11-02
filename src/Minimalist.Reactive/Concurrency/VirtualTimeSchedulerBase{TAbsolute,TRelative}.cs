﻿// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;

namespace Minimalist.Reactive.Concurrency;

/// <summary>
/// Base class for virtual time schedulers.
/// </summary>
/// <typeparam name="TAbsolute">Absolute time representation type.</typeparam>
/// <typeparam name="TRelative">Relative time representation type.</typeparam>
public abstract class VirtualTimeSchedulerBase<TAbsolute, TRelative> : IScheduler, IServiceProvider, IStopwatchProvider
    where TAbsolute : IComparable<TAbsolute>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualTimeSchedulerBase{TAbsolute, TRelative}"/> class.
    /// Creates a new virtual time scheduler with the default value of TAbsolute as the initial clock value.
    /// </summary>
    protected VirtualTimeSchedulerBase()
        : this(default!, Comparer<TAbsolute>.Default)
    {
        ////
        //// NB: We allow a default value for TAbsolute here, which typically is a struct. For compat reasons, we can't
        ////     add a generic constraint (either struct or, better, new()), and maybe a derived class has handled null
        ////     in all abstract methods.
        ////
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="VirtualTimeSchedulerBase{TAbsolute, TRelative}"/> class.
    /// Creates a new virtual time scheduler with the specified initial clock value and absolute time comparer.
    /// </summary>
    /// <param name="initialClock">Initial value for the clock.</param>
    /// <param name="comparer">Comparer to determine causality of events based on absolute time.</param>
    /// <exception cref="ArgumentNullException"><paramref name="comparer"/> is <c>null</c>.</exception>
    protected VirtualTimeSchedulerBase(TAbsolute initialClock, IComparer<TAbsolute> comparer)
    {
        Clock = initialClock;
        Comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
    }

    /// <summary>
    /// Gets or sets the scheduler's absolute time clock value.
    /// </summary>
    public TAbsolute Clock
    {
        get;
        protected set;
    }

    /// <summary>
    /// Gets a value indicating whether gets whether the scheduler is enabled to run work.
    /// </summary>
    public bool IsEnabled { get; private set; }

    /// <summary>
    /// Gets the scheduler's notion of current time.
    /// </summary>
    public DateTimeOffset Now => ToDateTimeOffset(Clock);

    /// <summary>
    /// Gets the comparer used to compare absolute time values.
    /// </summary>
    protected IComparer<TAbsolute> Comparer { get; }

    /// <summary>
    /// Advances the scheduler's clock by the specified relative time, running all work scheduled for that timespan.
    /// </summary>
    /// <param name="time">Relative time to advance the scheduler's clock by.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
    /// <exception cref="InvalidOperationException">The scheduler is already running. VirtualTimeScheduler doesn't support running nested work dispatch loops. To simulate time slippage while running work on the scheduler, use <see cref="Sleep"/>.</exception>
    public void AdvanceBy(TRelative time)
    {
        var dt = Add(Clock, time);

        var dueToClock = Comparer.Compare(dt, Clock);
        if (dueToClock < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(time));
        }

        if (dueToClock == 0)
        {
            return;
        }

        if (!IsEnabled)
        {
            AdvanceTo(dt);
        }
        else
        {
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "{0} cannot be called when the scheduler is already running. Try using Sleep instead.", nameof(AdvanceBy)));
        }
    }

    /// <summary>
    /// Advances the scheduler's clock to the specified time, running all work till that point.
    /// </summary>
    /// <param name="time">Absolute time to advance the scheduler's clock to.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is in the past.</exception>
    /// <exception cref="InvalidOperationException">The scheduler is already running. VirtualTimeScheduler doesn't support running nested work dispatch loops. To simulate time slippage while running work on the scheduler, use <see cref="Sleep"/>.</exception>
    public void AdvanceTo(TAbsolute time)
    {
        var dueToClock = Comparer.Compare(time, Clock);
        if (dueToClock < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(time));
        }

        if (dueToClock == 0)
        {
            return;
        }

        if (!IsEnabled)
        {
            IsEnabled = true;
            do
            {
                var next = GetNext();
                if (next != null && Comparer.Compare(next.DueTime, time) <= 0)
                {
                    if (Comparer.Compare(next.DueTime, Clock) > 0)
                    {
                        Clock = next.DueTime;
                    }

                    next.Invoke();
                }
                else
                {
                    IsEnabled = false;
                }
            }
            while (IsEnabled);

            Clock = time;
        }
        else
        {
            throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "{0} cannot be called when the scheduler is already running. Try using Sleep instead.", nameof(AdvanceTo)));
        }
    }

    /// <summary>
    /// Gets the service object of the specified type.
    /// </summary>
    /// <param name="serviceType">An object that specifies the type of service object to get.</param>
    /// <returns>
    /// A service object of type <paramref name="serviceType" />.
    /// -or-
    /// <see langword="null" /> if there is no service object of type <paramref name="serviceType" />.
    /// </returns>
    object? IServiceProvider.GetService(Type serviceType) => GetService(serviceType);

    /// <summary>
    /// Schedules an action to be executed.
    /// </summary>
    /// <typeparam name="TState">The type of the state passed to the scheduled action.</typeparam>
    /// <param name="state">State passed to the action to be executed.</param>
    /// <param name="action">Action to be executed.</param>
    /// <returns>The disposable object used to cancel the scheduled action (best effort).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
    public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return ScheduleAbsolute(state, Clock, action);
    }

    /// <summary>
    /// Schedules an action to be executed after dueTime.
    /// </summary>
    /// <typeparam name="TState">The type of the state passed to the scheduled action.</typeparam>
    /// <param name="state">State passed to the action to be executed.</param>
    /// <param name="dueTime">Relative time after which to execute the action.</param>
    /// <param name="action">Action to be executed.</param>
    /// <returns>The disposable object used to cancel the scheduled action (best effort).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
    public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return ScheduleRelative(state, ToRelative(dueTime), action);
    }

    /// <summary>
    /// Schedules an action to be executed at dueTime.
    /// </summary>
    /// <typeparam name="TState">The type of the state passed to the scheduled action.</typeparam>
    /// <param name="state">State passed to the action to be executed.</param>
    /// <param name="dueTime">Absolute time at which to execute the action.</param>
    /// <param name="action">Action to be executed.</param>
    /// <returns>The disposable object used to cancel the scheduled action (best effort).</returns>
    /// <exception cref="ArgumentNullException"><paramref name="action"/> is <c>null</c>.</exception>
    public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return ScheduleRelative(state, ToRelative(dueTime - Now), action);
    }

    /// <summary>
    /// Schedules an action to be executed at dueTime.
    /// </summary>
    /// <typeparam name="TState">The type of the state passed to the scheduled action.</typeparam>
    /// <param name="state">State passed to the action to be executed.</param>
    /// <param name="dueTime">Absolute time at which to execute the action.</param>
    /// <param name="action">Action to be executed.</param>
    /// <returns>The disposable object used to cancel the scheduled action (best effort).</returns>
    public abstract IDisposable ScheduleAbsolute<TState>(TState state, TAbsolute dueTime, Func<IScheduler, TState, IDisposable> action);

    /// <summary>
    /// Schedules an action to be executed at dueTime.
    /// </summary>
    /// <typeparam name="TState">The type of the state passed to the scheduled action.</typeparam>
    /// <param name="state">State passed to the action to be executed.</param>
    /// <param name="dueTime">Relative time after which to execute the action.</param>
    /// <param name="action">Action to be executed.</param>
    /// <returns>The disposable object used to cancel the scheduled action (best effort).</returns>
    public IDisposable ScheduleRelative<TState>(TState state, TRelative dueTime, Func<IScheduler, TState, IDisposable> action)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        var runAt = Add(Clock, dueTime);

        return ScheduleAbsolute(state, runAt, action);
    }

    /// <summary>
    /// Advances the scheduler's clock by the specified relative time.
    /// </summary>
    /// <param name="time">Relative time to advance the scheduler's clock by.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
    public void Sleep(TRelative time)
    {
        var dt = Add(Clock, time);

        var dueToClock = Comparer.Compare(dt, Clock);
        if (dueToClock < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(time));
        }

        Clock = dt;
    }

    /// <summary>
    /// Starts the virtual time scheduler.
    /// </summary>
    public void Start()
    {
        if (!IsEnabled)
        {
            IsEnabled = true;
            do
            {
                var next = GetNext();
                if (next != null)
                {
                    if (Comparer.Compare(next.DueTime, Clock) > 0)
                    {
                        Clock = next.DueTime;
                    }

                    next.Invoke();
                }
                else
                {
                    IsEnabled = false;
                }
            }
            while (IsEnabled);
        }
    }

    /// <summary>
    /// Starts a new stopwatch object.
    /// </summary>
    /// <returns>New stopwatch object; started at the time of the request.</returns>
    public IStopwatch StartStopwatch()
    {
        var start = ClockToDateTimeOffset();
        return new VirtualTimeStopwatch(this, start);
    }

    /// <summary>
    /// Stops the virtual time scheduler.
    /// </summary>
    public void Stop()
    {
        IsEnabled = false;
    }

    /// <summary>
    /// Adds a relative time value to an absolute time value.
    /// </summary>
    /// <param name="absolute">Absolute time value.</param>
    /// <param name="relative">Relative time value to add.</param>
    /// <returns>The resulting absolute time sum value.</returns>
    protected abstract TAbsolute Add(TAbsolute absolute, TRelative relative);

    /// <summary>
    /// Gets the next scheduled item to be executed.
    /// </summary>
    /// <returns>The next scheduled item.</returns>
    protected abstract IScheduledItem<TAbsolute>? GetNext();

    /// <summary>
    /// Discovers scheduler services by interface type. The base class implementation supports
    /// only the IStopwatchProvider service. To influence service discovery - such as adding
    /// support for other scheduler services - derived types can override this method.
    /// </summary>
    /// <param name="serviceType">Scheduler service interface type to discover.</param>
    /// <returns>Object implementing the requested service, if available; null otherwise.</returns>
    protected virtual object? GetService(Type serviceType)
    {
        if (serviceType == typeof(IStopwatchProvider))
        {
            return this;
        }

        return null;
    }

    /// <summary>
    /// Converts the absolute time value to a DateTimeOffset value.
    /// </summary>
    /// <param name="absolute">Absolute time value to convert.</param>
    /// <returns>The corresponding DateTimeOffset value.</returns>
    protected abstract DateTimeOffset ToDateTimeOffset(TAbsolute absolute);

    /// <summary>
    /// Converts the TimeSpan value to a relative time value.
    /// </summary>
    /// <param name="timeSpan">TimeSpan value to convert.</param>
    /// <returns>The corresponding relative time value.</returns>
    protected abstract TRelative ToRelative(TimeSpan timeSpan);

    private DateTimeOffset ClockToDateTimeOffset() => ToDateTimeOffset(Clock);

    private sealed class VirtualTimeStopwatch : IStopwatch
    {
        private readonly VirtualTimeSchedulerBase<TAbsolute, TRelative> _parent;
        private readonly DateTimeOffset _start;

        public VirtualTimeStopwatch(VirtualTimeSchedulerBase<TAbsolute, TRelative> parent, DateTimeOffset start)
        {
            _parent = parent;
            _start = start;
        }

        public TimeSpan Elapsed => _parent.ClockToDateTimeOffset() - _start;
    }
}
