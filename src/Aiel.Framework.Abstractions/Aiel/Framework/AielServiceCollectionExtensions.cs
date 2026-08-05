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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Aiel.Framework;

public static class AielServiceCollectionExtensions
{
    /// <summary>
    /// Adds strongly-typed options to the service collection, binds them to a configuration section, and registers a
    /// validator to ensure the options are valid at application startup.
    /// </summary>
    /// <remarks>Validation is performed when the application starts. If the options are invalid, application
    /// startup will fail. This method is useful for ensuring configuration errors are detected early.</remarks>
    /// <typeparam name="TOptions">The options class type to bind and validate. Must be a reference type.</typeparam>
    /// <typeparam name="TValidator">The type that implements validation logic for the options. Must implement IValidateOptions<TOptions>.</typeparam>
    /// <param name="services">The service collection to which the options and validator are added.</param>
    /// <param name="configuration">The configuration source from which to bind the options.</param>
    /// <param name="sectionName">The name of the configuration section to bind. If null, the name of the options type is used.</param>
    /// <param name="optionsName">The name of the options instance. If null, the default options instance is used.</param>
    /// <returns>The same IServiceCollection instance, enabling method chaining.</returns>
    public static IServiceCollection AddValidatedOptions<TOptions, TValidator>(
        this IServiceCollection services,
        IConfiguration configuration,
        String? sectionName = null,
        String? optionsName = null,
        Boolean validateOnStart = true)
        where TOptions : class
        where TValidator : class, IValidateOptions<TOptions>
    {
        sectionName ??= typeof(TOptions).Name;

        services.TryAddSingleton<IValidateOptions<TOptions>, TValidator>();

        var builder = validateOnStart
            ? services.AddOptionsWithValidateOnStart<TOptions>(optionsName)
            : services.AddOptions<TOptions>(optionsName);

        builder.Bind(configuration.GetSection(sectionName));

        return services;
    }
}
