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

using Aiel.Dependencies;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class AielWebAssemblyHostExtensions
{
    /// <summary>
    /// Resolves the registered dependency graph and calls
    /// <see cref="IDependencyInitializer.InitializeAsync"/> on each dependency that implements it,
    /// in post-order (dependencies before dependents). Prefers <see cref="IDependencyManager"/>
    /// when registered (source-generated graphs).
    /// </summary>
    public static async Task InitializeApplicationAsync(
        this WebAssemblyHost host,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(host);

        var environment = host.Services.GetRequiredService<AielEnvironment>();

        var context = new DependencyInitializationContext(
            environment,
            host.Services.GetRequiredService<IConfiguration>(),
            host.Services.GetRequiredService<ILogger<DependencyInitializationContext>>(),
            host.Services);

        var dependencyManager = host.Services.GetService<IDependencyManager>();
        if (dependencyManager is not null)
        {
            await dependencyManager.InitializeAsync(context, cancellationToken);
            return;
        }

        var root = host.Services.GetRequiredService<DependencyRoot>();

        // Iterative post-order DFS: push each node's subtree so that when popped, every dependency
        // is initialized before the dependency that depends on it.
        var initOrder = new Stack<DependencyNode>();
        var traversal = new Stack<DependencyNode>();
        var visited = new HashSet<DependencyNode>();
        traversal.Push(root);

        while (traversal.Count > 0)
        {
            var node = traversal.Pop();
            if (!visited.Add(node))
            {
                continue;
            }

            initOrder.Push(node);

            foreach (var dependency in node.Dependencies)
            {
                traversal.Push(dependency);
            }
        }

        while (initOrder.Count > 0)
        {
            var node = initOrder.Pop();
            if (node.Instance is IDependencyInitializer initializer)
            {
                await initializer.InitializeAsync(context, cancellationToken);
            }
        }
    }
}
