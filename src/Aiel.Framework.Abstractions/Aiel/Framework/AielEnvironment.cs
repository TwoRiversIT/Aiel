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

namespace Aiel.Framework;

/// <summary>
/// Defines the interface for the Aiel environment, which provides metadata about the host environment and application.
/// </summary>
public interface IAielEnvironment
{
    /// <summary>
    /// Gets the name of the current environment (e.g., Development, Production, Staging).
    /// </summary>
    String EnvironmentName { get; }

    /// <summary>
    /// Gets the name of the application.
    /// </summary>
    String ApplicationName { get; }

    /// <summary>
    /// Gets the version of the application.
    /// </summary>
    String ApplicationVersion { get; }

    /// <summary>
    /// Gets the unique identifier for the application instance.
    /// </summary>
    Guid ApplicationInstance { get; }
}

/// <summary>
/// Initializes a new instance of the <see cref="AielEnvironment"/> class which encapsulates
/// hostEnvironment and application metadata for the Aiel dependency injection framework.
/// It is used for logging, diagnostics, and correlation of application instances.
/// </summary>
/// <remarks>
/// <see cref="AielEnvironment"/> is a fundamental part of the application context and MUST
/// be registered as a singleton in the dependency injection container. This will happen
/// automatically when using the <c>AddApplicationAsync</c> extension method.
/// </remarks>
public class AielEnvironment : IAielEnvironment
{
    /// <inheritdoc/>
    public required String ApplicationVersion { get; init; }
    /// <inheritdoc/>
    public required Guid ApplicationInstance { get; init; }
    /// <inheritdoc/>
    public required String EnvironmentName { get; init; }
    /// <inheritdoc/>
    public required String ApplicationName { get; init; }
}

/// <summary>
/// Provides extension methods for the <see cref="IAielEnvironment"/> interface to facilitate environment checks.
/// </summary>
public static class AielEnvironmentExtensions
{
    private const String Production = "Production";
    private const String Development = "Development";
    private const String Staging = "Staging";
    private const String Testing = "Testing";

    /// <summary>
    /// Determines whether the current environment matches the specified environment name.
    /// </summary>
    /// <param name="environment">The Aiel environment.</param>
    /// <param name="environmentName">The name of the environment to check.</param>
    /// <returns><c>true</c> if the current environment matches the specified name; otherwise, <c>false</c>.</returns>
    public static Boolean IsEnvironment(this IAielEnvironment environment, String environmentName)
    {
        ArgumentNullException.ThrowIfNull(environment);

        var name = String.IsNullOrWhiteSpace(environmentName) ? Production : environmentName;

        return String.Equals(environment.EnvironmentName, name, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Determines whether the current environment is Development.
    /// </summary>
    /// <param name="environment">The Aiel environment.</param>
    /// <returns><c>true</c> if the current environment is Development; otherwise, <c>false</c>.</returns>
    public static Boolean IsDevelopment(this IAielEnvironment environment)
        => IsEnvironment(environment, Development);

    /// <summary>
    /// Determines whether the current environment is Production.
    /// </summary>
    /// <param name="environment">The Aiel environment.</param>
    /// <returns><c>true</c> if the current environment is Production; otherwise, <c>false</c>.</returns>
    public static Boolean IsProduction(this IAielEnvironment environment)
        => IsEnvironment(environment, Production);

    /// <summary>
    /// Determines whether the current environment is Staging.
    /// </summary>
    /// <param name="environment">The Aiel environment.</param>
    /// <returns><c>true</c> if the current environment is Staging; otherwise, <c>false</c>.</returns>
    public static Boolean IsStaging(this IAielEnvironment environment)
        => IsEnvironment(environment, Staging);

    /// <summary>
    /// Determines whether the current environment is Testing.
    /// </summary>
    /// <param name="environment">The Aiel environment.</param>
    /// <returns><c>true</c> if the current environment is Testing; otherwise, <c>false</c>.</returns>
    public static Boolean IsTesting(this IAielEnvironment environment)
        => IsEnvironment(environment, Testing);
}
