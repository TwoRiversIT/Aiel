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
/// Default implementation of <see cref="IDependencyManager"/> that builds a dependency graph
/// from a set of <see cref="DependencyDescriptor"/> instances and orchestrates configuration
/// and initialization in dependency order.
/// </summary>
public abstract class DependencyManager : IDependencyManager
{
    private readonly List<DependencyDescriptor> _descriptors;
    private readonly Dictionary<Type, DependencyDescriptor> _nodesByType = [];
    private readonly List<DependencyDescriptor> _reversed;

    /// <summary>
    /// Initializes a new initializer of the <see cref="DependencyManager"/> class.
    /// </summary>
    /// <param name="dependencyDescriptors">The descriptors that define the dependencies managed by this initializer.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dependencyDescriptors"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when duplicate or unknown dependency types are detected.</exception>
    /// <exception cref="CircularDependencyException">Thrown when a circular dependency is detected.</exception>
    protected DependencyManager(IEnumerable<DependencyDescriptor> dependencyDescriptors)
    {
        ArgumentNullException.ThrowIfNull(dependencyDescriptors);

        if (!dependencyDescriptors.Any())
        {
            throw new ArgumentException("At least one dependency descriptor must be provided.", nameof(dependencyDescriptors));
        }

        _descriptors = dependencyDescriptors.ToList();

        foreach (var descriptor in _descriptors)
        {
            ArgumentNullException.ThrowIfNull(descriptor);

            if (_nodesByType.ContainsKey(descriptor.DependencyType))
            {
                throw new InvalidOperationException($"Duplicate dependency type detected: {descriptor.DependencyType.FullName}.");
            }

            _nodesByType[descriptor.DependencyType] = descriptor;
        }

        foreach (var descriptor in _nodesByType.Values)
        {
            foreach (var dependencyType in descriptor.Dependencies)
            {
                if (!_nodesByType.TryGetValue(dependencyType, out var dependencyNode))
                {
                    throw new InvalidOperationException($"Dependency '{descriptor.DependencyType.FullName}' depends on unknown dependency type '{dependencyType.FullName}'.");
                }
            }
        }

        var root = _descriptors[0];

        var visited = new HashSet<DependencyDescriptor>();
        var visiting = new HashSet<DependencyDescriptor>();
        var ordered = new List<DependencyDescriptor>();
        var path = new List<Type>();

        Visit(root, visited, visiting, ordered, path);

        _reversed = ordered.ToList();
    }

    /// <inheritdoc />
    public IReadOnlyCollection<DependencyDescriptor> Dependencies => _descriptors.ToArray();

    /// <inheritdoc />
    public async ValueTask ConfigureAsync(ConfigurationContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Phase 1: pre-configure every module in topological order before any configure phase begins.
        foreach (var node in _reversed)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await node.Instance.PreConfigureAsync(context, cancellationToken);

            // We do not dispose here because we need the initializer to be alive for the configure phase.
        }

        // Phase 2: configure every module in topological order.
        foreach (var node in _reversed)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await node.Instance.ConfigureAsync(context, cancellationToken);

            // We do not dispose here because we need the initializer to be alive for the initialization phase.
        }
    }

    protected abstract Task InitializeAsync(InitializationContext context, DependencyDescriptor descriptor, CancellationToken cancellationToken);

    /// <inheritdoc />
    public async ValueTask InitializeAsync(InitializationContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var node in _reversed)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await InitializeAsync(context, node, cancellationToken);
            }
            finally
            {
                await node.Instance.SafelyDisposeAsync();
            }
        }
    }

    private void Visit(
        DependencyDescriptor descriptor,
        HashSet<DependencyDescriptor> visited,
        HashSet<DependencyDescriptor> visiting,
        List<DependencyDescriptor> ordered,
        List<Type> path)
    {
        if (visited.Contains(descriptor))
        {
            return;
        }

        if (visiting.Contains(descriptor))
        {
            var cyclePath = new List<Type>(path) { descriptor.DependencyType };
            var cycle = String.Join(" -> ", cyclePath.Select(type => type.Name));
            throw new CircularDependencyException($"Circular dependency detected: {cycle}.");
        }

        visiting.Add(descriptor);
        path.Add(descriptor.DependencyType);

        foreach (var dependency in descriptor.Dependencies)
        {
            if (_nodesByType.TryGetValue(dependency, out var dependencyDescriptor))
            {
                Visit(dependencyDescriptor, visited, visiting, ordered, path);
            }
            else
            {
                throw new InvalidOperationException($"Dependency '{descriptor.DependencyType.FullName}' depends on unknown dependency type '{dependency.FullName}'.");
            }
        }

        visiting.Remove(descriptor);
        path.RemoveAt(path.Count - 1);

        visited.Add(descriptor);
        ordered.Add(descriptor);
    }

    public static IEnumerable<DependencyDescriptor> GetAllDependencies<TApplication>()
        where TApplication : class, IApplicationConfigurator, new()
    {
        var doa = typeof(DependsOnAttribute);
        var graph = new Dictionary<Type, List<Type>>();
        var unresolved = new HashSet<Type>();
        var queue = new Queue<Type>();

        queue.Enqueue(typeof(TApplication));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (graph.ContainsKey(current))
            {
                continue;
            }

            graph[current] = [];

            foreach (var attribute in current.GetCustomAttributes(doa, inherit: false))
            {
                if (attribute is not DependsOnAttribute dependsOn)
                {
                    continue;
                }

                graph[current].Add(dependsOn.Type);
                if (typeof(IConfigurator).IsAssignableFrom(dependsOn.Type))
                {
                    if (!graph.ContainsKey(dependsOn.Type))
                    {
                        queue.Enqueue(dependsOn.Type);
                    }

                    continue;
                }

                unresolved.Add(dependsOn.Type);
            }
        }

        if (unresolved.Count > 0)
        {
            throw new AielException($"The following dependencies are unresolved: {String.Join(", ", unresolved.Select(t => t.FullName))}");
        }

        var list = new List<DependencyDescriptor>();
        foreach (var kvp in graph)
        {
            var instance = Activator.CreateInstance(kvp.Key) as IConfigurator
                ?? throw new InvalidOperationException($"Type {kvp.Key.FullName} does not implement IConfigurator.");

            list.Add(new DependencyDescriptor(kvp.Key.Name, kvp.Key, instance, kvp.Value));
        }

        return list.ToArray();
    }
}

public class DependencyManager<TApplication>() : DependencyManager(GetAllDependencies<TApplication>())
    where TApplication : class, IApplicationConfigurator, new()
{
    protected override async Task InitializeAsync(InitializationContext context, DependencyDescriptor descriptor, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(descriptor);

        cancellationToken.ThrowIfCancellationRequested();

        if (descriptor.Instance is IInitializer initializer)
        {
            await initializer.InitializeAsync(context, cancellationToken);
        }
    }
}
