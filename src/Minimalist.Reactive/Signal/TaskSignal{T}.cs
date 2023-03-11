﻿// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Minimalist.Reactive.Concurrency;
using Minimalist.Reactive.Disposables;

namespace Minimalist.Reactive.Signals;

/// <summary>
/// TaskSignal.
/// </summary>
/// <typeparam name="T">The object that provides notification information.</typeparam>
internal class TaskSignal<T> : ITaskSignal<T>
{
    private readonly IScheduler _scheduler;
    private readonly MultipleDisposable? _cleanUp = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="TaskSignal{T}" /> class.
    /// </summary>
    /// <param name="observableFactory">The observable factory.</param>
    /// <param name="scheduler">The scheduler.</param>
    /// <param name="cancellationTokenSource">The cancellation token source.</param>
    public TaskSignal(Func<ITaskSignal<T>, IObservable<T>> observableFactory, IScheduler? scheduler = null, CancellationTokenSource? cancellationTokenSource = null)
    {
        if (observableFactory is null)
        {
            throw new ArgumentNullException(nameof(observableFactory));
        }

        CancellationTokenSource = cancellationTokenSource ?? new();
        _scheduler = scheduler ?? CurrentThreadScheduler.Instance;
        Source = observableFactory(this);
    }

    /// <summary>
    /// Gets or sets the source.
    /// </summary>
    /// <value>
    /// The source.
    /// </value>
    public IObservable<T>? Source { get; set; }

    /// <summary>
    /// Gets the cancellation token source.
    /// </summary>
    /// <value>
    /// The cancellation token source.
    /// </value>
    public CancellationTokenSource? CancellationTokenSource { get; }

    /// <summary>
    /// Gets a value indicating whether this instance is cancellation requested.
    /// </summary>
    /// <value>
    ///   <c>true</c> if this instance is cancellation requested; otherwise, <c>false</c>.
    /// </value>
    public bool IsCancellationRequested => CancellationTokenSource?.IsCancellationRequested == true;

    /// <summary>
    /// Gets a value indicating whether gets a value that indicates whether the object is disposed.
    /// </summary>
    public bool IsDisposed => _cleanUp?.IsDisposed ?? true;

    /// <summary>
    /// Gets the operation canceled.
    /// </summary>
    /// <param name="observer">The observer.</param>
    public void GetOperationCanceled(IObserver<Exception> observer) =>
        CancellationTokenSource?.Token.Register(() => observer.OnNext(new OperationCanceledException())).DisposeWith(_cleanUp!);

    /// <summary>
    /// Subscribes the specified observer.
    /// </summary>
    /// <param name="observer">The observer.</param>
    /// <returns>A Disposable.</returns>
    public IDisposable Subscribe(IObserver<T> observer) =>
        Source!.WitnessOn(_scheduler).Subscribe(observer).DisposeWith(_cleanUp!);

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_cleanUp?.IsDisposed == false && disposing)
        {
            try
            {
                CancellationTokenSource?.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

            _cleanUp?.Dispose();
            CancellationTokenSource?.Dispose();
        }
    }
}
