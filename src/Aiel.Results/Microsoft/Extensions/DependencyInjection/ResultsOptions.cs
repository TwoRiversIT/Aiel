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

using System.Text.Json;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Configuration options for the Result Pattern.
/// </summary>
public class ResultsOptions
{
    /// <summary>
    /// Gets the options used to configure JSON serialization and deserialization for this instance.
    /// </summary>
    /// <remarks>Use this property to customize serialization behavior, such as property naming policies,
    /// converters, or formatting. Changes to the options affect how JSON data is processed by this instance.</remarks>
    public JsonSerializerOptions JsonSerializerOptions { get; } = new();

    /// <summary>
    /// Gets or sets the version of the Result Pattern. Default is "v1.0". This can be used to manage evolution
    /// of Custom Errors over time.
    /// </summary>
    public String Version { get; set; } = "v1.0";

    /// <summary>
    /// Enables strict mode for the Custom Error deserialization.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When Strict Mode is enabled, and a <see cref="Result"/> or <see cref="Result{TValue}"/> contains an
    /// <see cref="ErrorCode"/> or <see cref="Error"/> type that is not registered, deserialization will fail with
    /// an exception instead of silently accepting it. This is especially important in distributed systems where:
    /// </para>
    /// <list type="bullet">
    ///   <item>Clients and servers may be on different versions</item>
    ///   <item>Error types evolve over time</item>
    ///   <item>You want to avoid “mystery errors” that deserialize into nonsense</item>
    ///   <item>You want to detect version mismatches early</item>
    /// </list>
    /// </remarks>
    public Boolean StrictMode { get; set; }
}
