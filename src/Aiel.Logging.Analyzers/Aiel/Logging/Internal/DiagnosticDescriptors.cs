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

namespace Aiel.Logging.Internal;

public static class DiagnosticDescriptors
{
    /// <summary>
    /// AIEL00008 is raised when a <c>[LoggerMessage]</c> attribute's <c>EventId</c> argument
    /// is a raw integer literal rather than a cast of an <c>AielEvent</c>
    /// member (e.g. <c>(int)AielEvent.ModuleStart</c>).
    /// </summary>
    public static readonly DiagnosticDescriptor UseAielEventIds = new(
        id: DiagnosticRuleIDs.AIEL00008_UseAielEventIdsId,
        title: "Use AielEvent enum for event IDs",
        messageFormat: "EventId '{0}' is a raw integer. Use '(int)AielEvent.{1}' instead.",
        category: DiagnosticMetadata.LoggingCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "All LoggerMessage event IDs must reference AielEvent enum members so that IDs remain consistent across the framework.",
        helpLinkUri: DiagnosticMetadata.HelpBase + "AIEL00008");

    /// <summary>
    /// AIEL00009 is raised when an Aiel logging helper method does not include an optional
    /// <c>AielEvent eventId = AielEvent.X</c> parameter.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingEventIdParameter = new(
        id: DiagnosticRuleIDs.AIEL00009_MissingEventIdParameterId,
        title: "Logging helper missing EventId parameter",
        messageFormat: "Method '{0}' is a logging helper but is missing an  'AielEvent eventId = AielEvent.{1}' parameter",
        category: DiagnosticMetadata.LoggingCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Every Aiel logging helper method must have an optional AielEvent parameter so callers can override the default event ID at call sites.",
        helpLinkUri: DiagnosticMetadata.HelpBase + "AIEL00009");

    /// <summary>
    /// AIEL00010 is raised when a <c>[LoggerMessage]</c> message template does not contain
    /// the exact <c>[{EventId}]</c> placeholder.
    /// </summary>
    public static readonly DiagnosticDescriptor MissingTemplateEventIdPlaceholder = new(
        id: DiagnosticRuleIDs.AIEL00010_MissingTemplateEventIdPlaceholder,
        title: "Log message template missing [{EventId}] placeholder",
        messageFormat: "Message template '{0}' does not contain the '[{{EventId}}]' placeholder, or it is not formatted correctly. Note that it must be in square brackets.",
        category: DiagnosticMetadata.LoggingCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Aiel log message templates must include '[{EventId}]' so structured log consumers can filter and correlate events.",
        helpLinkUri: DiagnosticMetadata.HelpBase + "AIEL00010");

    /// <summary>
    /// AIEL00011 is raised when production code calls <c>ILogger</c> extension methods
    /// (e.g. <c>LogInformation</c>) directly instead of going through an
    /// Aiel logging helper decorated with <c>[LoggerMessage]</c>.
    /// </summary>
    public static readonly DiagnosticDescriptor NoDirectILoggerCalls = new(
        id: DiagnosticRuleIDs.AIEL00011_NoDirectILoggerCallsId,
        title: "Do not call ILogger methods directly",
        messageFormat: "Direct call to ILogger.{0}() bypasses Aiel logging conventions. Use a [LoggerMessage]-decorated helper instead.",
        category: DiagnosticMetadata.LoggingCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Calling ILogger extension methods directly prevents structured event-ID tracking and consistent message formatting.",
        helpLinkUri: DiagnosticMetadata.HelpBase + "AIEL00011");

    /// <summary>
    /// AIEL00012 is raised when the numeric EventId in a <c>[LoggerMessage]</c>
    /// attribute does not match the default value of the method's
    /// <c>AielEvent eventId</c> parameter.
    /// </summary>
    public static readonly DiagnosticDescriptor EventIdMismatch = new(
        id: DiagnosticRuleIDs.AIEL00012_EventIdMismatchId,
        title: "EventId mismatch between attribute and default parameter",
        messageFormat: "The [LoggerMessage] EventId ({0}) does not match the default value of parameter 'eventId' ({1}). They must agree.",
        category: DiagnosticMetadata.LoggingCategory,
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The numeric EventId declared in [LoggerMessage] must match the AielEvent member used as the default value for the 'eventId' parameter.",
        helpLinkUri: DiagnosticMetadata.HelpBase + "AIEL00012");
}
