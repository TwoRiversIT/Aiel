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

using Aiel.Actions;

namespace Aiel.Execution;

/// <summary>
/// Provides the framework-owned implementation of an action-aware execution context.
/// </summary>
/// <typeparam name="TAction">The action payload type.</typeparam>
public sealed class ActionExecutionContext<TAction> : IActionExecutionContext<TAction>
    where TAction : IAction
{
    private ActionExecutionContext(
        Guid operationId,
        IActor actor,
        Guid correlationId,
        Guid? causationId,
        Guid? clientInstanceId,
        IDictionary<String, Object?> properties,
        TAction action)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(properties);
        ArgumentNullException.ThrowIfNull(action);

        OperationId = EnsureNotEmpty(operationId, nameof(operationId));
        Actor = actor;
        CorrelationId = EnsureNotEmpty(correlationId, nameof(correlationId));
        CausationId = causationId;
        ClientInstanceId = clientInstanceId;
        Properties = properties;
        Action = action;
    }

    /// <inheritdoc />
    public Guid OperationId { get; }

    /// <inheritdoc />
    public IActor Actor { get; }

    /// <inheritdoc />
    public Guid CorrelationId { get; }

    /// <inheritdoc />
    public Guid? CausationId { get; }

    /// <inheritdoc />
    public Guid? ClientInstanceId { get; }

    /// <inheritdoc />
    public IDictionary<String, Object?> Properties { get; }

    /// <inheritdoc />
    public TAction Action { get; }

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
            operationId: Guid.NewGuid(),
            actor: actor,
            correlationId: EnsureNotEmpty(parent.CorrelationId, nameof(parent.CorrelationId)),
            causationId: EnsureNotEmpty(parent.OperationId, nameof(parent.OperationId)),
            clientInstanceId: parent.ClientInstanceId,
            properties: new Dictionary<String, Object?>(parent.Properties),
            action: action);
    }

    private static Guid EnsureNotEmpty(Guid value, String parameterName)
    {
        return value == Guid.Empty
            ? throw new ArgumentException("Execution context identifiers cannot be empty.", parameterName)
            : value;
    }
}
