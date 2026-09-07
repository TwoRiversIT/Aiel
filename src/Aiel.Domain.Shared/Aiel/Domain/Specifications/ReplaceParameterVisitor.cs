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
using System.Linq.Expressions;

namespace Aiel.Domain.Specifications;

/******************************************************************************
* 
* This exists to make expression-tree composition work in a way that is
* compatible with EF Core.
* 
* Why it's needed:
* 
* - Each `Expression<Func<T, bool>>` has its own `ParameterExpression`.
* - When you combine two specs, you cannot just splice the bodies together if
*   they still refer to different parameters.
* - `ReplaceParameterVisitor` rewrites both expressions so they share one
*    common parameter before they're merged.
* 
* Where it's used:
* 
* - `EntitySpecification<TEntity>.CombineSpecification(...)`
* - That method combines two specs into a single 
*   `Expression<Func<TEntity, Boolean>>` that EF Core can understand and
*   translate.
* 
* Why that's useful:
* 
* - You can write small reusable specs like `IsActive`, `HasEmail`, 
*   `IsDeleted == false`.
* - Then combine them with `&`, `|`, and `!` without negatively impacting
*   query translation in EF Core.
* - This lets the same specification be used both in-memory and in database
*   queries.
* 
* Without this, you'd likely end up with expressions that compile, but fail or
* behave poorly when used with `IQueryable`/EF Core because the parameters
* don't line up cleanly.
* 
* In short: it's the glue that makes specification composition safe.
* 
******************************************************************************/

/// <summary>
/// Represents an expression visitor that replaces parameter expressions in an expression tree.
/// </summary>
internal class ReplaceParameterVisitor : ExpressionVisitor, IEnumerable<KeyValuePair<ParameterExpression, ParameterExpression>>
{
    private readonly Dictionary<ParameterExpression, ParameterExpression> _map = [];

    protected override Expression VisitParameter(ParameterExpression node)
        => _map.TryGetValue(node, out var newValue) ? newValue : node;

    public void Add(ParameterExpression parameterToReplace, ParameterExpression replaceWith)
        => _map.Add(parameterToReplace, replaceWith);

    public IEnumerator<KeyValuePair<ParameterExpression, ParameterExpression>> GetEnumerator()
        => _map.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
