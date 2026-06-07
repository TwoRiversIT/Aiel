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

namespace Aiel.Authorization.Internal;

public static class DiagnosticDescriptors
{
    /// <summary>
    /// AIEL00006 is raised for any IAction implementation that does not have a corresponding
    /// IActionAuthorizationChecker<TAction> or is not marked with [DoesNotRespectAuthority].
    /// </summary>
    public static readonly DiagnosticDescriptor ActionHasNoAuthorizationStory = new(
        id: DiagnosticRuleIDs.AIEL00006_ActionHasNoAuthorizationStoryId,
        title: "Action type has no authorization story",
        messageFormat: "Action '{0}' has no concrete IActionAuthorizationChecker<{0}> and is not marked [DoesNotRespectAuthority]; every IAction implementation must have an authorization story. Note: generated permission definitions are recognized by the analyzer only after Aiel.Authorization.Generators is added.",
        category: DiagnosticMetadata.UsageCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Every IAction implementation must have either a concrete IActionAuthorizationChecker<TAction> in scope or be annotated [DoesNotRespectAuthority(Reason = \"...\")] to declare it permission-free. This rule is fail-closed: the absence of an authorization story is an error.",
        customTags: [WellKnownDiagnosticTags.NotConfigurable, WellKnownDiagnosticTags.CompilationEnd]);

    /// <summary>
    /// AIEL00007 is raised when an action marked with [DoesNotRespectAuthority] has an empty or whitespace-only Reason.
    /// </summary>
    public static readonly DiagnosticDescriptor DoesNotRespectAuthorityReasonIsEmpty = new(
        id: DiagnosticRuleIDs.AIEL00007_DoesNotRespectAuthorityReasonIsEmptyId,
        title: "[DoesNotRespectAuthority] Reason must not be empty or whitespace",
        messageFormat: "[DoesNotRespectAuthority] on action '{0}' has an empty or whitespace Reason; provide a non-empty justification",
        category: DiagnosticMetadata.UsageCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "[DoesNotRespectAuthority] is the explicit permission-free marker. Its Reason property must contain a non-empty, auditable justification.",
        customTags: WellKnownDiagnosticTags.CompilationEnd);
}
