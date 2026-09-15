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

using System.Reflection;

namespace Aiel.Framework.DependencyInjection;

public static class DependencyDiscoveryExtensions
{
    /// <summary>
	/// This method builds a hierarchy of assemblies based on the DependsOn attributes.
    /// </summary>
    /// <remarks>
	/// The root of the hierarchy is the assembly specified by <typeparamref name="TDependency"/>,
	/// and the children are the assemblies it depends on, and so on. If an assembly is depended on by
	/// multiple assemblies, it will only appear once in the hierarchy. First one to depend on it wins.
    /// </remarks>
	/// <exception cref="CircularDependencyException">Thrown when a circular attribute is detected in the assembly attribute hierarchy.</exception>
    public static DependencyRoot BuildDependencyTree<TDependency>()
        where TDependency : class, IConfigurator, new()
    {
        // Tracks the assemblies we've already processed by Type.
        var processed = new HashSet<Type>();
        var nodesByType = new Dictionary<Type, DependencyNode>();
        var stack = new Stack<Plate>();

        var dependencyRoot = new DependencyRoot(typeof(TDependency), new TDependency());
        nodesByType[dependencyRoot.Type] = dependencyRoot;
        var initialPath = new HashSet<Type> { typeof(TDependency) };
        stack.Push(new Plate(dependencyRoot, initialPath));

        // We walk the assembly hierarchy depth-first, using the DependsOn attributes to determine the dependencies.
        // We track both visited assemblies (to avoid reprocessing) and the current path (to detect circular dependencies).
        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (!processed.Add(current.Node.Type))
            {
                continue;
            }

            var attributes = current.Node.Type.GetCustomAttributes<DependsOnAttribute>(inherit: false);
            foreach (var attribute in attributes)
            {
                var type = attribute.Type;

                // Check for circular attribute: if the attribute is already in our current path, we have a cycle
                if (current.Path.Contains(type))
                {
                    var pathList = current.Path.ToList();
                    pathList.Add(type);
                    var cycle = String.Join(" -> ", pathList.Select(t => t.Name));
                    throw new CircularDependencyException($"Circular attribute detected: {cycle}");
                }

                // We are strict about the assembly types, so we throw an exception if the attribute rootType does not inherit from AielDependency.
                // This ensures that the attribute hierarchy is well-formed and that we can safely configure the assemblies later.
                if (!nodesByType.TryGetValue(type, out var dependency))
                {
                    var instance = Activator.CreateInstance(type) as AielDependency
                        ?? throw new InvalidOperationException($"Type {type.FullName} does not inherit from AielDependency.");

                    nodesByType[type] = new DependencyNode(type, current.Node.Depth + 1, instance, []);
                }

                if (!current.Node.Dependencies.Contains(nodesByType[type]))
                {
                    current.Node.Dependencies.Add(nodesByType[type]);
                }

                // Create new path for this attribute by copying current path and adding the attribute
                var newPath = new HashSet<Type>(current.Path) { type };
                stack.Push(new Plate(nodesByType[type], newPath));
            }
        }

        return dependencyRoot;
    }

    private record Plate(DependencyNode Node, HashSet<Type> Path);

    /// <summary>
    /// Asynchronously configures the specified root assembly and all its dependencies within the attribute hierarchy.
    /// </summary>
    /// <param name="compositionRoot">The root assembly to configure.</param>
    /// <param name="context">The configuration context that provides settings and services required for assembly configuration.</param>
    /// <returns>A task that represents the asynchronous operation of configuring the assemblies.</returns>
    /// <remarks>
    /// Dependencies at a greater depth are configured before assemblies at a lesser depth, so dependencies are configured
    /// before the assemblies that depend on them. Within a given depth, configuration order is not guaranteed because
    /// <see cref="System.Reflection.MemberInfo.GetCustomAttributes(Boolean)"/> does not guarantee attribute ordering.
    /// </remarks>
    public static async Task ConfigureDependenciesAsync(this DependencyRoot compositionRoot, ConfigurationContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(compositionRoot);
        ArgumentNullException.ThrowIfNull(context);

        var orderedNodes = compositionRoot.GetOrderedDependencies();

        // Phase 1: pre-configure every module before any configure phase begins.
        foreach (var node in orderedNodes)
        {
            await node.Instance.PreConfigureAsync(context, cancellationToken);
        }

        // Phase 2: configure every module.
        foreach (var node in orderedNodes)
        {
            await node.Instance.ConfigureAsync(context, cancellationToken);
        }
    }

    public static IReadOnlyCollection<DependencyNode> GetOrderedDependencies(this DependencyRoot compositionRoot)
    {
        ArgumentNullException.ThrowIfNull(compositionRoot);

        var orderedNodes = new List<DependencyNode>();

        var visited = new HashSet<Type>();

        void Visit(DependencyNode node)
        {
            if (visited.Contains(node.Type))
            {
                return;
            }

            visited.Add(node.Type);

            foreach (var dependency in node.Dependencies)
            {
                Visit(dependency);
            }

            orderedNodes.Add(node);
        }

        Visit(compositionRoot);

        return orderedNodes;
    }
}
