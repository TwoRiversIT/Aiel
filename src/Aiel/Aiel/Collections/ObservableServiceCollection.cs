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

using Microsoft.Extensions.DependencyInjection;
using System.Collections;

namespace Aiel.Collections;

/// <summary>
/// An <see cref="IServiceCollection"/> wrapper that fires registered callbacks whenever a
/// <see cref="ServiceDescriptor"/> is added to the collection.
/// </summary>
/// <remarks>
/// Wrap your <see cref="IServiceCollection"/> with this type before calling
/// <see cref="AielServiceCollectionExtensions.OnAdding"/>.
/// All subsequent registrations made on this instance will trigger any subscribed callbacks.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="ObservableServiceCollection"/> class.
/// </remarks>
/// <param name="inner">The service collection to wrap.</param>
/// <exception cref="ArgumentNullException">Thrown when <paramref name="inner"/> is <see langword="null"/>.</exception>
public sealed class ObservableServiceCollection(IServiceCollection inner) : IServiceCollection
{
    private readonly IServiceCollection _inner = inner ?? throw new ArgumentNullException(nameof(inner));
    private readonly List<Action<ServiceDescriptor>> _callbacks = [];

    /// <inheritdoc />
    public ServiceDescriptor this[Int32 index]
    {
        get => _inner[index];
        set => _inner[index] = value;
    }

    /// <inheritdoc />
    public Int32 Count => _inner.Count;

    /// <inheritdoc />
    public Boolean IsReadOnly => _inner.IsReadOnly;

    /// <inheritdoc />
    public void Add(ServiceDescriptor item)
    {
        _inner.Add(item);
        InvokeCallbacks(item);
    }

    /// <inheritdoc />
    public void Clear() => _inner.Clear();

    /// <inheritdoc />
    public Boolean Contains(ServiceDescriptor item) => _inner.Contains(item);

    /// <inheritdoc />
    public void CopyTo(ServiceDescriptor[] array, Int32 arrayIndex) => _inner.CopyTo(array, arrayIndex);

    /// <inheritdoc />
    public IEnumerator<ServiceDescriptor> GetEnumerator() => _inner.GetEnumerator();

    /// <inheritdoc />
    public Int32 IndexOf(ServiceDescriptor item) => _inner.IndexOf(item);

    /// <inheritdoc />
    public void Insert(Int32 index, ServiceDescriptor item)
    {
        _inner.Insert(index, item);
        InvokeCallbacks(item);
    }

    /// <inheritdoc />
    public Boolean Remove(ServiceDescriptor item) => _inner.Remove(item);

    /// <inheritdoc />
    public void RemoveAt(Int32 index) => _inner.RemoveAt(index);

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Subscribe(Action<ServiceDescriptor> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);

        _callbacks.Add(callback);
    }

    private void InvokeCallbacks(ServiceDescriptor item)
    {
        foreach (var callback in _callbacks)
        {
            callback(item);
        }
    }
}
