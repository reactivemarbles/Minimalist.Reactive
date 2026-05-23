// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Threading;
using Minimalist.Reactive.Concurrency;
using Minimalist.Reactive.Disposables;
using Xunit;

namespace Minimalist.Reactive.Tests;

/// <summary>
/// ConcurencyTests.
/// </summary>
public class ConcurencyTests
{
    /// <summary>
    /// Tests this instance.
    /// </summary>
    [Fact]
    public void TestCreate()
    {
        var scheduler = TaskPoolScheduler.Instance;
        var disposable = scheduler.Schedule(0, (__, _) => Disposable.Empty);
        Assert.NotNull(disposable);
        disposable.Dispose();
    }

    /// <summary>
    /// Tasks the pool now.
    /// </summary>
    [Fact]
    public void TaskPoolNow()
    {
        var res = TaskPoolScheduler.Instance.Now - DateTime.Now;
        Assert.True(res.Seconds < 1);
    }

    /// <summary>
    /// Tasks the pool schedule action.
    /// </summary>
    [Fact]
    public void TaskPoolScheduleAction()
    {
        var id = Environment.CurrentManagedThreadId;
        var nt = TaskPoolScheduler.Instance;
        var evt = new ManualResetEvent(false);
        nt.Schedule(() =>
        {
            Assert.NotEqual(id, Environment.CurrentManagedThreadId);
            evt.Set();
        });
        evt.WaitOne();
    }

    /// <summary>
    /// Tasks the pool schedule action due now.
    /// </summary>
    [Fact]
    public void TaskPoolScheduleActionDueNow()
    {
        var id = Environment.CurrentManagedThreadId;
        var nt = TaskPoolScheduler.Instance;
        var evt = new ManualResetEvent(false);
        nt.Schedule(TimeSpan.Zero, () =>
        {
            Assert.NotEqual(id, Environment.CurrentManagedThreadId);
            evt.Set();
        });
        evt.WaitOne();
    }

    /// <summary>
    /// Tasks the pool schedule action due.
    /// </summary>
    [Fact]
    public void TaskPoolScheduleActionDue()
    {
        var id = Environment.CurrentManagedThreadId;
        var nt = TaskPoolScheduler.Instance;
        var evt = new ManualResetEvent(false);
        nt.Schedule(TimeSpan.FromMilliseconds(1), () =>
        {
            Assert.NotEqual(id, Environment.CurrentManagedThreadId);
            evt.Set();
        });
        evt.WaitOne();
    }

    /// <summary>
    /// Tasks the pool schedule action cancel.
    /// </summary>
    [Fact]
    public void TaskPoolScheduleActionCancel()
    {
        var id = Environment.CurrentManagedThreadId;
        var nt = TaskPoolScheduler.Instance;
        var set = false;
        var d = nt.Schedule(TimeSpan.FromSeconds(0.2), () => set = true);
        d.Dispose();
        Thread.Sleep(400);
        Assert.False(set);
    }

    /// <summary>
    /// Tasks the pool delay larger than int maximum value.
    /// </summary>
    [Fact]
    public void TaskPoolDelayLargerThanIntMaxValue()
    {
        var dueTime = TimeSpan.FromMilliseconds((double)int.MaxValue + 1);

        // Just ensuring the call to Schedule does not throw.
        var d = TaskPoolScheduler.Instance.Schedule(dueTime, () => { });

        d.Dispose();
    }
}
