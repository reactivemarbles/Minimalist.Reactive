﻿// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Minimalist.Reactive.Concurrency;
using Minimalist.Reactive.Disposables;

namespace Minimalist.Reactive.Signals;

/// <summary>
/// Signals.
/// </summary>
public static partial class Signal
{
    /// <summary>
    /// Handles Asnyc Tasks with cancellation.
    /// </summary>
    /// <param name="execution">The function to execute.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="cancellationTokenSource">The cancellation token source.</param>
    /// <returns>
    /// An ITaskSignal of T.
    /// </returns>
    public static ITaskSignal<RxVoid> FromTask(Func<CancellationTokenSource, Task<RxVoid>> execution, IScheduler? scheduler = null, CancellationTokenSource? cancellationTokenSource = null) =>
        TaskSignal.Create<RxVoid>(
            ao => Defer(() => Create<RxVoid>(
            obs =>
            {
                // CancelationToken
                var src = ao.CancellationTokenSource!;
                var ct = src.Token;
                ct.ThrowIfCancellationRequested();
                var hasError = false;
                var hasCompleted = false;
                var cancellableTask = Task.Factory.StartNew(() => execution(src), ct, TaskCreationOptions.None, TaskScheduler.Current).WhenCancelled(ct);

                Task.Run(async () =>
                {
                    try
                    {
#pragma warning disable IDE0042 // Deconstruct variable declaration
                        var cancellableTaskHandler = await cancellableTask;
#pragma warning restore IDE0042 // Deconstruct variable declaration
                        var result = await cancellableTaskHandler.Result;
                        if (!cancellableTaskHandler.IsCanceled && !src.IsCancellationRequested)
                        {
                            obs.OnNext(result);
                            hasCompleted = !src.IsCancellationRequested;
                            obs.OnCompleted();
                        }
                        else
                        {
                            obs.OnError(new OperationCanceledException());
                        }
                    }
                    catch (Exception ex)
                    {
                        hasError = true;

                        // Catch the exception and pass it to the observer if not user handled.
                        obs.OnError(ex);
                        await Task.Delay(1);
                    }
                });
                return Disposable.Create(() =>
                {
                    if (hasError)
                    {
                        Task.Delay(2).Wait();
                    }

                    if (hasError || !hasCompleted)
                    {
                        try
                        {
                            src.Cancel();
                        }
                        catch (ObjectDisposedException)
                        {
                            throw new OperationCanceledException();
                        }
                    }

                    src.Dispose();
                });
            })),
            scheduler,
            cancellationTokenSource);

    /// <summary>
    /// Froms the asynchronous.
    /// </summary>
    /// <typeparam name="TResult">The type of the return value.</typeparam>
    /// <param name="actionAsync">The action asynchronous.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="cancellationTokenSource">The cancellation token source.</param>
    /// <returns>
    /// An TaskSignal of T.
    /// </returns>
    public static ITaskSignal<TResult> FromTask<TResult>(Func<CancellationTokenSource, Task<TResult>> actionAsync, IScheduler? scheduler = null, CancellationTokenSource? cancellationTokenSource = null) =>
        TaskSignal.Create<TResult>(
            ao => Defer(() => Create<TResult>(
            obs =>
            {
                // CancelationToken
                var src = ao.CancellationTokenSource!;
                var ct = src.Token;
                ct.ThrowIfCancellationRequested();
                var hasError = false;
                var hasCompleted = false;
                var cancellableTask = Task.Factory.StartNew(() => actionAsync(src), ct, TaskCreationOptions.None, TaskScheduler.Current).WhenCancelled(ct);

                Task.Run(async () =>
                {
                    try
                    {
#pragma warning disable IDE0042 // Deconstruct variable declaration
                        var cancellableTaskHandler = await cancellableTask;
#pragma warning restore IDE0042 // Deconstruct variable declaration
                        var result = await cancellableTaskHandler.Result;
                        if (result != null && !src.IsCancellationRequested)
                        {
                            obs.OnNext(result);
                            hasCompleted = !src.IsCancellationRequested;
                            obs.OnCompleted();
                        }
                        else
                        {
                            obs.OnError(new OperationCanceledException());
                        }
                    }
                    catch (Exception ex)
                    {
                        hasError = true;

                        // Catch the exception and pass it to the observer if not user handled.
                        obs.OnError(ex);
                        await Task.Delay(1);
                    }
                });
                return Disposable.Create(() =>
                {
                    if (hasError)
                    {
                        Task.Delay(2).Wait();
                    }

                    if (hasError || !hasCompleted)
                    {
                        try
                        {
                            src.Cancel();
                        }
                        catch (ObjectDisposedException)
                        {
                            throw new OperationCanceledException();
                        }
                    }

                    src.Dispose();
                });
            })),
            scheduler,
            cancellationTokenSource);

    /// <summary>
    /// Handles the cancellation.
    /// </summary>
    /// <param name="asyncTask">The asynchronous task.</param>
    /// <param name="action">The action.</param>
    /// <returns>A Task.</returns>
    public static async Task HandleCancellation(this Task asyncTask, Action? action = null)
    {
        try
        {
            await asyncTask;
        }
        catch (OperationCanceledException)
        {
            action?.Invoke();
        }
    }

    /// <summary>
    /// Handles the cancellation.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="asyncTask">The asynchronous task.</param>
    /// <param name="action">The action.</param>
    /// <returns>A Task of TResult.</returns>
    public static async Task<TResult?> HandleCancellation<TResult>(this Task<TResult> asyncTask, Action? action = null)
    {
        try
        {
            return await asyncTask;
        }
        catch (OperationCanceledException)
        {
            action?.Invoke();
        }

        return default;
    }

    /// <summary>
    /// Handles the cancellation.
    /// </summary>
    /// <typeparam name="TResult">The type.</typeparam>
    /// <param name="asyncTask">The asynchronous task.</param>
    /// <param name="token">The token.</param>
    /// <param name="action">The action.</param>
    /// <returns>
    /// A Task.
    /// </returns>
    public static async Task<TResult?> HandleCancellation<TResult>(this IObservable<TResult> asyncTask, CancellationToken token, Action? action = null)
    {
        try
        {
            token.ThrowIfCancellationRequested();
            return await Task.Run(async () => await asyncTask, token);
        }
        catch (OperationCanceledException)
        {
            action?.Invoke();
        }

        return default;
    }

    private static async Task<(TResult Result, bool IsCanceled)> WhenCancelled<TResult>(this Task<TResult> asyncTask, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<TResult>();
        cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
        var cancellationTask = tcs.Task;

        // Create a task that completes when either the async operation completes,
        // or cancellation is requested.
        var readyTask = await Task.WhenAny(asyncTask, cancellationTask);

        // In case of cancellation, register a continuation to observe any unhandled.
        // exceptions from the asynchronous operation (once it completes).
        if (readyTask == cancellationTask)
        {
            await asyncTask.ContinueWith(_ => asyncTask.Exception, cancellationToken, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Current);
        }

        return (await readyTask, tcs.Task.IsCanceled || readyTask.IsCanceled);
    }
}
