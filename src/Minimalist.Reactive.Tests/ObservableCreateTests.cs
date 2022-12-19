// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Minimalist.Reactive.Disposables;
using Minimalist.Reactive.Observables;
using Xunit;

namespace Minimalist.Reactive.Tests;

/// <summary>
/// ObservableCreateTests.
/// </summary>
public class ObservableCreateTests
{
    /// <summary>
    /// Creates the argument checking.
    /// </summary>
    [Fact]
    public void Create_ArgumentChecking()
    {
        Assert.Throws<ArgumentNullException>(() => Observable.Create(default(Func<IObserver<int>, IDisposable>)));

        Assert.Throws<ArgumentNullException>(() => Observable.Create<int>(default).Subscribe(null));
    }

    /// <summary>
    /// Creates the null coalescing action.
    /// </summary>
    [Fact]
    public void Create_NullCoalescingAction()
    {
        var xs = Observable.Create<int>(o =>
        {
            o.OnNext(42);
            return Disposable.Create(default!);
        });

        var lst = new List<int>();
        var d = xs.Subscribe(lst.Add);
        d.Dispose();

        Assert.True(lst.SequenceEqual(new[] { 42 }));
    }

    /// <summary>
    /// Creates the exception.
    /// </summary>
    [Fact]
    public void Create_Exception() =>
        Assert.Throws<InvalidOperationException>(() =>
               Observable.Create(new Func<IObserver<int>, IDisposable>(_ => throw new InvalidOperationException())).Subscribe());

    /// <summary>
    /// Creates the observer throws.
    /// </summary>
    [Fact]
    public void Create_ObserverThrows()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Observable.Create<int>(o =>
            {
                o.OnNext(1);
                return Disposable.Empty;
            }).Subscribe(x => { throw new InvalidOperationException(); }));
        Assert.Throws<InvalidOperationException>(() =>
            Observable.Create<int>(o =>
            {
                o.OnError(new Exception());
                return Disposable.Empty;
            }).Subscribe(x => { }, ex => { throw new InvalidOperationException(); }));
        Assert.Throws<InvalidOperationException>(() =>
            Observable.Create<int>(o =>
            {
                o.OnCompleted();
                return Disposable.Empty;
            }).Subscribe(x => { }, ex => { }, () => { throw new InvalidOperationException(); }));
    }

    /// <summary>
    /// Creates the with disposable argument checking.
    /// </summary>
    [Fact]
    public void CreateWithDisposable_ArgumentChecking()
    {
        Assert.Throws<ArgumentNullException>(() => Observable.Create(default(Func<IObserver<int>, IDisposable>)));
        Assert.Throws<ArgumentNullException>(() => Observable.Create<int>(_ => DummyDisposable.Instance).Subscribe(null));
        Assert.Throws<ArgumentNullException>(() => Observable.Create<int>(o =>
        {
            o.OnError(null);
            return DummyDisposable.Instance;
        }).Subscribe(null));
    }

    /// <summary>
    /// Creates the with disposable null coalescing action.
    /// </summary>
    [Fact]
    public void CreateWithDisposable_NullCoalescingAction()
    {
        var xs = Observable.Create<int>(o =>
        {
            o.OnNext(42);
            return default!;
        });

        var lst = new List<int>();
        var d = xs.Subscribe(lst.Add);
        d.Dispose();

        Assert.True(lst.SequenceEqual(new[] { 42 }));
    }

    /// <summary>
    /// Creates the with disposable exception.
    /// </summary>
    [Fact]
    public void CreateWithDisposable_Exception() =>
        Assert.Throws<InvalidOperationException>(() =>
               Observable.Create(new Func<IObserver<int>, IDisposable>(_ => throw new InvalidOperationException())).Subscribe());
}
