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

using Aiel.Framework;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static partial class AielHostApplicationExtensions
{
    /// <summary>
    /// Resolves the registered dependency graph and calls
    /// <see cref="IInitializer.InitializeAsync"/> on each dependency that implements it,
    /// in post-order (dependencies before dependents). Prefers <see cref="IDependencyManager"/>
    /// when registered (source-generated graphs).
    /// </summary>
    public static async Task InitializeApplicationAsync(this IHost host, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(host);

        var dependencyManager = host.Services.GetRequiredService<IDependencyManager>();

        var context = new HostApplicationInitializationContext(host);

        await dependencyManager.InitializeAsync(context, cancellationToken);
    }

    [LoggerMessage(EventId = 0, Level = LogLevel.Debug, Message = "Initializing Dependency {DependencyType}.")]
    private static partial void LogInitializingDependency(ILogger logger, String dependencyType);
}
