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

namespace Aiel.StrongIds.Internal;

public static class DiagnosticDescriptors
{
    /// <summary>
    /// AIEL00013 is raised when a strong ID declaration is not a partial record struct or a partial sealed record.
    /// </summary>
    public static readonly DiagnosticDescriptor StrongIdMustBePartialRecordType = new(
        id: DiagnosticRuleIDs.AIEL00013_MustBePartialRecordTypeId,
        title: "Strong ID declarations must be partial record types",
        messageFormat: "Strong ID type '{0}' must be declared as a partial record struct or a partial sealed record",
        category: DiagnosticMetadata.StrongIdCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Strong ID declarations must be partial record struct types or partial sealed record types so the generator can own the implementation surface.",
        customTags: []);

    /// <summary>
    /// AIEL00014 is raised when a strong ID declaration uses positional record syntax, which is not allowed because the generator owns the validating constructor and needs to control the parameter list.
    /// </summary>
    public static readonly DiagnosticDescriptor StrongIdMustNotUsePositionalRecordSyntax = new(
        id: DiagnosticRuleIDs.AIEL00014_MustNotUsePositionalRecordSyntaxId,
        title: "Strong ID declarations must not use positional record syntax",
        messageFormat: "Strong ID type '{0}' must not use positional record syntax",
        category: DiagnosticMetadata.StrongIdCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Strong ID declarations must omit positional record syntax because the generator owns the validating constructor.",
        customTags: []);

    /// <summary>
    /// AIEL00015 is raised when a strong ID declaration does not implement the IStrongId<TValue> interface with the same TValue specified in the StrongId attribute.
    /// </summary>
    public static readonly DiagnosticDescriptor StrongIdMustImplementMatchingInterface = new(
        id: DiagnosticRuleIDs.AIEL00015_MustImplementMatchingInterfaceId,
        title: "Strong ID declarations must implement IStrongId<TValue>",
        messageFormat: "Strong ID type '{0}' must implement IStrongId<{1}>",
        category: DiagnosticMetadata.StrongIdCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Strong ID declarations must implement IStrongId<TValue> with the same TValue used by the StrongId attribute.",
        customTags: []);

    /// <summary>
    /// AIEL00016 is raised when a strong ID declaration includes its own Value member, which is not allowed because the generator owns the Value member and will not generate code if a conflicting member exists.
    /// </summary>
    public static readonly DiagnosticDescriptor StrongIdMustNotDeclareValueMember = new(
        id: DiagnosticRuleIDs.AIEL00016_MustNotDeclareValueMemberId,
        title: "Strong ID declarations must not declare their own Value member",
        messageFormat: "Strong ID type '{0}' must not declare its own Value member",
        category: DiagnosticMetadata.StrongIdCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The Strong ID generator owns the Value member and will not generate code when a conflicting Value member exists.",
        customTags: []);

    /// <summary>
    /// AIEL00017 is raised when a strong ID declaration includes any instance constructors, which are not allowed because the generator owns all instance constructors and will not generate code if any instance constructor already exists.
    /// </summary>
    public static readonly DiagnosticDescriptor StrongIdMustNotDeclareInstanceConstructors = new(
        id: DiagnosticRuleIDs.AIEL00017_MustNotDeclareInstanceConstructorsId,
        title: "Strong ID declarations must not declare instance constructors",
        messageFormat: "Strong ID type '{0}' must not declare instance constructors",
        category: DiagnosticMetadata.StrongIdCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The Strong ID generator owns all instance constructors so it can enforce consistent validation.",
        customTags: []);

    /// <summary>
    /// AIEL00018 is raised when a strong ID declaration uses an unsupported backing type.
    /// </summary>
    public static readonly DiagnosticDescriptor StrongIdBackingTypeUnsupported = new(
        id: DiagnosticRuleIDs.AIEL00018_BackingTypeUnsupportedId,
        title: "Unsupported strong ID backing type",
        messageFormat: "Strong ID type '{0}' uses unsupported backing type '{1}'; supported backing types are Guid, Int32, Int64, and String",
        category: DiagnosticMetadata.StrongIdCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The Strong ID generator currently supports Guid, Int32, Int64, and String backing types.",
        customTags: []);
}
