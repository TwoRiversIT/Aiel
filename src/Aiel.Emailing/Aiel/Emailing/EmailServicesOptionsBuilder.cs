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
using System.Reflection;

namespace Aiel.Emailing;

/// <summary>
/// Builder class for configuring email services options in the Aiel framework.
/// This class allows you to specify assemblies containing email templates and
/// configure email-related options.
/// </summary>
/// <param name="services">The service collection to add email services to.</param>
/// <param name="configuration">The configuration to bind email options from.</param>
public class EmailServicesOptionsBuilder(IServiceCollection services, IConfiguration configuration)
{
    private readonly HashSet<Assembly> _assemblyList = [];

    /// <summary>
    /// Gets the service collection to which email services will be added.
    /// </summary>
    public IServiceCollection Services { get; } = services ?? throw new ArgumentNullException(nameof(services));
    /// <summary>
    /// Gets the configuration to bind email options from.
    /// </summary>
    public IConfiguration Configuration { get; } = configuration ?? throw new ArgumentNullException(nameof(configuration));

    /// <summary>
    /// Gets the assemblies containing email templates.
    /// </summary>
    public Assembly[] TemplateAssemblies => [.. _assemblyList];

    /// <summary>
    /// Gets the email options.
    /// </summary>
    public EmailOptions Options { get; } = new EmailOptions();

    /// <summary>
    /// Includes email templates from the specified assemblies.
    /// </summary>
    /// <param name="assemblies">The assemblies containing email templates.</param>
    /// <returns>The current <see cref="EmailServicesOptionsBuilder"/> instance.</returns>
    public EmailServicesOptionsBuilder IncludeTemplatesFrom(params Assembly[] assemblies)
    {
        foreach (var a in assemblies)
        {
            _assemblyList.Add(a);
        }

        return this;
    }

    /// <summary>
    /// Includes email templates from the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="T">The type whose containing assembly will be included.</typeparam>
    /// <returns>The current <see cref="EmailServicesOptionsBuilder"/> instance.</returns>
    public EmailServicesOptionsBuilder IncludeTemplatesFromAssemblyContaining<T>()
    {
        _assemblyList.Add(typeof(T).Assembly);
        return this;
    }
}
