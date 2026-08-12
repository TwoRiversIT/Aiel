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

public interface IAielEnvironment
{
    String EnvironmentName { get; }

    String ApplicationName { get; }

    String ApplicationVersion { get; }
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
    public required String ApplicationVersion { get; init; }
    public required Guid ApplicationInstance { get; init; }
    public required String EnvironmentName { get; init; }
    public required String ApplicationName { get; init; }
}

public static class AielEnvironmentExtensions
{
    public static Boolean IsEnvironment(this IAielEnvironment environment, String environmentName)
        => String.Equals(environment.EnvironmentName, environmentName, StringComparison.OrdinalIgnoreCase);

    public static Boolean IsDevelopment(this IAielEnvironment environment)
        => IsEnvironment(environment, "Development");

    public static Boolean IsProduction(this IAielEnvironment environment)
        => IsEnvironment(environment, "Production");

    public static Boolean IsStaging(this IAielEnvironment environment)
        => IsEnvironment(environment, "Staging");

    public static Boolean IsTesting(this IAielEnvironment environment)
        => IsEnvironment(environment, "Testing");
}
