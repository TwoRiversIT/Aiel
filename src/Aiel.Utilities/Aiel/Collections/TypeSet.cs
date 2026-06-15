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

using System.Collections;

namespace Aiel.Collections;

/// <summary>
/// Stores the types of items in a collection rather than the instances themselves,
/// allowing for type-based operations on the collection without needing to know
/// the specific item types at compile time.
/// </summary>
/// <typeparam name="TBase"></typeparam>
public interface ITypeSet<in TBase> : ISet<Type>, IReadOnlyCollection<Type>, IReadOnlySet<Type>
    where TBase : class
{
    void Add<T>(T item)
        where T : TBase;
    Boolean Contains<T>(T item)
        where T : TBase;
    Boolean Remove<T>(T item)
        where T : TBase;
}

public class TypeSet<TBase> : ITypeSet<TBase>
    where TBase : class
{
    private readonly HashSet<Type> _inner = [];

    public Int32 Count => _inner.Count;
    public Boolean IsReadOnly => false;

    public void Add<T>(T item) where T : TBase
    {
        ArgumentNullException.ThrowIfNull(item);
        _inner.Add(item.GetType());
    }

    public Boolean Add(Type item)
    {
        EnsureCompatibleType(item);
        return _inner.Add(item);
    }

    public void AddRange(IEnumerable<Type> types)
    {
        _inner.UnionWith(EnsureCompatibleTypes(types));
    }

    public void Clear() => _inner.Clear();

    public Boolean Contains<T>(T item) where T : TBase
    {
        ArgumentNullException.ThrowIfNull(item);
        return _inner.Contains(item.GetType());
    }

    public Boolean Contains(Type item)
    {
        EnsureCompatibleType(item);
        return _inner.Contains(item);
    }

    public void CopyTo(Type[] array, Int32 arrayIndex)
    {
        _inner.CopyTo(array, arrayIndex);
    }

    public void ExceptWith(IEnumerable<Type> other)
    {
        _inner.ExceptWith(EnsureCompatibleTypes(other));
    }

    public IEnumerator<Type> GetEnumerator() => _inner.GetEnumerator();

    public void IntersectWith(IEnumerable<Type> other)
    {
        _inner.IntersectWith(EnsureCompatibleTypes(other));
    }

    public Boolean IsProperSubsetOf(IEnumerable<Type> other)
    {
        return _inner.IsProperSubsetOf(EnsureCompatibleTypes(other));
    }

    public Boolean IsProperSupersetOf(IEnumerable<Type> other)
    {
        return _inner.IsProperSupersetOf(EnsureCompatibleTypes(other));
    }

    public Boolean IsSubsetOf(IEnumerable<Type> other)
    {
        return _inner.IsSubsetOf(EnsureCompatibleTypes(other));
    }

    public Boolean IsSupersetOf(IEnumerable<Type> other)
    {
        return _inner.IsSupersetOf(EnsureCompatibleTypes(other));
    }

    public Boolean Overlaps(IEnumerable<Type> other)
    {
        return _inner.Overlaps(EnsureCompatibleTypes(other));
    }

    public Boolean Remove<T>(T item) where T : TBase
    {
        ArgumentNullException.ThrowIfNull(item);
        return _inner.Remove(item.GetType());
    }

    public Boolean Remove(Type item)
    {
        EnsureCompatibleType(item);
        return _inner.Remove(item);
    }

    public Boolean SetEquals(IEnumerable<Type> other)
    {
        return _inner.SetEquals(EnsureCompatibleTypes(other));
    }

    public void SymmetricExceptWith(IEnumerable<Type> other)
    {
        _inner.SymmetricExceptWith(EnsureCompatibleTypes(other));
    }

    public void UnionWith(IEnumerable<Type> other)
    {
        _inner.UnionWith(EnsureCompatibleTypes(other));
    }

    void ICollection<Type>.Add(Type item)
    {
        _ = Add(item);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static List<Type> EnsureCompatibleTypes(IEnumerable<Type> types)
    {
        ArgumentNullException.ThrowIfNull(types);
        var result = new List<Type>();
        foreach (var type in types)
        {
            EnsureCompatibleType(type);
            result.Add(type);
        }

        return result;
    }

    private static void EnsureCompatibleType(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (!typeof(TBase).IsAssignableFrom(type))
        {
            throw new ArgumentException($"Type '{type.FullName}' must be assignable to '{typeof(TBase).FullName}'.", nameof(type));
        }
    }
}
