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
using Aiel.Execution;
using Aiel.Results;

namespace Aiel.Commands;

/// <summary>
/// The default <see cref="ICommandDispatcher"/> implementation.
/// Resolves <see cref="ICommandHandler{TCommand}"/> and any registered
/// <see cref="ICommandPipelineBehavior{TCommand}"/> instances from the DI container,
/// builds the pipeline chain in registration order (first registered = outermost),
/// and executes it within a child <see cref="DefaultExecutionContext"/>.
/// </summary>
public sealed class DefaultCommandDispatcher(IServiceProvider serviceProvider) : ICommandDispatcher
{
    /// <inheritdoc />
    public Task<Result> DispatchAsync<TCommand>(
        TCommand command,
        IExecutionContext context,
        CancellationToken cancellationToken = default)
        where TCommand : ICommand
    {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentNullException.ThrowIfNull(context);

        var childContext = DefaultExecutionContext.CreateChild(context);
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        var behaviors = serviceProvider.GetServices<ICommandPipelineBehavior<TCommand>>().ToList();

        CommandPipelineHandlerDelegate pipeline =
            ct => handler.HandleAsync(command, childContext, ct);

        for (var i = behaviors.Count - 1; i >= 0; i--)
        {
            var behavior = behaviors[i];
            var next = pipeline;
            pipeline = ct => behavior.HandleAsync(command, childContext, next, ct);
        }

        return pipeline(cancellationToken);
    }
}
