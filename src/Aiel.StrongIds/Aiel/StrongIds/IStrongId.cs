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

namespace Aiel.StrongIds;

/// <summary>
/// Represents a strongly-typed identifier.
/// </summary>
public interface IStrongId
{
    /// <summary>
    /// Gets a value indicating whether the identifier is the default value.
    /// </summary>
    Boolean IsDefault { get; }
}

/// <summary>
/// Represents a strongly-typed identifier with a specific value type as the backing store.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public interface IStrongId<TValue> : IStrongId
{
    /// <summary>
    /// Gets the value of the strongly-typed identifier.
    /// </summary>
    TValue Value { get; }
}

/// <summary>
/// Provides extension methods for working with strongly-typed identifiers.
/// </summary>
public static class StrongIdExtensions
{
    /// <summary>
    /// Throws an ArgumentException if the specified strongly-typed identifier is the default value.
    /// </summary>
    /// <typeparam name="T">The type of the strongly-typed identifier.</typeparam>
    /// <param name="value">The strongly-typed identifier to check.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <returns>The strongly-typed identifier if it is not the default value.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static T ThrowIfDefault<T>(this T value, String parameterName)
        where T : IStrongId
    {
        if (value.IsDefault)
        {
            throw new ArgumentException("The StrongId is empty or default.", parameterName);
        }

        return value;
    }

    /// <summary>
    /// Throws an ArgumentException if the specified nullable strongly-typed identifier is the default value. Does not throw an exception for null values.
    /// </summary>
    /// <typeparam name="T">The type of the strongly-typed identifier.</typeparam>
    /// <param name="value">The nullable strongly-typed identifier to check.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <returns>The nullable strongly-typed identifier if it is not the default value.</returns>
    /// <exception cref="ArgumentException"></exception>
    public static T? ThrowIfDefault<T>(this T? value, String parameterName)
        where T : struct, IStrongId
    {
        // Null is not default value so we don't throw an exception for null values.
        if (value?.IsDefault == true)
        {
            throw new ArgumentException("The StrongId is empty or default.", parameterName);
        }

        return value;
    }
}
