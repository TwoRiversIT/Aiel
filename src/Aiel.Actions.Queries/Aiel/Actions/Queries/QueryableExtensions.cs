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

using System.Linq.Expressions;

namespace Aiel.Actions.Queries;

public static class QueryableExtensions
{
    private const String OrderBy = nameof(Queryable.OrderBy);
    private const String ThenBy = nameof(Queryable.ThenBy);
    private const String OrderByDescending = nameof(Queryable.OrderByDescending);
    private const String ThenByDescending = nameof(Queryable.ThenByDescending);

    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> source, PageRequest page)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(page);

        return source.Skip(page.Offset).Take(page.Size);
    }

    public static IQueryable<T> ApplySorting<T>(this IQueryable<T> source, SortRequest sort)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(sort);

        if (!sort.HasValues)
        {
            return source;
        }

        var expression = source.Expression;
        var count = 0;

        foreach (var field in sort.Fields)
        {
            try
            {
                var parameter = Expression.Parameter(typeof(T), "x");
                var selector = Expression.PropertyOrField(parameter, field.Name);
                var method = field.Direction == SortDirection.Descending
                    ? (count == 0 ? OrderByDescending : ThenByDescending)
                    : (count == 0 ? OrderBy : ThenBy);

                expression = Expression.Call(
                    typeof(Queryable),
                    method,
                    [source.ElementType, selector.Type],
                    expression,
                    Expression.Quote(Expression.Lambda(selector, parameter)));

                count++;
            }
            catch (ArgumentException)
            {
                // Ignore invalid property names so application-level validation can decide how strict to be.
            }
        }

        return count > 0 ? source.Provider.CreateQuery<T>(expression) : source;
    }
}
