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

using Aiel.Results;
using Aiel.Actions;
using Aiel.Actions.Commands;

namespace Aiel.Authorization;

/// <summary>
/// Pre-execution authorization gate for command dispatch.
/// </summary>
/// <remarks>
/// Short-circuits with a failure result when <see cref="IActionGate{TAction}.AuthorizeAsync"/>
/// denies the command.  Commands decorated with <see cref="DoesNotRespectAuthorityAttribute"/>
/// bypass the gate entirely and proceed directly to the handler.
/// Register this behavior first so authorization is checked before any other pipeline stage.
/// </remarks>
/// <typeparam name="TCommand">The command type being dispatched.</typeparam>
public sealed class ActionGateCommandPipelineBehavior<TCommand>(IActionGate<TCommand> gate)
    : ICommandPipelineBehavior<TCommand>
    where TCommand : ICommand
{
    // Cached at generic type instantiation time — one reflection call per command type, not per dispatch.
    private static readonly Boolean DoesNotRespectAuthority =
        typeof(TCommand).IsDefined(typeof(DoesNotRespectAuthorityAttribute), inherit: false);

    /// <inheritdoc />
    public async Task<Result> HandleAsync(
        TCommand command,
        IExecutionContext context,
        CommandPipelineHandlerDelegate next,
        CancellationToken cancellationToken = default)
    {
        if (DoesNotRespectAuthority)
        {
            return await next(cancellationToken);
        }

        var authResult = await gate.AuthorizeAsync(context, command, cancellationToken);
        if (!authResult.IsSuccess)
        {
            return Result.Failure(authResult.Error);
        }

        return await next(cancellationToken);
    }
}
