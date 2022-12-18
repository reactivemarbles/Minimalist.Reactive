﻿// Copyright (c) 2019-2022 ReactiveUI Association Incorporated. All rights reserved.
// ReactiveUI Association Incorporated licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System.Globalization;

namespace Minimalist.Reactive.Core;
/// <summary>
/// Represents a value associated with time interval information.
/// The time interval can represent the time it took to produce the value, the interval relative to a previous value, the value's delivery time relative to a base, etc.
/// </summary>
/// <typeparam name="T">The type of the value being annotated with time interval information.</typeparam>
[Serializable]
public readonly struct TimeInterval<T> : IEquatable<TimeInterval<T>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeInterval{T}"/> struct.
    /// </summary>
    /// <param name="value">The value to be annotated with a time interval.</param>
    /// <param name="interval">Time interval associated with the value.</param>
    public TimeInterval(T value, TimeSpan interval)
    {
        Interval = interval;
        Value = value;
    }

    /// <summary>
    /// Gets the value.
    /// </summary>
    public T Value { get; }

    /// <summary>
    /// Gets the interval.
    /// </summary>
    public TimeSpan Interval { get; }

    /// <summary>
    /// Determines whether the two specified TimeInterval values have the same Value and Interval.
    /// </summary>
    /// <param name="first">The first TimeInterval value to compare.</param>
    /// <param name="second">The second TimeInterval value to compare.</param>
    /// <returns>true if the first TimeInterval value has the same Value and Interval as the second TimeInterval value; otherwise, false.</returns>
    public static bool operator ==(TimeInterval<T> first, TimeInterval<T> second) =>
        first.Equals(second);

    /// <summary>
    /// Determines whether the two specified TimeInterval values don't have the same Value and Interval.
    /// </summary>
    /// <param name="first">The first TimeInterval value to compare.</param>
    /// <param name="second">The second TimeInterval value to compare.</param>
    /// <returns>true if the first TimeInterval value has a different Value or Interval as the second TimeInterval value; otherwise, false.</returns>
    public static bool operator !=(TimeInterval<T> first, TimeInterval<T> second) =>
        !first.Equals(second);

    /// <summary>
    /// Determines whether the current TimeInterval value has the same Value and Interval as a specified TimeInterval value.
    /// </summary>
    /// <param name="other">An object to compare to the current TimeInterval value.</param>
    /// <returns>true if both TimeInterval values have the same Value and Interval; otherwise, false.</returns>
    public bool Equals(TimeInterval<T> other) =>
        other.Interval.Equals(Interval) && EqualityComparer<T>.Default.Equals(Value, other.Value);

    /// <summary>
    /// Determines whether the specified System.Object is equal to the current TimeInterval.
    /// </summary>
    /// <param name="obj">The System.Object to compare with the current TimeInterval.</param>
    /// <returns>true if the specified System.Object is equal to the current TimeInterval; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        if (obj is not TimeInterval<T>)
        {
            return false;
        }

        var other = (TimeInterval<T>)obj;
        return Equals(other);
    }

    /// <summary>
    /// Returns the hash code for the current TimeInterval value.
    /// </summary>
    /// <returns>A hash code for the current TimeInterval value.</returns>
    public override int GetHashCode()
    {
        var valueHashCode = Value == null ? 1963 : Value.GetHashCode();

        return Interval.GetHashCode() ^ valueHashCode;
    }

    /// <summary>
    /// Returns a string representation of the current TimeInterval value.
    /// </summary>
    /// <returns>String representation of the current TimeInterval value.</returns>
    public override string ToString() =>
        string.Format(CultureInfo.CurrentCulture, "{0}@{1}", Value, Interval);
}
