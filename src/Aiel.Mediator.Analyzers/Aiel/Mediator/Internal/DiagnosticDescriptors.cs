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

using Aiel.Internal;
using Microsoft.CodeAnalysis;

namespace Aiel.Mediator.Internal;

public static class DiagnosticDescriptors
{
    /// <summary>
    /// AIEL00005 is raised when a single method body dispatches via ISender or IPublisher more than once.
    /// Each dispatch creates its own DI scope, so multiple calls in one method likely
    /// indicates that a cross-cutting concern (transaction, unit-of-work) should be
    /// pushed into a pipeline behavior instead.
    /// </summary>
    public static readonly DiagnosticDescriptor MultipleDispatchCallsInMethod = new(
        id: DiagnosticRuleIDs.AIEL00005_MultipleDispatchCallsInMethodId,
        title: "Multiple mediator dispatch calls in a single method",
        messageFormat: "Method '{0}' calls the mediator {1} times. Each call creates an independent DI scope; consider consolidating into a single command or using a pipeline behavior for cross-cutting coordination.",
        category: DiagnosticMetadata.UsageCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Each call to ISender.ExecuteAsync, ISender.QueryAsync, or IPublisher.PublishAsync creates its own DI scope. " +
                     "Multiple calls in one method body may indicate that related operations should be composed into a single command/query, " +
                     "or that a shared concern such as a transaction should be expressed as a pipeline behavior.",
        customTags: []);
}
