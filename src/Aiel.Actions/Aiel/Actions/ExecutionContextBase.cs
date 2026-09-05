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
/// Base class for execution contexts, providing common functionality for managing execution context properties and identifiers.
/// </summary>
public class ExecutionContextBase : IExecutionContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExecutionContextBase"/> class with the specified parameters.
    /// </summary>
    /// <param name="actor"></param>
    /// <param name="operationId"></param>
    /// <param name="correlationId"></param>
    /// <param name="timestamp"></param>
    /// <param name="causationId"></param>
    /// <param name="clientInstanceId"></param>
    /// <param name="properties"></param>
    /// <exception cref="ArgumentException"></exception>
    protected ExecutionContextBase(
        IActor actor,
        Guid operationId,
        Guid correlationId,
        DateTimeOffset timestamp,
        Guid? causationId,
        Guid? clientInstanceId,
        IDictionary<String, Object?> properties)
    {
        ArgumentNullException.ThrowIfNull(actor);

        Actor = actor;

        OperationId = EnsureNotEmpty(operationId, nameof(operationId));
        CorrelationId = EnsureNotEmpty(correlationId, nameof(correlationId));

        Timestamp = timestamp == default ? throw new ArgumentException("Timestamp cannot be the default value.", nameof(timestamp)) : timestamp;

        // Optional identifiers can be null, but if they are provided, they cannot be empty.
        ClientInstanceId = clientInstanceId == null ? null : EnsureNotEmpty(clientInstanceId.Value, nameof(clientInstanceId));
        CausationId = causationId == null ? null : EnsureNotEmpty(causationId.Value, nameof(causationId));

        Properties = properties ?? new Dictionary<String, Object?>();
    }

    /// <inheritdoc />
    public IActor Actor { get; }

    /// <inheritdoc />
    public Guid? CausationId { get; }

    /// <inheritdoc />
    public Guid? ClientInstanceId { get; }

    /// <inheritdoc />
    public Guid CorrelationId { get; }

    /// <inheritdoc />
    public Guid OperationId { get; }

    /// <inheritdoc />
    public DateTimeOffset Timestamp { get; }

    /// <inheritdoc />
    public IDictionary<String, Object?> Properties { get; }

    /// <summary>
    /// Ensures that the provided <see cref="Guid"/> value is not empty. If the value is empty, an <see cref="ArgumentException"/> is thrown.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="paramName"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    protected static Guid EnsureNotEmpty(Guid value, String paramName)
    {
        return value == Guid.Empty
            ? throw new ArgumentException("Execution context identifiers cannot be empty.", paramName)
            : value;
    }
}
