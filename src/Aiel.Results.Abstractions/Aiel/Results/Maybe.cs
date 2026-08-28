// MIT License
//
// Copyright 2026 Two Rivers Information Technology Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sub-license,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

namespace Aiel.Results;

/// <summary>
/// Provides convenience methods for creating instances of <see cref="Maybe{T}"/>.
/// </summary>
public static class Maybe
{
    /// <summary>
    /// Returns a <see cref="Maybe{T}"/> instance representing no value.
    /// </summary>
    /// <typeparam name="T">The type of the value that may be present.</typeparam>
    /// <returns>A <see cref="Maybe{T}"/> instance representing no value.</returns>
    public static Maybe<T> None<T>()
        where T : notnull
    => Maybe<T>.None;

    /// <summary>
    /// Returns a <see cref="Maybe{T}"/> instance representing a value.
    /// </summary>
    /// <typeparam name="T">The type of the value that may be present.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A <see cref="Maybe{T}"/> instance representing the specified value.</returns>
    public static Maybe<T> Some<T>(T value)
        where T : notnull
        => Maybe<T>.Some(value);

    /// <summary>
    /// Returns a <see cref="Maybe{T}"/> instance representing a value that may be <see langword="null"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value that may be present.</typeparam>
    /// <param name="value">The value to wrap.</param>
    /// <returns>A <see cref="Maybe{T}"/> instance representing the specified value, or <see cref="None"/> if the value is <see langword="null"/>.</returns>
    public static Maybe<T> FromNullable<T>(T? value)
        where T : notnull
        => value is null ? Maybe<T>.None : Maybe<T>.Some(value);
}

/// <summary>
/// Represents a value that may or may not be present, without using <see langword="null"/>.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Maybe{T}"/> exists to model the outcome "the operation succeeded and the answer is
/// legitimately nothing" — the not-found query. Pair it with <see cref="Result{T}"/> as
/// <c>Result&lt;Maybe&lt;T&gt;&gt;</c> so that absence stays in the value and failure stays in the error:
/// </para>
/// <list type="bullet">
/// <item><description>Success carrying <see cref="Some(T)"/> — the operation worked and found a value.</description></item>
/// <item><description>Success carrying <see cref="None"/> — the operation worked and there is legitimately no value.</description></item>
/// <item><description>Failure — the operation did not work.</description></item>
/// </list>
/// <para>
/// The default value of <see cref="Maybe{T}"/> is <see cref="None"/>, so an uninitialized or
/// default-constructed instance always fails closed rather than exposing <see langword="default"/>
/// as though it were a real answer.
/// </para>
/// </remarks>
/// <typeparam name="T">The type of the value that may be present.</typeparam>
public readonly record struct Maybe<T>
    where T : notnull
{
    private readonly T _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="Maybe{T}"/> struct holding the specified value.
    /// </summary>
    /// <param name="value">The value to hold. Must not be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    private Maybe(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        _value = value;
        HasValue = true;
    }

    /// <summary>
    /// Gets a value indicating whether a value is present.
    /// </summary>
    public Boolean HasValue { get; }

    /// <summary>
    /// Gets a value indicating whether no value is present.
    /// </summary>
    public Boolean IsNone => !HasValue;

    /// <summary>
    /// Gets the contained value.
    /// </summary>
    /// <remarks>
    /// Prefer <see cref="TryGetValue(out T)"/> when absence is an expected outcome. This property is
    /// intended for call sites that have already established <see cref="HasValue"/> is <see langword="true"/>.
    /// </remarks>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="HasValue"/> is <see langword="false"/>.</exception>
    public T Value => HasValue
        ? _value
        : throw new InvalidOperationException($"Maybe<{typeof(T).Name}> has no value. Check the HasValue property before reading Value or use GetValueOrDefault and TryGetValue methods instead.");

    /// <summary>
    /// Gets the contained value when one is present.
    /// </summary>
    /// <param name="value">
    /// When this method returns <see langword="true"/>, contains the value; otherwise <see langword="default"/>.
    /// </param>
    /// <returns><see langword="true"/> when a value is present; otherwise <see langword="false"/>.</returns>
    public Boolean TryGetValue([NotNullWhen(true)] out T? value)
    {
        value = HasValue ? _value : default;
        return HasValue;
    }

    /// <summary>
    /// Gets the contained value, or the specified fallback when no value is present.
    /// </summary>
    /// <param name="fallback">The value to return when no value is present. Must not be <see langword="null"/>.</param>
    /// <returns>The contained value when present; otherwise <paramref name="fallback"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fallback"/> is <see langword="null"/>.</exception>
    public T GetValueOrDefault(T fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);

        return HasValue ? _value : fallback;
    }

    /// <summary>
    /// Returns a string representation of this instance for diagnostic purposes.
    /// </summary>
    /// <returns><c>Some(value)</c> when a value is present; otherwise <c>None</c>.</returns>
    public override String ToString() => HasValue ? $"Some({_value})" : "None";

    /// <summary>
    /// Gets a <see cref="Maybe{T}"/> that holds no value.
    /// </summary>
    public static Maybe<T> None => default;

    /// <summary>
    /// Creates a <see cref="Maybe{T}"/> holding the specified value.
    /// </summary>
    /// <param name="value">The value to hold. Must not be <see langword="null"/>.</param>
    /// <returns>A <see cref="Maybe{T}"/> holding <paramref name="value"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    internal static Maybe<T> Some(T value) => new(value);

    /// <summary>
    /// Creates a <see cref="Maybe{T}"/> from a value that may be <see langword="null"/>.
    /// </summary>
    /// <remarks>
    /// This is the adapter for boundaries that still produce <see langword="null"/>, such as
    /// <c>FirstOrDefaultAsync</c> on a data-access query.
    /// </remarks>
    /// <param name="value">The value to convert. May be <see langword="null"/>.</param>
    /// <returns><see cref="None"/> when <paramref name="value"/> is <see langword="null"/>; otherwise <see cref="Some(T)"/>.</returns>
    internal static Maybe<T> FromNullable(T? value) => value is null ? None : Some(value);

    /// <summary>
    /// Implicit conversion from a value to a <see cref="Maybe{T}"/> holding that value.
    /// </summary>
    /// <param name="value">The value to convert. Must not be <see langword="null"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static implicit operator Maybe<T>(T value) => Some(value);
}
