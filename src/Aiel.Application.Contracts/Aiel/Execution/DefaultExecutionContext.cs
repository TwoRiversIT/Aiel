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

namespace Aiel.Execution;

/// <summary>
/// The default implementation of <see cref="IExecutionContext"/>.
/// Identity properties are set once at construction; the <see cref="Properties"/> dictionary is
/// openly mutable for the lifetime of the context.
/// </summary>
public sealed class DefaultExecutionContext : IExecutionContext
{
    private DefaultExecutionContext(
        IActor actor,
        Guid operationId,
        Guid correlationId,
        Guid? causationId,
        Guid? clientInstanceId)
    {
        ArgumentNullException.ThrowIfNull(actor);

        Actor = actor;
        OperationId = EnsureNotEmpty(operationId, nameof(operationId));
        CorrelationId = EnsureNotEmpty(correlationId, nameof(correlationId));
        CausationId = causationId;
        ClientInstanceId = clientInstanceId;
    }

    /// <inheritdoc />
    public IActor Actor { get; }

    /// <inheritdoc />
    public Guid OperationId { get; }

    /// <inheritdoc />
    public Guid CorrelationId { get; }

    /// <inheritdoc />
    public Guid? CausationId { get; }

    /// <inheritdoc />
    public Guid? ClientInstanceId { get; }

    /// <inheritdoc />
    public IDictionary<String, Object?> Properties { get; } = new Dictionary<String, Object?>();

    /// <summary>
    /// Creates a root execution context, optionally anchoring it to an existing correlation or client instance.
    /// </summary>
    /// <param name="correlationId">
    /// The correlation ID for the request chain.  When <see langword="null"/>, a new ID is generated
    /// and used as both the <see cref="OperationId"/> and the <see cref="CorrelationId"/>.
    /// </param>
    /// <param name="clientInstanceId">An optional identifier for the originating client instance.</param>
    public static DefaultExecutionContext CreateRoot(
        IActor actor,
        Guid? correlationId = null,
        Guid? clientInstanceId = null)
    {
        ArgumentNullException.ThrowIfNull(actor);

        var operationId = Guid.NewGuid();

        return new DefaultExecutionContext(
            actor,
            operationId,
            correlationId ?? operationId,
            causationId: null,
            clientInstanceId);
    }

    /// <summary>
    /// Creates a root execution context for system-initiated work.
    /// </summary>
    public static DefaultExecutionContext CreateRoot(
        Guid? correlationId = null,
        Guid? clientInstanceId = null)
    {
        return CreateRoot(SystemActor.Instance, correlationId, clientInstanceId);
    }

    /// <summary>
    /// Creates a child execution context that inherits the correlation chain from <paramref name="parent"/>.
    /// The child receives a new <see cref="OperationId"/>; the parent’s <see cref="OperationId"/> becomes
    /// the child’s <see cref="CausationId"/>.
    /// </summary>
    /// <param name="parent">The parent context from which causation information is derived.</param>
    public static DefaultExecutionContext CreateChild(IExecutionContext parent)
    {
        ArgumentNullException.ThrowIfNull(parent);

        var actor = parent.Actor
            ?? throw new ArgumentException("Execution context actor cannot be null.", nameof(parent));

        return new DefaultExecutionContext(
            actor,
            operationId: Guid.NewGuid(),
            correlationId: EnsureNotEmpty(parent.CorrelationId, nameof(parent.CorrelationId)),
            causationId: EnsureNotEmpty(parent.OperationId, nameof(parent.OperationId)),
            clientInstanceId: parent.ClientInstanceId);
    }

    private static Guid EnsureNotEmpty(Guid value, String paramName)
    {
        return value == Guid.Empty
            ? throw new ArgumentException("Execution context identifiers cannot be empty.", paramName)
            : value;
    }
}
