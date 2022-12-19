// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using Minimalist.Reactive.Observables.Core;

namespace Minimalist.Reactive.Observables;

/// <summary>
/// Create Observable functionality.
/// </summary>
public static partial class Observable
{
    /// <summary>
    /// Create anonymous observable. Observer has exception durability.
    /// This is recommended for make operator and event, generating a HotObservable.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="subscribe">The subscribe.</param>
    /// <returns>An Observable.</returns>
    /// <exception cref="System.ArgumentNullException">subscribe.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="subscribe" /> is <c>null</c>.</exception>
    public static IObservable<T> Create<T>(Func<IObserver<T>, IDisposable> subscribe)
    {
        if (subscribe == null)
        {
            throw new ArgumentNullException(nameof(subscribe));
        }

        return new CreateObservable<T>(subscribe);
    }

    /// <summary>
    /// Create anonymous observable. Observer has exception durability.
    /// This is recommended for make operator and event, generating a HotObservable.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="subscribe">The subscribe.</param>
    /// <param name="isRequiredSubscribeOnCurrentThread">if set to <c>true</c> [is required subscribe on current thread].</param>
    /// <returns>An Observable.</returns>
    /// <exception cref="System.ArgumentNullException">subscribe.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="subscribe" /> is <c>null</c>.</exception>
    public static IObservable<T> Create<T>(Func<IObserver<T>, IDisposable> subscribe, bool isRequiredSubscribeOnCurrentThread)
    {
        if (subscribe == null)
        {
            throw new ArgumentNullException(nameof(subscribe));
        }

        return new CreateObservable<T>(subscribe, isRequiredSubscribeOnCurrentThread);
    }

    /// <summary>
    /// Create anonymous observable. Observer has exception durability.
    /// This is recommended for make operator and event, generating a HotObservable.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="state">The state.</param>
    /// <param name="subscribe">The subscribe.</param>
    /// <returns>An Observable.</returns>
    /// <exception cref="System.ArgumentNullException">subscribe.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="subscribe" /> is <c>null</c>.</exception>
    public static IObservable<T> CreateWithState<T, TState>(TState state, Func<TState, IObserver<T>, IDisposable> subscribe)
    {
        if (subscribe == null)
        {
            throw new ArgumentNullException(nameof(subscribe));
        }

        return new CreateObservable<T, TState>(state, subscribe);
    }

    /// <summary>
    /// Create anonymous observable. Observer has exception durability.
    /// This is recommended for make operator and event, generating a HotObservable.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <typeparam name="TState">The type of the state.</typeparam>
    /// <param name="state">The state.</param>
    /// <param name="subscribe">The subscribe.</param>
    /// <param name="isRequiredSubscribeOnCurrentThread">if set to <c>true</c> [is required subscribe on current thread].</param>
    /// <returns>An Observable.</returns>
    /// <exception cref="System.ArgumentNullException">subscribe.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="subscribe" /> is <c>null</c>.</exception>
    public static IObservable<T> CreateWithState<T, TState>(TState state, Func<TState, IObserver<T>, IDisposable> subscribe, bool isRequiredSubscribeOnCurrentThread)
    {
        if (subscribe == null)
        {
            throw new ArgumentNullException(nameof(subscribe));
        }

        return new CreateObservable<T, TState>(state, subscribe, isRequiredSubscribeOnCurrentThread);
    }

    /// <summary>
    /// Create anonymous observable. Safe means auto detach when error raised in onNext pipeline.
    /// This is recommended for making a ColdObservable.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="subscribe">The subscribe.</param>
    /// <returns>An Observable.</returns>
    /// <exception cref="System.ArgumentNullException">subscribe.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="subscribe" /> is <c>null</c>.</exception>
    public static IObservable<T> CreateSafe<T>(Func<IObserver<T>, IDisposable> subscribe)
    {
        if (subscribe == null)
        {
            throw new ArgumentNullException(nameof(subscribe));
        }

        return new CreateSafeObservable<T>(subscribe);
    }

    /// <summary>
    /// Create anonymous observable. Safe means auto detach when error raised in onNext pipeline.
    /// This is recommended for making a ColdObservable.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    /// <param name="subscribe">The subscribe.</param>
    /// <param name="isRequiredSubscribeOnCurrentThread">if set to <c>true</c> [is required subscribe on current thread].</param>
    /// <returns>An Observable.</returns>
    /// <exception cref="System.ArgumentNullException">subscribe.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="subscribe" /> is <c>null</c>.</exception>
    public static IObservable<T> CreateSafe<T>(Func<IObserver<T>, IDisposable> subscribe, bool isRequiredSubscribeOnCurrentThread)
    {
        if (subscribe == null)
        {
            throw new ArgumentNullException(nameof(subscribe));
        }

        return new CreateSafeObservable<T>(subscribe, isRequiredSubscribeOnCurrentThread);
    }
}
