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
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aiel.Dependencies;

/// <summary>
/// Default implementation of <see cref="IDependencyManager"/> that builds a dependency graph
/// from a set of <see cref="DependencyDescriptor"/> instances and orchestrates configuration
/// and initialization in dependency order.
/// </summary>
public sealed class DependencyManager : IDependencyManager
{
    private readonly IReadOnlyList<DependencyNode> _orderedNodes;

    /// <summary>
    /// Initializes a new instance of the <see cref="DependencyManager"/> class.
    /// </summary>
    /// <param name="dependencyDescriptors">The descriptors that define the dependencies managed by this instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="dependencyDescriptors"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when duplicate or unknown dependency types are detected.</exception>
    /// <exception cref="CircularDependencyException">Thrown when a circular dependency is detected.</exception>
    public DependencyManager(IEnumerable<DependencyDescriptor> dependencyDescriptors)
    {
        ArgumentNullException.ThrowIfNull(dependencyDescriptors);
        var descriptors = dependencyDescriptors.ToArray();
        if (descriptors.Length == 0)
        {
            Dependencies = [];
            _orderedNodes = [];
            return;
        }

        var nodesByType = new Dictionary<Type, DependencyNode>();

        foreach (var descriptor in descriptors)
        {
            ArgumentNullException.ThrowIfNull(descriptor);

            if (nodesByType.ContainsKey(descriptor.DependencyType))
            {
                throw new InvalidOperationException($"Duplicate dependency type detected: {descriptor.DependencyType.FullName}.");
            }

            nodesByType[descriptor.DependencyType] = new DependencyNode(descriptor);
        }

        foreach (var node in nodesByType.Values)
        {
            foreach (var dependencyType in node.Descriptor.Dependencies)
            {
                if (!nodesByType.TryGetValue(dependencyType, out var dependencyNode))
                {
                    throw new InvalidOperationException($"Dependency '{node.Descriptor.DependencyType.FullName}' depends on unknown dependency type '{dependencyType.FullName}'.");
                }

                if (!node.Dependencies.Contains(dependencyNode))
                {
                    node.Dependencies.Add(dependencyNode);
                }
            }
        }

        var visited = new HashSet<DependencyNode>();
        var visiting = new HashSet<DependencyNode>();
        var ordered = new List<DependencyNode>();

        foreach (var node in nodesByType.Values)
        {
            Visit(node, visited, visiting, ordered, []);
        }

        Dependencies = descriptors;
        _orderedNodes = ordered;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<DependencyDescriptor> Dependencies { get; }

    /// <inheritdoc />
    public async Task ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);

        // Phase 1: pre-configure every module in topological order before any configure phase begins.
        foreach (var node in _orderedNodes)
        {
            foreach (var configuratorType in node.Descriptor.Configurators)
            {
                var instance = CreateInstance(configuratorType) as IDependencyConfigurator
                    ?? throw new InvalidOperationException($"Type '{configuratorType.FullName}' must implement '{nameof(IDependencyConfigurator)}'.");

                try
                {
                    await instance.PreConfigureAsync(context, cancellationToken);
                }
                finally
                {
                    await DisposeIfNeededAsync(instance);
                }
            }
        }

        // Phase 2: configure every module in topological order.
        foreach (var node in _orderedNodes)
        {
            foreach (var configuratorType in node.Descriptor.Configurators)
            {
                var instance = CreateInstance(configuratorType) as IDependencyConfigurator
                    ?? throw new InvalidOperationException($"Type '{configuratorType.FullName}' must implement '{nameof(IDependencyConfigurator)}'.");

                try
                {
                    await instance.ConfigureAsync(context, cancellationToken);
                }
                finally
                {
                    await DisposeIfNeededAsync(instance);
                }
            }
        }
    }

    /// <inheritdoc />
    public async Task InitializeAsync(DependencyInitializationContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        foreach (var node in _orderedNodes)
        {
            foreach (var initializerType in node.Descriptor.Initializers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var instance = CreateInstance(initializerType) as IDependencyInitializer
                    ?? throw new InvalidOperationException($"Type '{initializerType.FullName}' must implement '{nameof(IDependencyInitializer)}'.");

                try
                {
                    await instance.InitializeAsync(context, cancellationToken);
                }
                finally
                {
                    await DisposeIfNeededAsync(instance);
                }
            }
        }
    }

    private static Object CreateInstance(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        var instance = Activator.CreateInstance(type)
            ?? throw new InvalidOperationException($"Unable to create an instance of type '{type.FullName}'. Ensure the type has a public parameterless constructor.");

        return instance;
    }

    private static async Task DisposeIfNeededAsync(Object instance)
    {
        if (instance is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else if (instance is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private static void Visit(
        DependencyNode node,
        ISet<DependencyNode> visited,
        ISet<DependencyNode> visiting,
        ICollection<DependencyNode> ordered,
        IList<Type> path)
    {
        if (visited.Contains(node))
        {
            return;
        }

        if (visiting.Contains(node))
        {
            var cyclePath = new List<Type>(path) { node.Descriptor.DependencyType };
            var cycle = String.Join(" -> ", cyclePath.Select(type => type.Name));
            throw new CircularDependencyException($"Circular dependency detected: {cycle}.");
        }

        visiting.Add(node);
        path.Add(node.Descriptor.DependencyType);

        foreach (var dependency in node.Dependencies)
        {
            Visit(dependency, visited, visiting, ordered, path);
        }

        visiting.Remove(node);
        path.RemoveAt(path.Count - 1);

        visited.Add(node);
        ordered.Add(node);
    }

    private sealed class DependencyNode(DependencyDescriptor descriptor)
    {
        public DependencyDescriptor Descriptor { get; } = descriptor ?? throw new ArgumentNullException(nameof(descriptor));

        public List<DependencyNode> Dependencies { get; } = [];
    }

    public static async Task ConfigureDependenciesAsync(
        IEnumerable<DependencyDescriptor> dependencyDescriptors,
        IServiceCollection services, IConfiguration configuration, String environmentName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(dependencyDescriptors);
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        if (String.IsNullOrWhiteSpace(environmentName))
        {
            environmentName = "Production";
        }

        var environment = services.GetInstance<AielEnvironment>();
        if (environment is null)
        {
            var d = dependencyDescriptors.FirstOrDefault(d => typeof(AielApplication).IsAssignableFrom(d.DependencyType))
                ?? throw new InvalidOperationException($"No derivative of {nameof(AielApplication)} found. Did you forget to call 'services.AddApplication()'?");

            // We have to create an instance of the application to get the application name and version. This is a bit unfortunate,
            // but it only happens once at startup, so it shouldn't be a big deal.
            var app = Activator.CreateInstance(d.DependencyType) as AielApplication
                ?? throw new InvalidOperationException($"Failed to create an instance of {d.DependencyType.FullName}. It MUST have a parameterless constructor.");

            environment = new AielEnvironment(app.ApplicationName, app.ApplicationVersion, environmentName, Guid.NewGuid());

            await app.SafelyDisposeAsync();

            services.TryAddSingleton(environment);
        }

        var context = new DependencyConfigurationContext(environment, services, configuration);

        var manager = new DependencyManager(dependencyDescriptors);

        services.AddSingleton<IDependencyManager>(manager);

        await manager.ConfigureAsync(context, cancellationToken);
    }
}
