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

using System.Collections.Generic;

namespace Aiel.Dependencies;

/// <summary>
/// Decorates an <see cref="ICollection{T}"/> and raises events before and after mutating operations.
/// </summary>
/// <typeparam name="T">The item type for the decorated collection.</typeparam>
public class CollectionDecorator<T> : ICollection<T>
{
    private readonly ICollection<T> _inner;

    /// <summary>
    /// Initializes a new instance of the <see cref="CollectionDecorator{T}"/> class.
    /// </summary>
    /// <param name="inner">The inner collection to decorate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="inner"/> is <see langword="null"/>.</exception>
    public CollectionDecorator(ICollection<T> inner)
    {
        ArgumentNullException.ThrowIfNull(inner);
        _inner = inner;
    }

    /// <summary>
    /// Occurs before a mutating operation is applied to the collection.
    /// </summary>
    public event EventHandler<CollectionChangingEventArgs<T>>? Changing;

    /// <summary>
    /// Occurs after a mutating operation has been applied to the collection.
    /// </summary>
    public event EventHandler<CollectionChangedEventArgs<T>>? Changed;

    /// <inheritdoc />
    public Int32 Count => _inner.Count;

    /// <inheritdoc />
    public Boolean IsReadOnly => _inner.IsReadOnly;

    /// <inheritdoc />
    public void Add(T item)
    {
        var changingEventArgs = new CollectionChangingEventArgs<T>(CollectionChangeAction.Add, item);
        OnChanging(changingEventArgs);

        if (changingEventArgs.Cancel)
        {
            return;
        }

        _inner.Add(changingEventArgs.Item);
        OnChanged(new CollectionChangedEventArgs<T>(CollectionChangeAction.Add, changingEventArgs.Item));
    }

    /// <inheritdoc />
    public void Clear()
    {
        if (_inner.Count == 0)
        {
            return;
        }

        var changingEventArgs = new CollectionChangingEventArgs<T>(CollectionChangeAction.Clear, default!);
        OnChanging(changingEventArgs);

        if (changingEventArgs.Cancel)
        {
            return;
        }

        _inner.Clear();
        OnChanged(new CollectionChangedEventArgs<T>(CollectionChangeAction.Clear, default!));
    }

    /// <inheritdoc />
    public Boolean Contains(T item)
    {
        return _inner.Contains(item);
    }

    /// <inheritdoc />
    public void CopyTo(T[] array, Int32 arrayIndex)
    {
        _inner.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc />
    public Boolean Remove(T item)
    {
        var changingEventArgs = new CollectionChangingEventArgs<T>(CollectionChangeAction.Remove, item);
        OnChanging(changingEventArgs);

        if (changingEventArgs.Cancel)
        {
            return false;
        }

        var wasRemoved = _inner.Remove(changingEventArgs.Item);
        if (!wasRemoved)
        {
            return false;
        }

        OnChanged(new CollectionChangedEventArgs<T>(CollectionChangeAction.Remove, changingEventArgs.Item));
        return true;
    }

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator()
    {
        return _inner.GetEnumerator();
    }

    /// <inheritdoc />
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Raises the <see cref="Changing"/> event.
    /// </summary>
    /// <param name="eventArgs">The event arguments.</param>
    protected virtual void OnChanging(CollectionChangingEventArgs<T> eventArgs)
    {
        ArgumentNullException.ThrowIfNull(eventArgs);
        Changing?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Raises the <see cref="Changed"/> event.
    /// </summary>
    /// <param name="eventArgs">The event arguments.</param>
    protected virtual void OnChanged(CollectionChangedEventArgs<T> eventArgs)
    {
        ArgumentNullException.ThrowIfNull(eventArgs);
        Changed?.Invoke(this, eventArgs);
    }
}

/// <summary>
/// Describes a mutating collection operation.
/// </summary>
public enum CollectionChangeAction
{
    Add,
    Remove,
    Clear,
}

/// <summary>
/// Represents event data for collection operations that are about to be applied.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public class CollectionChangingEventArgs<T>(CollectionChangeAction action, T item)
    : EventArgs
{
    /// <summary>
    /// Gets the operation being performed.
    /// </summary>
    public CollectionChangeAction Action { get; } = action;

    /// <summary>
    /// Gets or sets the item involved in the operation.
    /// </summary>
    public T Item { get; set; } = item;

    /// <summary>
    /// Gets or sets a value indicating whether the operation should be canceled.
    /// </summary>
    public Boolean Cancel { get; set; }
}

/// <summary>
/// Represents event data for collection operations that have been applied.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public class CollectionChangedEventArgs<T>(CollectionChangeAction action, T item)
    : EventArgs
{
    /// <summary>
    /// Gets the operation that was performed.
    /// </summary>
    public CollectionChangeAction Action { get; } = action;

    /// <summary>
    /// Gets the item involved in the operation.
    /// </summary>
    public T Item { get; } = item;
}
