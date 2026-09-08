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

using Aiel.Domain.Aggregates;
using Aiel.Domain.Events;
using Aiel.StrongIds;

namespace Aiel.Domain;

/// <summary>
/// Represents the base class for aggregate roots in the domain-driven design
/// context. An aggregate root is an entity that serves as the entry point
/// for a cluster of related entities and ensures the consistency of changes
/// within that cluster. This class provides functionality for managing
/// domain events associated with the aggregate root.
/// </summary>
/// <typeparam name="TKey">a strongly typed identifier for the aggregate root.</typeparam>
public abstract class AggregateRoot<TKey> : Entity<TKey>, IAggregateRoot
    where TKey : notnull, IStrongId
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Occurs when a domain event is added to the aggregate root.
    /// </summary>
    public event EventHandler<DomainEventArgs>? DomainEventAdded;

    /// <inheritdoc/>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TKey}"/> class with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the aggregate root.</param>
    protected AggregateRoot(TKey id)
        : base(id)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateRoot{TKey}"/> class.
    /// </summary>
    protected AggregateRoot()
    {
    }

    /// <summary>
    /// Adds a domain event to the list of domain events associated with the aggregate root.
    /// </summary>
    /// <param name="domainEvent">The domain event to add to the list of domain events.</param>
    protected virtual void AddEvent(IDomainEvent domainEvent)
    {
        ArgumentNullException.ThrowIfNull(domainEvent);

        _domainEvents.Add(domainEvent);
        OnDomainEventAdded(new DomainEventArgs(domainEvent));
    }

    /// <summary>
    /// Called when a domain event is raised. This method can be overridden in derived classes to perform additional actions when a domain event is raised.
    /// </summary>
    /// <param name="e">The argument containing the domain event that was added.</param>
    protected virtual void OnDomainEventAdded(DomainEventArgs e) => DomainEventAdded?.Invoke(this, e);

    /// <summary>
    /// Clears all domain events from the aggregate root.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}

/// <summary>
/// Represents the event arguments for a domain event. This class is used to encapsulate the domain event that is raised by an aggregate root.
/// </summary>
public class DomainEventArgs : EventArgs
{
    /// <summary>
    /// Gets the domain event associated with the event arguments.
    /// </summary>
    public IDomainEvent DomainEvent { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainEventArgs"/> class with the specified domain event.
    /// </summary>
    /// <param name="domainEvent">The domain event to associate with the event arguments.</param>
    internal DomainEventArgs(IDomainEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }
}