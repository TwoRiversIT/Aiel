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

namespace Aiel.Actions;

/// <summary>
/// Provides the framework-owned implementation of an action-aware execution context.
/// </summary>
/// <typeparam name="TAction">The action payload type.</typeparam>
public sealed class ActionExecutionContext<TAction>(
    IActor actor,
    Guid operationId,
    Guid correlationId,
    Guid? causationId,
    Guid? clientInstanceId,
    IDictionary<String, Object?> properties,
    TAction action
    ) : ExecutionContextBase(actor, operationId, correlationId, causationId, clientInstanceId, properties), IActionExecutionContext<TAction>
    where TAction : IAction
{
    /// <inheritdoc />
    public TAction Action { get; } = action ?? throw new ArgumentNullException(nameof(action));

    /// <summary>
    /// Creates a child action execution context from an existing execution context and action payload.
    /// </summary>
    /// <param name="parent">The parent execution context.</param>
    /// <param name="action">The action payload for the child execution.</param>
    public static ActionExecutionContext<TAction> CreateChild(IExecutionContext parent, TAction action)
    {
        ArgumentNullException.ThrowIfNull(parent);
        ArgumentNullException.ThrowIfNull(action);

        var actor = parent.Actor
            ?? throw new ArgumentException("Execution context actor cannot be null.", nameof(parent));

        return new ActionExecutionContext<TAction>(
            actor: actor,
            operationId: Guid.NewGuid(),
            correlationId: EnsureNotEmpty(parent.CorrelationId, nameof(parent.CorrelationId)),
            causationId: EnsureNotEmpty(parent.OperationId, nameof(parent.OperationId)),
            clientInstanceId: parent.ClientInstanceId,
            properties: new Dictionary<String, Object?>(parent.Properties),
            action: action);
    }
}
