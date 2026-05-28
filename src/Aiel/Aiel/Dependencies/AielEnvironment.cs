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

namespace Aiel.Dependencies;

/// <summary>
/// Initializes a new instance of the <see cref="AielEnvironment"/> class which encapsulates
/// environment and application metadata for the Aiel dependency injection framework.
/// It is used for logging, diagnostics, and correlation of application instances.
/// </summary>
/// <remarks>
/// <see cref="AielEnvironment"/> is a fundamental part of the application context and MUST
/// be registered as a singleton in the dependency injection container. This will happen
/// automatically when using the <c>AddApplicationAsync</c> extension method.
/// </remarks>
/// <param name="applicationName">The name of the application.</param>
/// <param name="applicationVersion">The version of the application.</param>
/// <param name="environmentName">The environment name (e.g., Development, Staging, Production).</param>
/// <param name="applicationInstance">Unique identifier for this application instance. Use
/// <see cref="Guid.NewGuid"/> to generate a new instance ID.</param>
public class AielEnvironment(
    String applicationName,
    String applicationVersion,
    String environmentName,
    Guid applicationInstance)
{

    /// <summary>
    /// Gets the name of the application.
    /// </summary>
    public String ApplicationName { get; } = applicationName ?? String.Empty;

    /// <summary>
    /// Gets the version of the application.
    /// </summary>
    public String ApplicationVersion { get; } = applicationVersion ?? String.Empty;

    /// <summary>
    /// Gets the environment name (e.g., Development, Staging, Production).
    /// </summary>
    public String EnvironmentName { get; } = environmentName ?? String.Empty;

    /// <summary>
    /// Gets the unique identifier for this application instance.
    /// This GUID allows correlation of log entries across distributed systems
    /// and distinguishes between multiple instances of the same application.
    /// </summary>
    public Guid ApplicationInstance { get; } = applicationInstance;
}
