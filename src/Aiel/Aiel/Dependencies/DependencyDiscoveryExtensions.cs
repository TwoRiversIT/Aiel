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
	/// <exception cref="CircularDependencyException">Thrown when a circular dependency is detected in the assembly dependency hierarchy.</exception>
	public static DependencyRoot BuildDependencyTree<TDependency>(this DependencyConfigurationContext _)
        where TDependency : AielDependencyConfigurator, new()
    {
        // Tracks the assemblies we've already processed by Type.
        var processed = new HashSet<Type>();
        var nodesByType = new Dictionary<Type, DependencyNode>();
        var stack = new Stack<(DependencyNode Assembly, HashSet<Type> Path)>();

        var rootAssembly = new DependencyRoot(typeof(TDependency), new TDependency());
        nodesByType[rootAssembly.Type] = rootAssembly;
        var initialPath = new HashSet<Type> { typeof(TDependency) };
        stack.Push((rootAssembly, initialPath));

        // We walk the assembly hierarchy depth-first, using the DependsOn attributes to determine the dependencies.
        // We track both visited assemblies (to avoid reprocessing) and the current path (to detect circular dependencies).
        while (stack.Count > 0)
        {
            var (assemblyInfo, path) = stack.Pop();
            if (!processed.Add(assemblyInfo.Type))
            {
                continue;
            }

            var dependencies = assemblyInfo.Type.GetCustomAttributes(typeof(DependsOnAttribute), inherit: false);
            foreach (var dependency in dependencies.Cast<DependsOnAttribute>()) // This cast is safe because we specified the attribute type in GetCustomAttributes.
            {
                var dependencyType = dependency.Type;

                // Check for circular dependency: if the dependency is already in our current path, we have a cycle
                if (path.Contains(dependencyType))
                {
                    var pathList = path.ToList();
                    pathList.Add(dependencyType);
                    var cycle = String.Join(" -> ", pathList.Select(t => t.Name));
                    throw new CircularDependencyException($"Circular dependency detected: {cycle}");
                }

                // We are strict about the assembly types, so we throw an exception if the dependency type does not inherit from AielDependencyConfigurator.
                // This ensures that the dependency hierarchy is well-formed and that we can safely configure the assemblies later.
                if (!nodesByType.TryGetValue(dependencyType, out var dependencyAssemblyInfo))
                {
                    var instance = Activator.CreateInstance(dependencyType) as AielDependencyConfigurator
                        ?? throw new InvalidOperationException($"Type {dependencyType.FullName} does not inherit from AielDependencyConfigurator.");

                    dependencyAssemblyInfo = new DependencyNode(dependencyType, assemblyInfo.Depth + 1, instance);
                    nodesByType[dependencyType] = dependencyAssemblyInfo;
                }

                if (!assemblyInfo.Dependencies.Contains(dependencyAssemblyInfo))
                {
                    assemblyInfo.Dependencies.Add(dependencyAssemblyInfo);
                }

                // Create new path for this dependency by copying current path and adding the dependency
                var newPath = new HashSet<Type>(path) { dependencyType };
                stack.Push((dependencyAssemblyInfo, newPath));
            }
        }

        return rootAssembly;
    }

    /// <summary>
    /// Asynchronously configures the specified root assembly and all its dependencies within the dependency hierarchy.
    /// </summary>
    /// <param name="compositionRoot">The root assembly to configure.</param>
    /// <param name="context">The configuration context that provides settings and services required for assembly configuration.</param>
    /// <returns>A task that represents the asynchronous operation of configuring the assemblies.</returns>
    /// <remarks>
    /// Dependencies at a greater depth are configured before assemblies at a lesser depth, so dependencies are configured
    /// before the assemblies that depend on them. Within a given depth, configuration order is not guaranteed because
    /// <see cref="System.Reflection.MemberInfo.GetCustomAttributes(Boolean)"/> does not guarantee attribute ordering.
    /// </remarks>
    public static async Task ConfigureDependenciesAsync(this DependencyRoot compositionRoot, DependencyConfigurationContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(compositionRoot);
        ArgumentNullException.ThrowIfNull(context);

        // Collect all nodes; traversal order is irrelevant because we sort by depth below.
        // BuildDependencyTree guarantees each type appears exactly once in the tree.
        var allAssemblys = new List<DependencyNode>();
        var visited = new HashSet<DependencyNode>();
        var stack = new Stack<DependencyNode>();
        stack.Push(compositionRoot);

        while (stack.Count > 0)
        {
            var assemblyInfo = stack.Pop();
            if (!visited.Add(assemblyInfo))
            {
                continue;
            }

            allAssemblys.Add(assemblyInfo);

            foreach (var dependency in assemblyInfo.Dependencies)
            {
                stack.Push(dependency);
            }
        }

        // Configure deepest assemblies first so that each assembly's dependencies are already configured
        // by the time the assembly itself runs. Within a depth tier, order is not guaranteed.
        var orderedNodes = allAssemblys.OrderByDescending(static m => m.Depth).ToArray();

        // Phase 1: pre-configure every module before any configure phase begins.
        foreach (var assemblyInfo in orderedNodes)
        {
            await assemblyInfo.Instance.PreConfigureAsync(context, cancellationToken);
        }

        // Phase 2: configure every module.
        foreach (var assemblyInfo in orderedNodes)
        {
            await assemblyInfo.Instance.ConfigureAsync(context, cancellationToken);
        }
    }
}
