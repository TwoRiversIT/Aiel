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

using Aiel.Execution;
using Aiel.Results;

namespace Aiel.Queries;

/// <summary>
/// Represents the next stage in a query pipeline.  The query and execution context are
/// captured by the closure when the chain is constructed; behaviors call this delegate to
/// pass control to the next registered behavior, or to the handler itself when no more
/// behaviors remain.
/// </summary>
/// <typeparam name="TResult">The result type returned by the query.</typeparam>
public delegate Task<Result<TResult>> QueryPipelineHandlerDelegate<TResult>(
    CancellationToken cancellationToken = default);

/// <summary>
/// Defines a cross-cutting behavior that wraps query dispatch.
/// </summary>
/// <remarks>
/// Behaviors are executed in registration order: the first behavior registered is the outermost
/// wrapper and runs first.  A behavior MUST NOT also implement
/// <see cref="Commands.ICommandPipelineBehavior{TCommand}"/>.
/// <para>
/// Register behaviors with open-generic DI to apply them to every query type:
/// <code>
/// services.AddTransient(
///     typeof(IQueryPipelineBehavior&lt;,&gt;),
///     typeof(MyQueryBehavior&lt;,&gt;));
/// </code>
/// </para>
/// </remarks>
/// <typeparam name="TQuery">The query type this behavior wraps.</typeparam>
/// <typeparam name="TResult">The result type returned by the query.</typeparam>
public interface IQueryPipelineBehavior<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Executes this behavior, invoking <paramref name="next"/> to continue the pipeline.
    /// </summary>
    /// <param name="query">The query being dispatched.</param>
    /// <param name="context">
    /// The execution context for this dispatch.  Behaviors may read and write
    /// <see cref="IExecutionContext.Properties"/> to communicate with downstream stages.
    /// </param>
    /// <param name="next">Delegate that invokes the next stage in the pipeline.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<Result<TResult>> HandleAsync(
        TQuery query,
        IExecutionContext context,
        QueryPipelineHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default);
}
