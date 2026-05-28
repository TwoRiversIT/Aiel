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

namespace Aiel.Commands;

/// <summary>
/// Represents the next stage in a command pipeline.  The command and execution context are
/// captured by the closure when the chain is constructed; behaviors call this delegate to
/// pass control to the next registered behavior, or to the handler itself when no more
/// behaviors remain.
/// </summary>
public delegate Task<Result> CommandPipelineHandlerDelegate(
    CancellationToken cancellationToken = default);

/// <summary>
/// Defines a cross-cutting behavior that wraps command dispatch.
/// </summary>
/// <remarks>
/// Behaviors are executed in registration order: the first behavior registered is the outermost
/// wrapper and runs first.  A behavior MUST NOT also implement
/// <see cref="Queries.IQueryPipelineBehavior{TQuery, TResult}"/>.
/// <para>
/// Register behaviors with open-generic DI to apply them to every command type:
/// <code>
/// services.AddTransient(
///     typeof(ICommandPipelineBehavior&lt;&gt;),
///     typeof(MyCommandBehavior&lt;&gt;));
/// </code>
/// Add a <c>where TCommand : ICommand</c> constraint on the implementation class to restrict
/// the behavior to a specific command type or subset.
/// </para>
/// </remarks>
/// <typeparam name="TCommand">The command type this behavior wraps.</typeparam>
public interface ICommandPipelineBehavior<TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Executes this behavior, invoking <paramref name="next"/> to continue the pipeline.
    /// </summary>
    /// <param name="command">The command being dispatched.</param>
    /// <param name="context">
    /// The execution context for this dispatch.  Behaviors may read and write
    /// <see cref="IExecutionContext.Properties"/> to communicate with downstream stages.
    /// </param>
    /// <param name="next">Delegate that invokes the next stage in the pipeline.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<Result> HandleAsync(
        TCommand command,
        IExecutionContext context,
        CommandPipelineHandlerDelegate next,
        CancellationToken cancellationToken = default);
}
