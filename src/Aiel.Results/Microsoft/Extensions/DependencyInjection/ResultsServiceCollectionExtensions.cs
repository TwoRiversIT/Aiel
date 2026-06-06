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

using Aiel.Results;
using System.Text.Json;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering result pattern serialization and configuration services in an ASP.NET
/// Core application's dependency injection container.
/// </summary>
/// <remarks>This class enables integration of result pattern o and JSON serialization settings into the
/// application's service collection. Use the provided extension method to configure result pattern support, including
/// custom JSON type information resolvers for error polymorphism. Thread safety and correct configuration are ensured
/// when used during application startup.</remarks>
public static class ResultsServiceCollectionExtensions
{
    /// <summary>
    /// Adds result pattern services to the specified service collection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method configures the base Results pattern support by creating and registering a singleton
    /// <see cref="JsonSerializerOptions"/> instance with all necessary converters for <see cref="Result"/>,
    /// <see cref="Result{T}"/>, and <see cref="Error"/> types. This instance is used by static JSON serialization calls.
    /// </para>
    /// <para>
    /// For ASP.NET Core applications, use the platform-specific extension methods instead:
    /// - Use <c>AddResultPattern()</c> from Aiel.Results.AspNetCore for server-side apps
    /// - Use <c>AddResultPattern()</c> from Aiel.Results.AspNetCore.Blazor.WebAssembly for Blazor WebAssembly apps
    /// </para>
    /// </remarks>
    /// <param name="services">The service collection to add the result pattern services to.</param>
    /// <param name="configureOptions">An optional action to configure the JSON serializer options before Results converters are added.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddResultPattern(this IServiceCollection services, Action<JsonSerializerOptions>? configureOptions = null)
    {
        // Ensure the NoError type is registered in the ErrorRegistry to prevent issues with JSON deserialization of Result types
        _ = ErrorRegistry.GetErrorType(typeof(NoError).FullName!);

        // Create and configure the singleton JsonSerializerOptions for Results.JSO
        var options = new JsonSerializerOptions();
        configureOptions?.Invoke(options);
        options.ConfigureForResults();

        // Register the configured singleton for dependency injection
        services.AddSingleton(options);

        return services;
    }
}
