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

using Aiel.MultiTenancy;

namespace Aiel.MessageBus;

/// <summary>
/// Strongly-typed metadata carried on every transport message. Gives first-class names to
/// correlation, causation, actor, tenant, and message identifiers instead of hiding them
/// in transport headers.
/// </summary>
public sealed record MessageMetadata(
    Guid MessageId,
    Guid CorrelationId,
    Guid? CausationMessageId,
    Guid? ProducerOperationId,
    Guid? ClientInstanceId,
    MessageActorSnapshot Actor,
    TenantIdentity? Tenant,
    SagaId? SagaCorrelationId,
    DateTimeOffset OccurredAtUtc,
    IReadOnlyDictionary<MessagePropertyName, String> Properties)
{
    public Guid MessageId { get; init; } = EnsureNotEmpty(MessageId, nameof(MessageId));
    public Guid CorrelationId { get; init; } = EnsureNotEmpty(CorrelationId, nameof(CorrelationId));

    private static Guid EnsureNotEmpty(Guid value, String paramName)
        => value == Guid.Empty
            ? throw new ArgumentException("Message identifiers cannot be empty.", paramName)
            : value;
}
