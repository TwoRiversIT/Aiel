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

using Aiel.Queries;
using Aiel.Results;
using Microsoft.Extensions.Logging;

namespace Aiel.Actions.Queries;

/// <summary>
/// A query pipeline behavior that emits structured log entries before and after every
/// query dispatch, including the query type name, correlation ID, and outcome.
/// </summary>
/// <typeparam name="TQuery">The query type being dispatched.</typeparam>
/// <typeparam name="TResult">The result type returned by the query.</typeparam>
public sealed class QueryLoggingPipelineBehavior<TQuery, TResult>(
    ILogger<QueryLoggingPipelineBehavior<TQuery, TResult>> logger)
    : IQueryPipelineBehavior<TQuery, TResult>
    where TQuery : IQuery<TResult>
{
    /// <inheritdoc />
    public async Task<Result<TResult>> HandleAsync(
        TQuery query,
        IExecutionContext context,
        QueryPipelineHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        var inputTypeName = typeof(TQuery).Name;
        var correlationId = context.CorrelationId;

        logger.LogDispatching(inputTypeName, correlationId);

        var result = await next(cancellationToken);

        if (result.IsSuccess)
        {
            logger.LogSuccess(inputTypeName, correlationId);
        }
        else
        {
            logger.LogFailure(inputTypeName, correlationId);
        }

        return result;
    }
}
