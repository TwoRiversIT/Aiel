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
public interface IStrongId : IComparable
{
    /// <summary>
    /// Gets a value indicating whether the identifier has a value.
    /// </summary>
    Boolean HasValue { get; }
}

/// <summary>
/// Represents a strongly-typed identifier with a specific value type as the backing store.
/// </summary>
/// <typeparam name="TValue">The type of the value. Supported types are Int16, Int32, Int64, UInt16, UInt32, UInt64, Guid, and String.</typeparam>
public interface IStrongId<TValue> : IStrongId, IComparable<IStrongId<TValue>>, IEquatable<IStrongId<TValue>>
    where TValue : notnull, IComparable<TValue>, IEquatable<TValue>
{
    /// <summary>
    /// Gets the value of the strongly-typed identifier.
    /// </summary>
    TValue Value { get; }
}

/// <summary>
/// Represents an entity with a strongly-typed identifier.
/// </summary>
public interface IHasStrongId
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    IStrongId Id { get; }
}

/// <summary>
/// Represents an entity with a strongly-typed identifier of a specific type.
/// </summary>
/// <typeparam name="TId">The type of the strongly-typed identifier.</typeparam>
public interface IHasStrongId<TId>
    where TId : IStrongId
{
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    TId Id { get; }
}

