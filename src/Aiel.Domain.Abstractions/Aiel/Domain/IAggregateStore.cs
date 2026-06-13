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

namespace Aiel.Domain;

/// <summary>
/// Defines the write-side persistence contract for an aggregate root.
/// </summary>
/// <remarks>
/// Implementations are responsible for loading and persisting aggregate state.
/// Saving changes (committing the unit of work) is a separate concern handled by
/// <see cref="Aiel.Commands.IUnitOfWork"/> and is intentionally not part of this interface.
/// </remarks>
/// <typeparam name="TAggregate">The aggregate root type.</typeparam>
/// <typeparam name="TId">The strong-ID type that identifies the aggregate.</typeparam>
public interface IAggregateStore<TAggregate, TId>
    where TAggregate : IAggregateRoot<TId>
    where TId : notnull, IStrongId
{
    /// <summary>
    /// Returns the aggregate with the specified <paramref name="id"/>, or <see langword="null"/>
    /// if no such aggregate exists.
    /// </summary>
    Task<TAggregate?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a new <paramref name="aggregate"/> for persistence.
    /// The aggregate is not written to the store until the current unit of work is saved.
    /// </summary>
    Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
}
