﻿// Copyright (c) 2019-2023 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

namespace Minimalist.Reactive;

internal static class Handle<T1, T2>
{
    public static readonly Action<T1, T2> Ignore = (_, __) => { };
    public static readonly Action<Exception, T1, T2> Throw = (ex, _, __) => ex.Throw();
}
