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
using Microsoft.Extensions.DependencyInjection;

namespace Aiel.Queries;

/// <summary>
/// The default <see cref="IQueryDispatcher"/> implementation.
/// Resolves <see cref="IQueryHandler{TQuery, TResult}"/> and any registered
/// <see cref="IQueryPipelineBehavior{TQuery, TResult}"/> instances from the DI container,
/// builds the pipeline chain in registration order (first registered = outermost),
/// and executes it within a child <see cref="DefaultExecutionContext"/>.
/// </summary>
public sealed class DefaultQueryDispatcher(IServiceProvider serviceProvider) : IQueryDispatcher
{
    /// <inheritdoc />
    public Task<Result<TResult>> DispatchAsync<TQuery, TResult>(
        TQuery query,
        IExecutionContext context,
        CancellationToken cancellationToken = default)
        where TQuery : IQuery<TResult>
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(context);

        var childContext = DefaultExecutionContext.CreateChild(context);
        var handler = serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResult>>();
        var behaviors = serviceProvider.GetServices<IQueryPipelineBehavior<TQuery, TResult>>().ToList();

        QueryPipelineHandlerDelegate<TResult> pipeline =
            ct => handler.HandleAsync(query, childContext, ct);

        for (var i = behaviors.Count - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var next = pipeline;
            pipeline = ct => behavior.HandleAsync(query, childContext, next, ct);
        }

        return pipeline(cancellationToken);
    }
}
