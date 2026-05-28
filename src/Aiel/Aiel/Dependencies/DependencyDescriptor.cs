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
/// Describes a logical application dependency and the startup contributors that participate in
/// configuring and initializing that dependency.
/// </summary>
public sealed class DependencyDescriptor
{
    /// <summary>
	/// Initializes a new instance of the <see cref="DependencyDescriptor"/> class.
    /// </summary>
	/// <param name="name">The logical name of the dependency.</param>
	/// <param name="dependencyType">The <see cref="Type"/> that represents the dependency.</param>
	/// <param name="dependencies">The dependency types this dependency depends on.</param>
	/// <param name="configurators">Types that implement <see cref="IDependencyConfigurator"/> for this dependency.</param>
	/// <param name="initializers">Types that implement <see cref="IDependencyInitializer"/> for this dependency.</param>
    /// <param name="displayName">An optional user friendly display name for the AielDependency.</param>
    /// <param name="version">An optional version for the AielDependency.</param>
	public DependencyDescriptor(
        String name,
        Type dependencyType,
        IEnumerable<Type> dependencies,
        IEnumerable<Type> configurators,
        IEnumerable<Type> initializers,
        String? displayName = null,
        Version? version = null)
    {
        if (String.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Dependency name must not be null or whitespace.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(dependencyType);
        ArgumentNullException.ThrowIfNull(dependencies);
        ArgumentNullException.ThrowIfNull(configurators);
        ArgumentNullException.ThrowIfNull(initializers);

        Name = name;
        DependencyType = dependencyType;
        Dependencies = dependencies.ToArray();
        Configurators = configurators.ToArray();
        Initializers = initializers.ToArray();
        DisplayName = displayName;
        Version = version;
    }

    /// <summary>
    /// Gets the logical name of the dependency.
    /// </summary>
    public String Name { get; }

    /// <summary>
    /// Gets the <see cref="Type"/> that represents the dependency.
    /// </summary>
    public Type DependencyType { get; }

    /// <summary>
    /// Gets the collection of dependency types that this dependency depends on.
    /// </summary>
    public IReadOnlyCollection<Type> Dependencies { get; }

    /// <summary>
    /// Gets the collection of types that implement <see cref="IDependencyConfigurator"/> for this dependency.
    /// </summary>
    public IReadOnlyCollection<Type> Configurators { get; }

    /// <summary>
	/// Gets the collection of types that implement <see cref="IDependencyInitializer"/> for this dependency.
	/// </summary>
    public IReadOnlyCollection<Type> Initializers { get; }

    /// <summary>
    /// Gets the optional user friendly display name for the AielDependency.
    /// </summary>
    public String? DisplayName { get; }

    /// <summary>
    /// Gets the optional version for the AielDependency.
    /// </summary>
    public Version? Version { get; }
}
