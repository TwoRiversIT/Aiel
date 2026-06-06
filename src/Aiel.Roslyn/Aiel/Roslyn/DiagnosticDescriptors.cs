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

using Microsoft.CodeAnalysis;

namespace Aiel.Roslyn;

// IMPORTANT: All diagnostic descriptor IDs must start with the canonical "AIEL" prefix.
// Never use any other prefix (e.g. TRAF, TRSG, TRMD). See GitHub issue #7.
internal static class DiagnosticDescriptors
{
    public const String AIEL00001_RootDependencyRequiredId = "AIEL00001";
    public const String AIEL00002_ErrorTypesMustHaveSingleStringConstructorId = "AIEL00002";
    public const String AIEL00003_PreferResultHttpClientExtensionsId = "AIEL00003";
    public const String AIEL00004_AmbiguousProjectTypeDiagnosticId = "AIEL00004";
    public const String AIEL00005_MultipleDispatchCallsInMethodId = "AIEL00005";
    public const String AIEL00006_ActionHasNoAuthorizationStoryId = "AIEL00006";
    public const String AIEL00007_DoesNotRespectAuthorityReasonIsEmptyId = "AIEL00007";
    public const String AIEL00008_UseAielEventIdsId = "AIEL00008";
    public const String AIEL00009_MissingEventIdParameterId = "AIEL00009";
    public const String AIEL00010_MissingEventIdInMessageId = "AIEL00010";
    public const String AIEL00011_NoDirectILoggerCallsId = "AIEL00011";
    public const String AIEL00012_EventIdMismatchId = "AIEL00012";
    public const String AIEL00013_MustBePartialRecordTypeId = "AIEL00013";
    public const String AIEL00014_MustNotUsePositionalRecordSyntaxId = "AIEL00014";
    public const String AIEL00015_MustImplementMatchingInterfaceId = "AIEL00015";
    public const String AIEL00016_MustNotDeclareValueMemberId = "AIEL00016";
    public const String AIEL00017_MustNotDeclareInstanceConstructorsId = "AIEL00017";
    public const String AIEL00018_BackingTypeUnsupportedId = "AIEL00018";

    // Common category and help link base for Aiel logging analyzers
    private const String HelpBase = "https://docs.aiel.ca/analyzers/";
    private const String LoggingCategory = "AielLogging";
    private const String StrongIdCategory = "AielStrongId";
    private const String UsageCategory = "AielUsage";

    /// <summary>
    /// AIEL00001 is raised when an assembly referencing Aiel does not declare exactly one public,
    /// non-abstract <see cref="Dependencies.AielDependencyConfigurator"/> subclass or 
    /// <see cref="Dependencies.AielApplication"/> subclass with a
    /// public parameterless constructor. This type serves as the logical root for the
    /// assembly's participation in application configuration, initialization, and
    /// dependency resolution.
    /// </summary>
    public static readonly DiagnosticDescriptor RootDependencyRequired = new(
        id: AIEL00001_RootDependencyRequiredId,
        title: "Assemblies referencing the `Aiel` NuGet package directly or transitively must declare their dependency type, either `AielDependencyConfigurator` or `AielApplication`",
        messageFormat: "The '{0}' assembly must declare exactly one public sealed class with a public parameterless constructor that inherits `AielApplication` or `AielDependencyConfigurator`",
        category: UsageCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Any assembly that references Aiel directly or transitively must define exactly one public sealed class with a public parameterless constructor, inheriting from either `Aiel.Dependencies.AielDependencyConfigurator` or `Aiel.Dependencies.AielApplication`. These types serve as the root for the dependency graph.",
        customTags: []);

    /// <summary>
    /// AIEL00002 is raised when the generator detects an error type that does not have exactly one public constructor accepting a single string parameter for the error message.
    /// </summary>
    public static readonly DiagnosticDescriptor DerivedErrorTypesMustHaveSingleStringConstructor = new(
        id: AIEL00002_ErrorTypesMustHaveSingleStringConstructorId,
        title: "Derived error types must have a single string constructor",
        messageFormat: "Error type '{0}' must have a single string constructor accepting the error message",
        category: UsageCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "All error types derived from Error must have exactly one public constructor that accepts a single string parameter for the error message.",
        customTags: []
    );

    /// <summary>
    /// AIEL00003 is raised when the generator detects a call to a generic HttpClient JSON extension method
    /// </summary>
    public static readonly DiagnosticDescriptor Prefer_ResultHttpClientExtensions = new(
        id: AIEL00003_PreferResultHttpClientExtensionsId,
        title: "Use ResultHttpClientExtensions for Result types",
        messageFormat: "Use ResultHttpClientExtensions methods instead of generic HttpClient JSON methods for Result deserialization",
        category: UsageCategory,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "The ResultHttpClientExtensions class provides specialized methods for working with Result and Result<T> types. These methods ensure proper configuration of JSON serialization options. See the README.md documentation for available methods like GetResultAsync<T>, PostAndReturnResultAsync<TRequest, TResponse>, etc.",
        customTags: []
    );

    /// <summary>
    /// AIEL00004 is raised when the generator detects multiple hosting types in the same assembly.
    /// </summary>
    public static readonly DiagnosticDescriptor AmbiguousProjectType = new(
        id: AIEL00004_AmbiguousProjectTypeDiagnosticId,
        title: "Unable to determine a single Aiel project type",
        messageFormat: "The generator detected multiple project types in the same assembly. Exactly one of WebAssembly, WebApplication, or HostApplication is supported per assembly.",
        category: UsageCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The generator detected multiple project types in the same assembly. Exactly one of WebAssembly, WebApplication, or HostApplication is supported per assembly.",
        customTags: []);

    /// <summary>
    /// AIEL00005 is raised when a single method body dispatches via ISender or IPublisher more than once.
    /// Each dispatch creates its own DI scope, so multiple calls in one method likely
    /// indicates that a cross-cutting concern (transaction, unit-of-work) should be
    /// pushed into a pipeline behavior instead.
    /// </summary>
    public static readonly DiagnosticDescriptor MultipleDispatchCallsInMethod = new(
        id: AIEL00005_MultipleDispatchCallsInMethodId,
        title: "Multiple mediator dispatch calls in a single method",
        messageFormat: "Method '{0}' calls the mediator {1} times. Each call creates an independent DI scope; consider consolidating into a single command or using a pipeline behavior for cross-cutting coordination.",
        category: UsageCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Each call to ISender.ExecuteAsync, ISender.QueryAsync, or IPublisher.PublishAsync creates its own DI scope. " +
                     "Multiple calls in one method body may indicate that related operations should be composed into a single command/query, " +
                     "or that a shared concern such as a transaction should be expressed as a pipeline behavior.",
        customTags: []);

    /// <summary>
    /// AIEL00006 is raised for any IAction implementation that does not have a corresponding
    /// IActionAuthorizationChecker<TAction> or is not marked with [DoesNotRespectAuthority].
    /// </summary>
    public static readonly DiagnosticDescriptor ActionHasNoAuthorizationStory = new(
        id: AIEL00006_ActionHasNoAuthorizationStoryId,
        title: "Action type has no authorization story",
        messageFormat: "Action '{0}' has no concrete IActionAuthorizationChecker<{0}> and is not marked [DoesNotRespectAuthority]; every IAction implementation must have an authorization story. Note: generated permission definitions are recognized by the analyzer only after Aiel.Authorization.Generators is added.",
        category: UsageCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Every IAction implementation must have either a concrete IActionAuthorizationChecker<TAction> in scope or be annotated [DoesNotRespectAuthority(Reason = \"...\")] to declare it permission-free. This rule is fail-closed: the absence of an authorization story is an error.",
        customTags: [WellKnownDiagnosticTags.NotConfigurable, WellKnownDiagnosticTags.CompilationEnd]);

    /// <summary>
    /// AIEL00007 is raised when an action marked with [DoesNotRespectAuthority] has an empty or whitespace-only Reason.
    /// </summary>
    public static readonly DiagnosticDescriptor DoesNotRespectAuthorityReasonIsEmpty = new(
        id: AIEL00007_DoesNotRespectAuthorityReasonIsEmptyId,
        title: "[DoesNotRespectAuthority] Reason must not be empty or whitespace",
        messageFormat: "[DoesNotRespectAuthority] on action '{0}' has an empty or whitespace Reason; provide a non-empty justification",
        category: UsageCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "[DoesNotRespectAuthority] is the explicit permission-free marker. Its Reason property must contain a non-empty, auditable justification.",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    /// <summary>
    /// AIEL00008 is raised when a <c>[LoggerMessage]</c> attribute's <c>EventId</c> argument
    /// is a raw integer literal rather than a cast of an <c>AielEventIds</c>
    /// member (e.g. <c>(int)AielEventIds.ModuleStart</c>).
    /// </summary>
    public static readonly DiagnosticDescriptor UseAielEventIds = new(
        id: AIEL00008_UseAielEventIdsId,
        title: "Use AielEventIds enum for event IDs",
        messageFormat: "EventId '{0}' is a raw integer. Use '(int)AielEventIds.{1}' instead.",
        category: LoggingCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "All LoggerMessage event IDs must reference AielEventIds enum members so that IDs remain consistent across the framework.",
        helpLinkUri: HelpBase + "AIEL00008");

    /// <summary>
    /// AIEL00009 is raised when an Aiel logging helper method does not include an optional
    /// <c>AielEventIds eventId = AielEventIds.X</c> parameter.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingEventIdParameter = new(
        id: AIEL00009_MissingEventIdParameterId,
        title: "Logging helper missing AielEventIds parameter",
        messageFormat: "Method '{0}' is a logging helper but is missing an optional 'AielEventIds eventId = AielEventIds.{1}' parameter",
        category: LoggingCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Every Aiel logging helper method must accept an optional AielEventIds parameter so callers can override the default event ID at call sites.",
        helpLinkUri: HelpBase + "AIEL00009");

    /// <summary>
    /// AIEL00010 is raised when a <c>[LoggerMessage]</c> message template does not contain
    /// the exact <c>[{EventId}]</c> placeholder.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingEventIdInMessage = new(
        id: AIEL00010_MissingEventIdInMessageId,
        title: "Log message template missing [{EventId}] placeholder",
        messageFormat: "Message template '{0}' does not contain the '[{{EventId}}]' placeholder, or it is not formatted correctly. Note that it must be in square brackets.",
        category: LoggingCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Aiel log message templates must include '[{EventId}]' so structured log consumers can filter and correlate events.",
        helpLinkUri: HelpBase + "AIEL00010");

    /// <summary>
    /// AIEL00011 is raised when production code calls <c>ILogger</c> extension methods
    /// (e.g. <c>LogInformation</c>) directly instead of going through an
    /// Aiel logging helper decorated with <c>[LoggerMessage]</c>.
    /// </summary>
    public static readonly DiagnosticDescriptor NoDirectILoggerCalls = new(
        id: AIEL00011_NoDirectILoggerCallsId,
        title: "Do not call ILogger methods directly",
        messageFormat: "Direct call to ILogger.{0}() bypasses Aiel logging conventions. Use a [LoggerMessage]-decorated helper instead.",
        category: LoggingCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Calling ILogger extension methods directly prevents structured event-ID tracking and consistent message formatting.",
        helpLinkUri: HelpBase + "AIEL00011");

    /// <summary>
    /// AIEL00012 is raised when the numeric EventId in a <c>[LoggerMessage]</c>
    /// attribute does not match the default value of the method's
    /// <c>AielEventIds eventId</c> parameter.
    /// </summary>
    public static readonly DiagnosticDescriptor EventIdMismatch = new(
        id: AIEL00012_EventIdMismatchId,
        title: "EventId mismatch between attribute and default parameter",
        messageFormat: "The [LoggerMessage] EventId ({0}) does not match the default value of parameter 'eventId' ({1}). They must agree.",
        category: LoggingCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The numeric EventId declared in [LoggerMessage] must match the AielEventIds member used as the default value for the 'eventId' parameter.",
        helpLinkUri: HelpBase + "AIEL00012");

    /// <summary>
    /// AIEL00013 is raised when a strong ID declaration is not a partial record struct or a partial sealed record.
    /// </summary>
    public static readonly DiagnosticDescriptor StrongIdMustBePartialRecordType = new(
        id: AIEL00013_MustBePartialRecordTypeId,
        title: "Strong ID declarations must be partial record types",
        messageFormat: "Strong ID type '{0}' must be declared as a partial record struct or a partial sealed record",
        category: StrongIdCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Strong ID declarations must be partial record struct types or partial sealed record types so the generator can own the implementation surface.",
        customTags: []);

    /// <summary>
    /// AIEL00014 is raised when a strong ID declaration uses positional record syntax, which is not allowed because the generator owns the validating constructor and needs to control the parameter list.
    /// </summary>
    public static readonly DiagnosticDescriptor StrongIdMustNotUsePositionalRecordSyntax = new(
        id: AIEL00014_MustNotUsePositionalRecordSyntaxId,
        title: "Strong ID declarations must not use positional record syntax",
        messageFormat: "Strong ID type '{0}' must not use positional record syntax",
        category: StrongIdCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Strong ID declarations must omit positional record syntax because the generator owns the validating constructor.",
        customTags: []);

    /// <summary>
    /// AIEL00015 is raised when a strong ID declaration does not implement the IStrongId<TValue> interface with the same TValue specified in the StrongId attribute.
    /// </summary>
    public static readonly DiagnosticDescriptor StrongIdMustImplementMatchingInterface = new(
        id: AIEL00015_MustImplementMatchingInterfaceId,
        title: "Strong ID declarations must implement IStrongId<TValue>",
        messageFormat: "Strong ID type '{0}' must implement IStrongId<{1}>",
        category: StrongIdCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Strong ID declarations must implement IStrongId<TValue> with the same TValue used by the StrongId attribute.",
        customTags: []);

    /// <summary>
    /// AIEL00016 is raised when a strong ID declaration includes its own Value member, which is not allowed because the generator owns the Value member and will not generate code if a conflicting member exists.
    /// </summary>
    public static readonly DiagnosticDescriptor StrongIdMustNotDeclareValueMember = new(
        id: AIEL00016_MustNotDeclareValueMemberId,
        title: "Strong ID declarations must not declare their own Value member",
        messageFormat: "Strong ID type '{0}' must not declare its own Value member",
        category: StrongIdCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The Strong ID generator owns the Value member and will not generate code when a conflicting Value member exists.",
        customTags: []);

    /// <summary>
    /// AIEL00017 is raised when a strong ID declaration includes any instance constructors, which are not allowed because the generator owns all instance constructors and will not generate code if any instance constructor already exists.
    /// </summary>
    public static readonly DiagnosticDescriptor StrongIdMustNotDeclareInstanceConstructors = new(
        id: AIEL00017_MustNotDeclareInstanceConstructorsId,
        title: "Strong ID declarations must not declare instance constructors",
        messageFormat: "Strong ID type '{0}' must not declare instance constructors",
        category: StrongIdCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The Strong ID generator owns all instance constructors so it can enforce consistent validation.",
        customTags: []);

    /// <summary>
    /// AIEL00018 is raised when a strong ID declaration uses an unsupported backing type.
    /// </summary>
    public static readonly DiagnosticDescriptor StrongIdBackingTypeUnsupported = new(
        id: AIEL00018_BackingTypeUnsupportedId,
        title: "Unsupported strong ID backing type",
        messageFormat: "Strong ID type '{0}' uses unsupported backing type '{1}'; supported backing types are Guid, Int32, Int64, and String",
        category: StrongIdCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The Strong ID generator currently supports Guid, Int32, Int64, and String backing types.",
        customTags: []);
}
