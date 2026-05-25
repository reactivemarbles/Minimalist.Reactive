// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Minimalist.Reactive.Concurrency;
using Minimalist.Reactive.Disposables;
using Minimalist.Reactive.Signals.Core;

#pragma warning disable SA1107, SA1116, SA1117, SA1501, SA1611, SA1615, SA1618

namespace Minimalist.Reactive.Signals;

/// <summary>
/// Additional Minimalist.Reactive factory surface for finite, resource, conversion, and time signals.
/// </summary>
public static partial class Signal
{
    /// <summary>
    /// Creates a finite integer signal from <paramref name="start"/> for <paramref name="count"/> values.
    /// </summary>
    public static IObservable<int> Range(int start, int count) =>
        Range(start, count, Scheduler.CurrentThread);

    /// <summary>
    /// Creates a finite integer signal from <paramref name="start"/> for <paramref name="count"/> values on <paramref name="scheduler"/>.
    /// </summary>
    public static IObservable<int> Range(int start, int count, IScheduler scheduler)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (scheduler == null)
        {
            throw new ArgumentNullException(nameof(scheduler));
        }

        if (count == 0)
        {
            return Empty<int>();
        }

        if (scheduler == Scheduler.Immediate || scheduler == Scheduler.CurrentThread)
        {
            return new RangeSignal(start, count);
        }

        return CreateSafe<int>(observer => scheduler.Schedule(() =>
        {
            for (var i = 0; i < count; i++)
            {
                observer.OnNext(start + i);
            }

            observer.OnCompleted();
        }), scheduler == Scheduler.CurrentThread);
    }

    /// <summary>
    /// Creates a signal that repeats a value forever.
    /// </summary>
    public static IObservable<T> Repeat<T>(T value) =>
        Create<T>(observer => Scheduler.CurrentThread.Schedule(self =>
        {
            observer.OnNext(value);
            self();
        }), true);

    /// <summary>
    /// Creates a signal that repeats a value <paramref name="count"/> times.
    /// </summary>
    public static IObservable<T> Repeat<T>(T value, int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        if (count == 0)
        {
            return Empty<T>();
        }

        return new RepeatSignal<T>(value, count);
    }

    /// <summary>
    /// Unfolds state into a finite signal.
    /// </summary>
    public static IObservable<TResult> Unfold<TState, TResult>(
        TState initialState,
        Func<TState, bool> condition,
        Func<TState, TState> iterate,
        Func<TState, TResult> resultSelector)
    {
        if (condition == null)
        {
            throw new ArgumentNullException(nameof(condition));
        }

        if (iterate == null)
        {
            throw new ArgumentNullException(nameof(iterate));
        }

        if (resultSelector == null)
        {
            throw new ArgumentNullException(nameof(resultSelector));
        }

        return CreateSafe<TResult>(observer => Scheduler.CurrentThread.Schedule(() =>
        {
            var state = initialState;
            while (condition(state))
            {
                observer.OnNext(resultSelector(state));
                state = iterate(state);
            }

            observer.OnCompleted();
        }), true);
    }

    /// <summary>
    /// Creates a signal whose subscription lifetime owns a resource.
    /// </summary>
    public static IObservable<T> Use<TResource, T>(Func<TResource> resourceFactory, Func<TResource, IObservable<T>> signalFactory)
        where TResource : IDisposable
    {
        if (resourceFactory == null)
        {
            throw new ArgumentNullException(nameof(resourceFactory));
        }

        if (signalFactory == null)
        {
            throw new ArgumentNullException(nameof(signalFactory));
        }

        return Create<T>(observer =>
        {
            TResource resource;
            IObservable<T> source;
            try
            {
                resource = resourceFactory();
                source = signalFactory(resource) ?? throw new InvalidOperationException("The signal factory returned null.");
            }
            catch (Exception error)
            {
                observer.OnError(error);
                return Disposable.Empty;
            }

            var sourceSubscription = source.Subscribe(observer);
            return MultipleDisposable.Create(sourceSubscription, resource);
        });
    }

    /// <summary>
    /// Creates a signal from an enumerable sequence.
    /// </summary>
    public static IObservable<T> FromEnumerable<T>(IEnumerable<T> values)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        return CreateSafe<T>(observer =>
        {
            using (var enumerator = values.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    observer.OnNext(enumerator.Current);
                }
            }

            observer.OnCompleted();
            return Disposable.Empty;
        });
    }

    /// <summary>
    /// Creates a signal from a task instance.
    /// </summary>
    public static IObservable<T> FromTask<T>(Task<T> task)
    {
        if (task == null)
        {
            throw new ArgumentNullException(nameof(task));
        }

        if (task.Status == TaskStatus.RanToCompletion)
        {
            return Return(task.Result);
        }

        if (task.IsCanceled)
        {
            return Throw<T>(new TaskCanceledException(task));
        }

        if (task.IsFaulted)
        {
            return Throw<T>(task.Exception!.InnerException ?? task.Exception);
        }

        return CreateSafe<T>(observer =>
        {
            var disposed = 0;
            task.ContinueWith(completed =>
            {
                if (Volatile.Read(ref disposed) != 0)
                {
                    return;
                }

                if (completed.IsCanceled)
                {
                    observer.OnError(new TaskCanceledException(completed));
                }
                else if (completed.IsFaulted)
                {
                    observer.OnError(completed.Exception!.InnerException ?? completed.Exception);
                }
                else
                {
                    observer.OnNext(completed.Result);
                    observer.OnCompleted();
                }
            }, TaskScheduler.Default);

            return Disposable.Create(() => Volatile.Write(ref disposed, 1));
        });
    }

    /// <summary>
    /// Runs a function on the supplied scheduler and emits its result.
    /// </summary>
    public static IObservable<T> Start<T>(Func<T> function, IScheduler? scheduler = null)
    {
        if (function == null)
        {
            throw new ArgumentNullException(nameof(function));
        }

        scheduler ??= Scheduler.Default;
        return CreateSafe<T>(observer => scheduler.Schedule(() =>
        {
            try
            {
                observer.OnNext(function());
                observer.OnCompleted();
            }
            catch (Exception error)
            {
                observer.OnError(error);
            }
        }), scheduler == Scheduler.CurrentThread);
    }

    /// <summary>
    /// Runs an action on the supplied scheduler and emits <see cref="RxVoid.Default"/> when it completes.
    /// </summary>
    public static IObservable<RxVoid> Start(Action action, IScheduler? scheduler = null)
    {
        if (action == null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        return Start(
            () =>
            {
                action();
                return RxVoid.Default;
            },
            scheduler);
    }

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Creates a signal from an async enumerable sequence and cancels enumeration when disposed.
    /// </summary>
    public static IObservable<T> FromAsyncEnumerable<T>(IAsyncEnumerable<T> values, CancellationToken cancellationToken = default)
    {
        if (values == null)
        {
            throw new ArgumentNullException(nameof(values));
        }

        return CreateSafe<T>(observer =>
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            IAsyncEnumerator<T>? enumerator = null;
            _ = Task.Run(async () =>
            {
                try
                {
                    enumerator = values.GetAsyncEnumerator(cts.Token);
                    while (!cts.IsCancellationRequested && await enumerator.MoveNextAsync().ConfigureAwait(false))
                    {
                        observer.OnNext(enumerator.Current);
                    }

                    if (!cts.IsCancellationRequested)
                    {
                        observer.OnCompleted();
                    }
                }
                catch (OperationCanceledException) when (cts.IsCancellationRequested)
                {
                }
                catch (Exception error) when (!cts.IsCancellationRequested)
                {
                    observer.OnError(error);
                }
                finally
                {
                    if (enumerator != null)
                    {
                        await enumerator.DisposeAsync().ConfigureAwait(false);
                    }

                    cts.Dispose();
                }
            }, CancellationToken.None);

            return Disposable.Create(() =>
            {
                cts.Cancel();
                var current = Volatile.Read(ref enumerator);
                if (current != null)
                {
                    try
                    {
                        _ = current.DisposeAsync().AsTask();
                    }
                    catch (NotSupportedException)
                    {
                    }
                }
            });
        });
    }
#endif

    /// <summary>
    /// Emits a single zero tick after the due time.
    /// </summary>
    public static IObservable<long> After(TimeSpan dueTime, IScheduler? scheduler = null)
    {
        scheduler ??= ThreadPoolScheduler.Instance;
        return CreateSafe<long>(observer => scheduler.Schedule(Scheduler.Normalize(dueTime), () =>
        {
            observer.OnNext(0L);
            observer.OnCompleted();
        }), scheduler == Scheduler.CurrentThread);
    }

    /// <summary>
    /// Emits monotonically increasing ticks at the specified period.
    /// </summary>
    public static IObservable<long> Every(TimeSpan period, IScheduler? scheduler = null)
    {
        if (period < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(period));
        }

        scheduler ??= ThreadPoolScheduler.Instance;
        return CreateSafe<long>(observer =>
        {
            var slot = new SingleReplaceableDisposable();
            var tick = 0L;
            Action? scheduleNext = null;
            scheduleNext = () => slot.Create(scheduler.Schedule(period, () =>
            {
                observer.OnNext(tick++);
                if (!slot.IsDisposed)
                {
                    scheduleNext!();
                }
            }));

            scheduleNext();
            return slot;
        }, scheduler == Scheduler.CurrentThread);
    }

    /// <summary>
    /// Alias for <see cref="Every(TimeSpan, IScheduler?)"/>.
    /// </summary>
    public static IObservable<long> Pulse(TimeSpan period, IScheduler? scheduler = null) => Every(period, scheduler);

    /// <summary>
    /// Alias for <see cref="Every(TimeSpan, IScheduler?)"/>.
    /// </summary>
    public static IObservable<long> Interval(TimeSpan period, IScheduler? scheduler = null) => Every(period, scheduler);

    /// <summary>
    /// Alias for <see cref="After(TimeSpan, IScheduler?)"/>.
    /// </summary>
    public static IObservable<long> Timer(TimeSpan dueTime, IScheduler? scheduler = null) => After(dueTime, scheduler);

    /// <summary>
    /// Creates a timer that emits first after <paramref name="dueTime"/> and then at <paramref name="period"/>.
    /// </summary>
    public static IObservable<long> Timer(TimeSpan dueTime, TimeSpan period, IScheduler? scheduler = null)
    {
        scheduler ??= ThreadPoolScheduler.Instance;
        return CreateSafe<long>(observer =>
        {
            var pocket = new MultipleDisposable();
            var current = 0L;
            pocket.Add(scheduler.Schedule(Scheduler.Normalize(dueTime), () =>
            {
                observer.OnNext(current++);
                pocket.Add(Every(period, scheduler).Subscribe(value => observer.OnNext(current + value), observer.OnError, observer.OnCompleted));
            }));

            return pocket;
        }, scheduler == Scheduler.CurrentThread);
    }

    /// <summary>
    /// Concatenates the supplied signals.
    /// </summary>
    public static IObservable<T> Concat<T>(params IObservable<T>[] sources) =>
        Minimalist.Reactive.LinqMixins.Concat(FromEnumerable(sources));

    /// <summary>
    /// Merges the supplied signals.
    /// </summary>
    public static IObservable<T> Merge<T>(params IObservable<T>[] sources) =>
        Minimalist.Reactive.LinqMixins.Merge(FromEnumerable(sources));

    /// <summary>
    /// Races the supplied signals and mirrors the first one to produce a value or terminal signal.
    /// </summary>
    public static IObservable<T> Race<T>(params IObservable<T>[] sources) =>
        Minimalist.Reactive.LinqMixins.Race(FromEnumerable(sources));

    /// <summary>
    /// Zips two signals with a result selector.
    /// </summary>
    public static IObservable<TResult> Zip<TLeft, TRight, TResult>(IObservable<TLeft> left, IObservable<TRight> right, Func<TLeft, TRight, TResult> selector) =>
        Minimalist.Reactive.LinqMixins.Zip(left, right, selector);

    /// <summary>
    /// Combines the latest values from two signals.
    /// </summary>
    public static IObservable<TResult> CombineLatest<TLeft, TRight, TResult>(IObservable<TLeft> left, IObservable<TRight> right, Func<TLeft, TRight, TResult> selector) =>
        Minimalist.Reactive.LinqMixins.CombineLatest(left, right, selector);

    /// <summary>
    /// Combines latest values from two signals using latest-fusion semantics.
    /// </summary>
    public static IObservable<TResult> ZipLatest<TLeft, TRight, TResult>(IObservable<TLeft> left, IObservable<TRight> right, Func<TLeft, TRight, TResult> selector) =>
        Minimalist.Reactive.LinqMixins.ZipLatest(left, right, selector);

    /// <summary>
    /// Waits for both signals to complete and emits one result from their last values.
    /// </summary>
    public static IObservable<TResult> ForkJoin<TLeft, TRight, TResult>(IObservable<TLeft> left, IObservable<TRight> right, Func<TLeft, TRight, TResult> selector) =>
        Minimalist.Reactive.LinqMixins.ForkJoin(left, right, selector);
}
