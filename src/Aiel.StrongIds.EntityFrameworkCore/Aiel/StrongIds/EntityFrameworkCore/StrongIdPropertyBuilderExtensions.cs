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

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;
using System.Reflection;

namespace Aiel.StrongIds.EntityFrameworkCore;

public static class StrongIdPropertyBuilderExtensions
{
    public static PropertyBuilder<TStrongId> HasStrongIdConversion<TStrongId, TValue>(this PropertyBuilder<TStrongId> propertyBuilder)
        where TStrongId : notnull, IStrongId<TValue>
    {
        ArgumentNullException.ThrowIfNull(propertyBuilder);

        var converter = StrongIdConversionExpressions<TStrongId, TValue>.CreateConverter();
        var comparer = StrongIdConversionExpressions<TStrongId, TValue>.CreateComparer();

        propertyBuilder.HasConversion(converter);
        propertyBuilder.Metadata.SetValueComparer(comparer);

        return propertyBuilder;
    }

    public static PropertyBuilder<TStrongId?> HasStrongIdConversion<TStrongId, TValue>(this PropertyBuilder<TStrongId?> propertyBuilder)
        where TStrongId : struct, IStrongId<TValue>
        where TValue : struct
    {
        ArgumentNullException.ThrowIfNull(propertyBuilder);

        var converter = NullableStrongIdConversionExpressions<TStrongId, TValue>.CreateConverter();
        var comparer = NullableStrongIdConversionExpressions<TStrongId, TValue>.CreateComparer();

        propertyBuilder.HasConversion(converter);
        propertyBuilder.Metadata.SetValueComparer(comparer);

        return propertyBuilder;
    }
}

internal static class StrongIdConversionExpressions<TStrongId, TValue>
    where TStrongId : notnull, IStrongId<TValue>
{
    private static readonly MethodInfo FromMethod = ResolveFromMethod();
    private static readonly Func<TValue, TStrongId> From = BuildFromDelegate();

    public static ValueConverter<TStrongId, TValue> CreateConverter()
    {
        return new ValueConverter<TStrongId, TValue>(ToProviderExpression(), FromProviderExpression());
    }

    public static ValueComparer<TStrongId> CreateComparer()
    {
        return new ValueComparer<TStrongId>(
            (left, right) => EqualityComparer<TStrongId>.Default.Equals(left, right),
            strongId => EqualityComparer<TStrongId>.Default.GetHashCode(strongId),
            strongId => strongId);
    }

    private static Expression<Func<TStrongId, TValue>> ToProviderExpression()
    {
        var strongId = Expression.Parameter(typeof(TStrongId), "strongId");
        var value = Expression.Property(strongId, nameof(IStrongId<>.Value));

        return Expression.Lambda<Func<TStrongId, TValue>>(value, strongId);
    }

    private static Expression<Func<TValue, TStrongId>> FromProviderExpression()
    {
        var value = Expression.Parameter(typeof(TValue), "value");
        var call = Expression.Call(FromMethod, value);

        return Expression.Lambda<Func<TValue, TStrongId>>(call, value);
    }

    private static Func<TValue, TStrongId> BuildFromDelegate()
    {
        return FromProviderExpression().Compile();
    }

    private static MethodInfo ResolveFromMethod()
    {
        var method = typeof(TStrongId).GetMethod(
            "From",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            [typeof(TValue)],
            modifiers: null);

        if (method is null || method.ReturnType != typeof(TStrongId))
        {
            throw new InvalidOperationException($"Strong ID type '{typeof(TStrongId).FullName}' must expose a public static From({typeof(TValue).FullName}) method.");
        }

        return method;
    }
}

internal static class NullableStrongIdConversionExpressions<TStrongId, TValue>
    where TStrongId : struct, IStrongId<TValue>
    where TValue : struct
{
    private static readonly MethodInfo FromMethod = ResolveFromMethod();

    public static ValueConverter<TStrongId?, TValue?> CreateConverter()
    {
        return new ValueConverter<TStrongId?, TValue?>(ToProviderExpression(), FromProviderExpression());
    }

    public static ValueComparer<TStrongId?> CreateComparer()
    {
        return new ValueComparer<TStrongId?>(
            (left, right) => EqualityComparer<TStrongId?>.Default.Equals(left, right),
            strongId => strongId.HasValue ? EqualityComparer<TStrongId>.Default.GetHashCode(strongId.Value) : 0,
            strongId => strongId);
    }

    private static Expression<Func<TStrongId?, TValue?>> ToProviderExpression()
    {
        var strongId = Expression.Parameter(typeof(TStrongId?), "strongId");
        var hasValue = Expression.Property(strongId, nameof(Nullable<>.HasValue));
        var strongIdValue = Expression.Property(strongId, nameof(Nullable<>.Value));
        var providerValue = Expression.Property(strongIdValue, nameof(IStrongId<>.Value));
        var body = Expression.Condition(
            hasValue,
            Expression.Convert(providerValue, typeof(TValue?)),
            Expression.Constant(default(TValue?), typeof(TValue?)));

        return Expression.Lambda<Func<TStrongId?, TValue?>>(body, strongId);
    }

    private static Expression<Func<TValue?, TStrongId?>> FromProviderExpression()
    {
        var value = Expression.Parameter(typeof(TValue?), "value");
        var hasValue = Expression.Property(value, nameof(Nullable<>.HasValue));
        var nonNullableValue = Expression.Property(value, nameof(Nullable<>.Value));
        var createStrongId = Expression.Convert(Expression.Call(FromMethod, nonNullableValue), typeof(TStrongId?));
        var body = Expression.Condition(
            hasValue,
            createStrongId,
            Expression.Constant(default(TStrongId?), typeof(TStrongId?)));

        return Expression.Lambda<Func<TValue?, TStrongId?>>(body, value);
    }

    private static MethodInfo ResolveFromMethod()
    {
        var method = typeof(TStrongId).GetMethod(
            "From",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            [typeof(TValue)],
            modifiers: null);

        if (method is null || method.ReturnType != typeof(TStrongId))
        {
            throw new InvalidOperationException($"Strong ID type '{typeof(TStrongId).FullName}' must expose a public static From({typeof(TValue).FullName}) method.");
        }

        return method;
    }
}
