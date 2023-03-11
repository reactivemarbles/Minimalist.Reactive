﻿// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
#if NET48
#endif

namespace Minimalist.Reactive.Tests;

internal class DummyDisposable : IDisposable
{
    public static readonly DummyDisposable Instance = new();

    public void Dispose() => throw new NotImplementedException();
}
