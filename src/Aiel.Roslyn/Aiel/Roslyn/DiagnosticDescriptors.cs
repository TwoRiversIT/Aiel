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
    // Aiel.Analyzers IDs
    public const String RootDependencyRequiredId = "AIEL00001";
    public const String ErrorTypesMustHaveSingleStringConstructorId = "AIEL00002";
    public const String PreferResultHttpClientExtensionsId = "AIEL00003";
    public const String AmbiguousProjectTypeDiagnosticId = "AIEL00004";
    public const String MultipleDispatchCallsInMethodId = "AIEL00005";

    // Aiel.Authorization.Analyzers IDs
    public const String ActionHasNoAuthorizationStoryId = "AIEL20001";
    public const String DoesNotRespectAuthorityReasonIsEmptyId = "AIEL20002";

    // Aiel.Logging.Analyzers IDs
    public const String UseAielEventIdsId = "AIEL001";
    public const String MissingEventIdParameterId = "AIEL002";
    public const String MissingEventIdInMessageId = "AIEL003";
    public const String NoDirectILoggerCallsId = "AIEL004";
    public const String EventIdMismatchId = "AIEL005";

    // Aiel.StrongIds.Generators IDs
    public const String StrongIdMustBePartialRecordTypeId = "AIEL10001";
    public const String StrongIdMustNotUsePositionalRecordSyntaxId = "AIEL10002";
    public const String StrongIdMustImplementMatchingInterfaceId = "AIEL10003";
    public const String StrongIdMustNotDeclareValueMemberId = "AIEL10004";
    public const String StrongIdMustNotDeclareInstanceConstructorsId = "AIEL10005";
    public const String StrongIdBackingTypeUnsupportedId = "AIEL10006";

    // Common category and help link base for Aiel logging analyzers
    private const String HelpBase = "https://docs.aiel.io/analyzers/";
    private const String AuthorizationCategory = "Authorization";
    private const String LoggingCategory = "AielLogging";
    private const String UsageCategory = "Usage";

    public static readonly DiagnosticDescriptor ActionHasNoAuthorizationStory = new(
        id: ActionHasNoAuthorizationStoryId,
        title: "Action type has no authorization story",
        messageFormat: "Action '{0}' has no concrete IActionAuthorizationChecker<{0}> and is not marked [DoesNotRespectAuthority]; every IAction implementation must have an authorization story. Note: generated permission definitions are recognized by the analyzer only after Aiel.Authorization.Generators is added.",
        category: AuthorizationCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Every IAction implementation must have either a concrete IActionAuthorizationChecker<TAction> in scope or be annotated [DoesNotRespectAuthority(Reason = \"...\")] to declare it permission-free. This rule is fail-closed: the absence of an authorization story is an error.",
        customTags: [WellKnownDiagnosticTags.NotConfigurable, WellKnownDiagnosticTags.CompilationEnd]);

    public static readonly DiagnosticDescriptor DoesNotRespectAuthorityReasonIsEmpty = new(
        id: DoesNotRespectAuthorityReasonIsEmptyId,
        title: "[DoesNotRespectAuthority] Reason must not be empty or whitespace",
        messageFormat: "[DoesNotRespectAuthority] on action '{0}' has an empty or whitespace Reason; provide a non-empty justification",
        category: AuthorizationCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "[DoesNotRespectAuthority] is the explicit permission-free marker. Its Reason property must contain a non-empty, auditable justification.",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    /// <summary>
    /// Any assembly that references Aiel must declare exactly one public,
    /// non-abstract <see cref="Dependencies.AielDependencyConfigurator"/> subclass with a
    /// public parameterless constructor. This type serves as the logical root for the
    /// assembly's participation in application configuration, initialization, and
    /// dependency resolution.
    /// </summary>
    public static readonly DiagnosticDescriptor RootDependencyRequired = new(
        id: RootDependencyRequiredId,
        title: "Assemblies referencing the `Aiel` NuGet package directly or transitively must declare their dependency type, either `AielDependencyConfigurator` or `AielApplication`",
        messageFormat: "The '{0}' assembly must declare exactly one public sealed class with a public parameterless constructor that inherits `AielApplication` or `AielDependencyConfigurator`",
        category: UsageCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Any assembly that references Aiel directly or transitively must define exactly one public sealed class with a public parameterless constructor, inheriting from either `Aiel.Dependencies.AielDependencyConfigurator` or `Aiel.Dependencies.AielApplication`. These types serve as the root for the dependency graph.",
        customTags: []);

    public static readonly DiagnosticDescriptor DerivedErrorTypesMustHaveSingleStringConstructor = new(
        id: ErrorTypesMustHaveSingleStringConstructorId,
        title: "Derived error types must have a single string constructor",
        messageFormat: "Error type '{0}' must have a single string constructor accepting the error message",
        category: UsageCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "All error types derived from Error must have exactly one public constructor that accepts a single string parameter for the error message.",
        customTags: []
    );

    public static readonly DiagnosticDescriptor Prefer_ResultHttpClientExtensions = new(
        id: PreferResultHttpClientExtensionsId,
        title: "Use ResultHttpClientExtensions for Result types",
        messageFormat: "Use ResultHttpClientExtensions methods instead of generic HttpClient JSON methods for Result deserialization",
        category: UsageCategory,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "The ResultHttpClientExtensions class provides specialized methods for working with Result and Result<T> types. These methods ensure proper configuration of JSON serialization options. See the README.md documentation for available methods like GetResultAsync<T>, PostAndReturnResultAsync<TRequest, TResponse>, etc.",
        customTags: []
    );

    public static readonly DiagnosticDescriptor AmbiguousProjectType = new(
        id: AmbiguousProjectTypeDiagnosticId,
        title: "Unable to determine a single Aiel project type",
        messageFormat: "The generator detected multiple project types in the same assembly. Exactly one of WebAssembly, WebApplication, or HostApplication is supported per assembly.",
        category: UsageCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The generator detected multiple project types in the same assembly. Exactly one of WebAssembly, WebApplication, or HostApplication is supported per assembly.",
        customTags: []);

    public static readonly DiagnosticDescriptor StrongIdMustBePartialRecordType = new(
        id: StrongIdMustBePartialRecordTypeId,
        title: "Strong ID declarations must be partial record types",
        messageFormat: "Strong ID type '{0}' must be declared as a partial record struct or a partial sealed record",
        category: UsageCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Strong ID declarations must be partial record struct types or partial sealed record types so the generator can own the implementation surface.",
        customTags: []);

    public static readonly DiagnosticDescriptor StrongIdMustNotUsePositionalRecordSyntax = new(
        id: StrongIdMustNotUsePositionalRecordSyntaxId,
        title: "Strong ID declarations must not use positional record syntax",
        messageFormat: "Strong ID type '{0}' must not use positional record syntax",
        category: UsageCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Strong ID declarations must omit positional record syntax because the generator owns the validating constructor.",
        customTags: []);

    public static readonly DiagnosticDescriptor StrongIdMustImplementMatchingInterface = new(
        id: StrongIdMustImplementMatchingInterfaceId,
        title: "Strong ID declarations must implement IStrongId<TValue>",
        messageFormat: "Strong ID type '{0}' must implement IStrongId<{1}>",
        category: UsageCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Strong ID declarations must implement IStrongId<TValue> with the same TValue used by the StrongId attribute.",
        customTags: []);

    public static readonly DiagnosticDescriptor StrongIdMustNotDeclareValueMember = new(
        id: StrongIdMustNotDeclareValueMemberId,
        title: "Strong ID declarations must not declare their own Value member",
        messageFormat: "Strong ID type '{0}' must not declare its own Value member",
        category: UsageCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The Strong ID generator owns the Value member and will not generate code when a conflicting Value member exists.",
        customTags: []);

    public static readonly DiagnosticDescriptor StrongIdMustNotDeclareInstanceConstructors = new(
        id: StrongIdMustNotDeclareInstanceConstructorsId,
        title: "Strong ID declarations must not declare instance constructors",
        messageFormat: "Strong ID type '{0}' must not declare instance constructors",
        category: UsageCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The Strong ID generator owns all instance constructors so it can enforce consistent validation.",
        customTags: []);

    public static readonly DiagnosticDescriptor StrongIdBackingTypeUnsupported = new(
        id: StrongIdBackingTypeUnsupportedId,
        title: "Unsupported strong ID backing type",
        messageFormat: "Strong ID type '{0}' uses unsupported backing type '{1}'; supported backing types are Guid, Int32, Int64, and String",
        category: UsageCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The Strong ID generator currently supports Guid, Int32, Int64, and String backing types.",
        customTags: []);

    /// <summary>
    /// A single method body dispatches via ISender or IPublisher more than once.
    /// Each dispatch creates its own DI scope, so multiple calls in one method likely
    /// indicates that a cross-cutting concern (transaction, unit-of-work) should be
    /// pushed into a pipeline behavior instead.
    /// </summary>
    public static readonly DiagnosticDescriptor MultipleDispatchCallsInMethod = new(
        id: MultipleDispatchCallsInMethodId,
        title: "Multiple mediator dispatch calls in a single method",
        messageFormat: "Method '{0}' calls the mediator {1} times. Each call creates an independent DI scope; consider consolidating into a single command or using a pipeline behavior for cross-cutting coordination.",
        category: UsageCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Each call to ISender.ExecuteAsync, ISender.QueryAsync, or IPublisher.PublishAsync creates its own DI scope. " +
                     "Multiple calls in one method body may indicate that related operations should be composed into a single command/query, " +
                     "or that a shared concern such as a transaction should be expressed as a pipeline behavior.",
        customTags: []);

    // ── AIEL001 – UseAielEventIds ────────────────────────────────────────
    /// <summary>
    /// Fired when a <c>[LoggerMessage]</c> attribute's <c>EventId</c> argument
    /// is a raw integer literal rather than a cast of an <c>AielEventIds</c>
    /// member (e.g. <c>(int)AielEventIds.ModuleStart</c>).
    /// </summary>
    public static readonly DiagnosticDescriptor UseAielEventIds = new(
        id: UseAielEventIdsId,
        title: "Use AielEventIds enum for event IDs",
        messageFormat: "EventId '{0}' is a raw integer. Use '(int)AielEventIds.{1}' instead.",
        category: LoggingCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "All LoggerMessage event IDs must reference AielEventIds enum members so that IDs remain consistent across the framework.",
        helpLinkUri: HelpBase + "AIEL001");

    // ── AIEL002 – MissingEventIdParameter ───────────────────────────────
    /// <summary>
    /// Fired when an Aiel logging helper method does not include an optional
    /// <c>AielEventIds eventId = AielEventIds.X</c> parameter.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingEventIdParameter = new(
        id: MissingEventIdParameterId,
        title: "Logging helper missing AielEventIds parameter",
        messageFormat: "Method '{0}' is a logging helper but is missing an optional 'AielEventIds eventId = AielEventIds.{1}' parameter",
        category: LoggingCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Every Aiel logging helper method must accept an optional AielEventIds parameter so callers can override the default event ID at call sites.",
        helpLinkUri: HelpBase + "AIEL002");

    // ── AIEL003 – MissingEventIdInMessage ───────────────────────────────
    /// <summary>
    /// Fired when a <c>[LoggerMessage]</c> message template does not contain
    /// the <c>[{EventId}]</c> placeholder.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingEventIdInMessage = new(
        id: MissingEventIdInMessageId,
        title: "Log message template missing [{EventId}] placeholder",
        messageFormat: "Message template '{0}' does not contain the '[{{EventId}}]' placeholder",
        category: LoggingCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Aiel log message templates must include '[{EventId}]' so structured log consumers can filter and correlate events.",
        helpLinkUri: HelpBase + "AIEL003");

    // ── AIEL004 – NoDirectILoggerCalls ──────────────────────────────────
    /// <summary>
    /// Fired when production code calls <c>ILogger</c> extension methods
    /// (e.g. <c>LogInformation</c>) directly instead of going through an
    /// Aiel logging helper decorated with <c>[LoggerMessage]</c>.
    /// </summary>
    public static readonly DiagnosticDescriptor NoDirectILoggerCalls = new(
        id: NoDirectILoggerCallsId,
        title: "Do not call ILogger methods directly",
        messageFormat: "Direct call to ILogger.{0}() bypasses Aiel logging conventions. Use a [LoggerMessage]-decorated helper instead.",
        category: LoggingCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Calling ILogger extension methods directly prevents structured event-ID tracking and consistent message formatting.",
        helpLinkUri: HelpBase + "AIEL004");

    // ── AIEL005 – EventIdMismatch ────────────────────────────────────────
    /// <summary>
    /// Fired when the numeric EventId in a <c>[LoggerMessage]</c> attribute
    /// does not match the default value of the method's optional
    /// <c>AielEventIds eventId</c> parameter.
    /// </summary>
    public static readonly DiagnosticDescriptor EventIdMismatch = new(
        id: EventIdMismatchId,
        title: "EventId mismatch between attribute and default parameter",
        messageFormat: "The [LoggerMessage] EventId ({0}) does not match the default value of parameter 'eventId' ({1}). They must agree.",
        category: LoggingCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The numeric EventId declared in [LoggerMessage] must match the AielEventIds member used as the default for the 'eventId' parameter.",
        helpLinkUri: HelpBase + "AIEL005");
}
