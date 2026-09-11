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

using Aiel.StrongIds;

namespace Aiel.Application;

/// <summary>
/// Base class for data transfer objects that carry a strongly-typed identifier.
/// </summary>
/// <remarks>
/// <para>
/// This type is a plain reference type: equality and hashing use reference semantics,
/// and it does not implement <see cref="IComparable"/>, <see cref="IEquatable{T}"/>,
/// or any domain contract. It exists solely to transfer data.
/// </para>
/// <para>
/// Choose <see cref="ClassDto{TKey}"/> when reference identity is what you want (for example,
/// mutable view state in a UI, or when instances must be distinguished even if their data
/// matches). Choose <see cref="RecordDto{TKey}"/> when structural (value) equality,
/// <c>with</c> expressions, and a generated <c>ToString()</c> are what you want.
/// </para>
/// </remarks>
/// <typeparam name="TKey">The type of the strongly-typed identifier.</typeparam>
public abstract class ClassDto<TKey>
    where TKey : notnull, IStrongId
{
    /// <summary>
    /// Gets the identifier of the underlying data.
    /// </summary>
    public TKey Id { get; init; } = default!;
}
