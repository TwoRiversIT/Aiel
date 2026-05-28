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
using System.Text.Json.Serialization;

namespace Aiel.Results;

/// <summary>
/// Provides a centralized location for configuring JSON serialization options for the result pattern, including custom converters
/// for Error, Result, and Result&lt;T&gt; types.
/// </summary>
public static class Results
{
    private static JsonSerializerOptions? PrivateJSO = ConfigureForResults(new JsonSerializerOptions(JsonSerializerDefaults.Web));

    /// <summary>
    /// Gets the default <see cref="JsonSerializerOptions"/> configured for the result pattern.
    /// </summary>
    /// <remarks>
    /// This instance is pre-configured with converters for <see cref="Result"/>, and <see cref="Result{T}"/> types.
    /// </remarks>
    public static JsonSerializerOptions JSO => PrivateJSO ?? ConfigureForResults(new JsonSerializerOptions());

    /// <summary>
    /// Resets the static JSO instance. For testing purposes only.
    /// </summary>
    /// <remarks>This method should only be used in unit tests to ensure test isolation.</remarks>
    internal static void ResetForTesting()
    {
        PrivateJSO = null;
    }

    /// <summary>
    /// A convenience method that configures the specified JsonSerializerOptions instance to support custom serialization
    /// and deserialization for <see cref="Result"/>, and <see cref="Result{T}"/> types.
    /// </summary>
    /// <remarks>This method adds custom converters to the provided JsonSerializerOptions to enable correct
    /// handling of error and result types during JSON serialization and deserialization. Use this method when working
    /// with APIs or data models that utilize these types to ensure proper JSON processing.</remarks>
    /// <param name="jso">The JsonSerializerOptions instance to configure. Cannot be null.</param>
    public static JsonSerializerOptions ConfigureForResults(this JsonSerializerOptions jso)
    {
        ArgumentNullException.ThrowIfNull(jso);

        if (jso.IsReadOnly)
        {
            throw new InvalidOperationException("The provided JsonSerializerOptions instance is read-only and cannot be configured.");
        }

        // Theoretically, these settings should be set by the caller, but we'll set them here just in case.
        jso.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        jso.PropertyNameCaseInsensitive = true;
        jso.NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString;

        if (!jso.Converters.Any(c => c.GetType() == typeof(ErrorJsonConverterFactory)))
        {
            jso.Converters.Add(new ErrorJsonConverterFactory());
        }

        if (!jso.Converters.Any(c => c.GetType() == typeof(ResultJsonConverter)))
        {
            jso.Converters.Add(new ResultJsonConverter());
        }

        if (!jso.Converters.Any(c => c.GetType() == typeof(ResultOfTJsonConverterFactory)))
        {
            jso.Converters.Add(new ResultOfTJsonConverterFactory());
        }

        return jso;
    }

    /// <summary>
    /// Configures the JSON serializer options used for serialization and deserialization of
    /// <see cref="Result"/>, and <see cref="Result{T}"/> types.
    /// </summary>
    /// <remarks>This method applies the specified configuration to the shared serializer options and ensures
    /// that the options are set up appropriately for result handling.</remarks>
    /// <param name="configureOptions">An optional action that receives the current <see cref="JsonSerializerOptions"/> instance, allowing
    /// customization of serialization behavior.</param>
    public static void ConfigureJsonSerializerOptionsForResults(Action<JsonSerializerOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(configureOptions);
        configureOptions(JSO);
        ConfigureForResults(JSO);
    }
}
