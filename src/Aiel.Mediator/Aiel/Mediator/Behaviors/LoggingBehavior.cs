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

using Microsoft.Extensions.Logging;
using Aiel.Actions;
using Aiel.Results;
using System.Diagnostics;

namespace Aiel.Mediator.Behaviors;

/// <summary>
/// Logs the start, completion, and failure of a dispatched action.
/// </summary>
/// <typeparam name="TAction">The action type flowing through the pipeline.</typeparam>
public sealed class LoggingBehavior<TAction>(ILogger<LoggingBehavior<TAction>> logger)
    : IPipelineBehavior<TAction>
    where TAction : IAction
{
    /// <summary>
    /// Logs around the next pipeline step for the current action.
    /// </summary>
    /// <param name="request">The dispatched action being processed.</param>
    /// <param name="next">The next behavior or handler in the pipeline.</param>
    /// <param name="cancellationToken">The token that cancels the dispatch.</param>
    /// <returns>The result returned by the next pipeline step.</returns>
    /// <exception cref="Exception">Propagates any exception thrown by the next pipeline step after logging it.</exception>
    public async ValueTask<Result> HandleAsync(
        TAction request,
        ActionHandlerDelegate next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TAction).Name;
        logger.LogInformation("Handling {RequestName}", requestName);

        var sw = Stopwatch.StartNew();
        try
        {
            var response = await next();
            sw.Stop();
            logger.LogInformation("Handled {RequestName} in {Elapsed}ms", requestName, sw.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();
            logger.LogError(ex, "Handler {RequestName} threw after {Elapsed}ms", requestName, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
