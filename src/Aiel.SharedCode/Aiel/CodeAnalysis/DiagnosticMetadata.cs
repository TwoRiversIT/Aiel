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

namespace Aiel.CodeAnalysis;

/// <summary>
/// Provides metadata for diagnostics used in Aiel analyzers, including common categories and help link base.
/// </summary>
public static class DiagnosticMetadata
{
    /// <summary>
    /// Gets the base URL for help links related to Aiel analyzers.
    /// </summary>
    public const String HelpBaseUrl = "https://docs.aiel.ca/analyzers/";

    /// <summary>
    /// Gets the logging category used for Aiel analyzers.
    /// </summary>
    public const String LoggingCategory = "AielLogging";

    /// <summary>
    /// Gets the category used for diagnostics related to strong identifiers in Aiel analyzers.
    /// </summary>
    public const String StrongIdCategory = "AielStrongId";

    /// <summary>
    /// Gets the category used for diagnostics related to usage in Aiel analyzers.
    /// </summary>
    public const String UsageCategory = "AielUsage";
}
