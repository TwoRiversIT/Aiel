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

namespace Aiel.Framework.DependencyInjection;

/// <summary>
/// Default implementation of <see cref="IDependencyManager"/> that builds a dependency graph
/// from a set of <see cref="DependencyNode"/> instances and orchestrates configuration
/// and initialization in dependency order.
/// </summary>
public abstract class DependencyManager : IDependencyManager
{
    private readonly Dictionary<Type, DependencyNode> _nodesByType = [];
    private List<DependencyNode> _reversed = [];
    private List<DependencyNode> _descriptors = [];

    /// <inheritdoc />
    public IReadOnlyCollection<DependencyNode> Dependencies => _descriptors.ToArray();

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

    /// <summary>
    /// Initializes a new initializer of the <see cref="DependencyManager"/> class.
    /// </summary>
    /// <param name="dependencyDescriptors">The descriptors that define the dependencies managed by this initializer.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dependencyDescriptors"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when duplicate or unknown dependency types are detected.</exception>
    /// <exception cref="CircularDependencyException">Thrown when a circular dependency is detected.</exception>
    public void Initialize(IEnumerable<DependencyNode> dependencyDescriptors)
    {
        ArgumentNullException.ThrowIfNull(dependencyDescriptors);

        if (!dependencyDescriptors.Any())
        {
            throw new ArgumentException("At least one dependency descriptor must be provided.", nameof(dependencyDescriptors));
        }

        _descriptors = dependencyDescriptors.ToList();

        foreach (var descriptor in _descriptors)
        {
            _nodesByType[descriptor.Type] = descriptor;
            foreach (var node in _nodesByType[descriptor.Type].Dependencies)
            {
                if (!_nodesByType.ContainsKey(node.Type))
                {
                    _nodesByType[node.Type] = node;
                }
            }
        }

        foreach (var node in _nodesByType.Values)
        {
            foreach (var dependency in node.Dependencies)
            {
                if (!_nodesByType.TryGetValue(dependency.Type, out var _))
                {
                    throw new InvalidOperationException($"Dependency '{node.Type.FullName}' depends on unknown dependency type '{dependency.Type.FullName}'.");
                }
            }
        }

        var root = _descriptors[0];

        var visited = new HashSet<DependencyNode>();
        var visiting = new HashSet<DependencyNode>();
        var ordered = new List<DependencyNode>();
        var path = new List<Type>();

        Visit(root, visited, visiting, ordered, path, 0);

        _reversed = ordered.ToList();
    }

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

    protected abstract Task InitializeAsync(InitializationContext context, DependencyNode descriptor, CancellationToken cancellationToken);

    private void Visit(
        DependencyNode descriptor,
        HashSet<DependencyNode> visited,
        HashSet<DependencyNode> visiting,
        List<DependencyNode> ordered,
        List<Type> path,
        Int32 depth)
    {
        if (visited.Contains(descriptor))
        {
            return;
        }

        if (visiting.Contains(descriptor))
        {
            var cyclePath = new List<Type>(path) { descriptor.Type };
            var cycle = String.Join(" -> ", cyclePath.Select(type => type.Name));
            throw new CircularDependencyException($"Circular dependency detected: {cycle}.");
        }

        visiting.Add(descriptor);
        path.Add(descriptor.Type);

        foreach (var dependency in descriptor.Dependencies)
        {
            if (_nodesByType.TryGetValue(dependency.Type, out var dependencyDescriptor))
            {
                Visit(dependencyDescriptor, visited, visiting, ordered, path, depth + 1);
            }
            else
            {
                throw new InvalidOperationException($"Dependency '{descriptor.Type.FullName}' depends on unknown dependency type '{dependency.Type.FullName}'.");
            }
        }

        visiting.Remove(descriptor);
        path.RemoveAt(path.Count - 1);

        visited.Add(descriptor);
        ordered.Add(descriptor);
    }
}

//public class DependencyManager<TApplication> : DependencyManager
//    where TApplication : class, IApplicationConfigurator, new()
//{
//    public DependencyManager()
//    {
//        Initialize(GetAllDependencies<TApplication>());
//    }

//    protected override async Task InitializeAsync(InitializationContext context, DependencyNode descriptor, CancellationToken cancellationToken)
//    {
//        ArgumentNullException.ThrowIfNull(context);
//        ArgumentNullException.ThrowIfNull(descriptor);

//        cancellationToken.ThrowIfCancellationRequested();

//        if (descriptor.Instance is IInitializer initializer)
//        {
//            await initializer.InitializeAsync(context, cancellationToken);
//        }
//    }
//}
