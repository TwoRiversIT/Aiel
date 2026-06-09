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

namespace Aiel.Results.Internal;

public static class DiagnosticDescriptors
{
    /// <summary>
    /// AIEL00002 is raised when the generator detects an error type that does not have exactly one public constructor accepting a single string parameter for the error message.
    /// </summary>
    public static readonly DiagnosticDescriptor DerivedErrorTypesMustHaveSingleStringConstructor = new(
        id: DiagnosticRuleIDs.AIEL00002_ErrorTypesMustHaveSingleStringConstructorId,
        title: "Derived error types must have a single string constructor",
        messageFormat: "Error type '{0}' must have a single string constructor accepting the error message",
        category: DiagnosticMetadata.UsageCategory,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "All error types derived from Error must have exactly one public constructor that accepts a single string parameter for the error message.",
        customTags: []
    );

    /// <summary>
    /// AIEL00003 is raised when the generator detects a call to a generic HttpClient JSON extension method
    /// </summary>
    public static readonly DiagnosticDescriptor Prefer_ResultHttpClientExtensions = new(
        id: DiagnosticRuleIDs.AIEL00003_PreferResultHttpClientExtensionsId,
        title: "Use ResultHttpClientExtensions for Result types",
        messageFormat: "Use ResultHttpClientExtensions methods instead of generic HttpClient JSON methods for Result deserialization",
        category: DiagnosticMetadata.UsageCategory,
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "The ResultHttpClientExtensions class provides specialized methods for working with Result and Result<T> types. These methods ensure proper configuration of JSON serialization options. See the README.md documentation for available methods like GetResultAsync<T>, PostAndReturnResultAsync<TRequest, TResponse>, etc.",
        customTags: []
    );
}
