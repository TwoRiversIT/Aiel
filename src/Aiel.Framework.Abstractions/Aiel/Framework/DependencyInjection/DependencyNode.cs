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

//using System.Collections.ObjectModel;

namespace Aiel.Framework.DependencyInjection;

/// <summary>
/// Represents a node in the dependency graph, containing information about the
/// dependency type, its depth in the graph, the configurator instance, and its
/// child dependencies.
/// </summary>
public class DependencyNode : IAsyncDisposable
{
    /// <summary>
    /// Gets the type of the dependency represented by this node.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Gets the depth of the dependency in the dependency graph.
    /// </summary>
    public Int32 Depth { get; }

    /// <summary>
    /// Gets the configurator instance associated with this dependency node.
    /// </summary>
    public IConfigurator Instance { get; }

    /// <summary>
    /// Gets the collection of child dependencies for this node.
    /// </summary>
    public System.Collections.ObjectModel.Collection<DependencyNode> Dependencies { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DependencyNode"/> class.
    /// </summary>
    /// <param name="type">The type of the dependency.</param>
    /// <param name="depth">The depth of the dependency in the graph.</param>
    /// <param name="instance">The configurator instance for the dependency.</param>
    /// <param name="dependencies">The child dependencies of this node.</param>
    public DependencyNode(Type type, Int32 depth, IConfigurator instance, System.Collections.ObjectModel.Collection<DependencyNode> dependencies)
    {
        ArgumentNullException.ThrowIfNull(dependencies);
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Depth = depth;
        Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        Dependencies = dependencies ?? [];
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="DependencyNode"/>.
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override Boolean Equals(Object? obj)
    {
        if (obj is not DependencyNode other)
        {
            return false;
        }

        return Type == other.Type;
    }

    /// <summary>
    /// Returns a hash code for the current <see cref="DependencyNode"/>.
    /// </summary>
    /// <returns></returns>
    public override Int32 GetHashCode()
    {
        return Type.GetHashCode();
    }

    /// <summary>
    /// Returns a string representation of the current <see cref="DependencyNode"/>, including its type and depth.
    /// </summary>
    /// <returns></returns>
    public override String ToString() => $"{Type.Name} (Depth: {Depth})";

    /// <summary>
    /// Asynchronously disposes of the resources used by the <see cref="DependencyNode"/> instance.
    /// </summary>
    /// <returns>A task that represents the asynchronous dispose operation.</returns>
    public async ValueTask DisposeAsync()
    {
        await Instance.SafelyDisposeAsync();
        GC.SuppressFinalize(this);
    }
}
