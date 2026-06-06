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

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

namespace Aiel.Resources;

/// <summary>
/// Resource helper for loading embedded resources from assemblies.
/// </summary>
[DebuggerStepThrough]
public static class RH
{
    private static readonly ConcurrentDictionary<String, String> Cache = new();

    /// <summary>
    /// Looks in the assembly and namespace that contains <typeparamref name="T"/> for a resource named <paramref name="resource"/> and returns it as a <see cref="String"/>.
    /// </summary>
    /// <typeparam name="T">The type used to locate the assembly and namespace for the resource.</typeparam>
    /// <param name="resource">The case-sensitive name of the manifest resource being requested.</param>
    /// <returns>The manifest resource as a string; or throws if not found.</returns>
    /// <remarks>
    /// Strings returned by this method are cached in memory the first time they are accessed.
    /// Subsequent requests for the same resource are returned directly from the cache.
    /// </remarks>
    [DebuggerStepThrough]
    public static String GetString<T>(String resource)
    {
        var type = typeof(T);
        return GetString(resource, type);
    }

    /// <summary>
    /// Looks in the assembly and namespace that contains <paramref name="type"/> for an embedded
    /// resource named <paramref name="resource"/> and returns it as a <see cref="String"/>, if found.
    /// </summary>
    /// <param name="resource">The case-sensitive name of the manifest resource being requested.</param>
    /// <param name="type">The type used to locate the assembly and namespace for the resource.</param>
    /// <returns>The manifest resource as a string; or throws if not found.</returns>
    /// <remarks>
    /// Strings returned by this method are cached in memory the first time they are accessed.
    /// Subsequent requests for the same resource are returned directly from the cache.
    /// </remarks>
    [DebuggerStepThrough]
    public static String GetString(String resource, Type type)
    {
        var cachekey = type.AssemblyQualifiedName + "::" + resource;
        return Cache.GetOrAdd(cachekey, _ =>
        {
            try
            {
                var assembly = type.GetTypeInfo().Assembly
                ?? throw new Exception("Could not load the assembly.");

                var key = type.Namespace + "." + resource;

                using var stream = assembly.GetManifestResourceStream(key)
                    ?? throw new Exception($"Failed to load the resource '{resource}' from {type.AssemblyQualifiedName}.");

                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load the resource '{resource}' from {type.AssemblyQualifiedName}.", ex);
            }
        });
    }

    /// <summary>
    /// Loads the specified manifest resource, scoped by the namespace of the specified type, from the assembly containing the type as a byte array.
    /// </summary>
    /// <typeparam name="T">The type used to locate the assembly and namespace for the resource.</typeparam>
    /// <param name="resource">The case-sensitive name of the manifest resource being requested.</param>
    /// <returns>A byte array containing the manifest resource; or throws if not found.</returns>
    [DebuggerStepThrough]
    public static Byte[] GetBytes<T>(String resource) => GetBytes(resource, typeof(T));

    /// <summary>
    /// Loads the specified manifest resource, scoped by the namespace of the specified type, from the assembly containing the type as a byte array.
    /// </summary>
    /// <param name="resource">The case-sensitive name of the manifest resource being requested.</param>
    /// <param name="type">The type used to locate the assembly and namespace for the resource.</param>
    /// <returns>A byte array containing the manifest resource; or throws if not found.</returns>
    [DebuggerStepThrough]
    public static Byte[] GetBytes(String resource, Type type)
    {
        using (var stream = GetStream(resource, type))
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }

    /// <summary>
    /// Loads the specified manifest resource, scoped by the namespace of the specified type, from the assembly containing the type as a stream.
    /// </summary>
    /// <typeparam name="T">The type used to locate the assembly and namespace for the resource.</typeparam>
    /// <param name="resource">The case-sensitive name of the manifest resource being requested.</param>
    /// <returns>A stream containing the manifest resource; or throws if not found. The caller is responsible for disposing the stream.</returns>
    [DebuggerStepThrough]
    public static Stream GetStream<T>(String resource)
    {
        var type = typeof(T);
        return GetStream(resource, type);
    }

    /// <summary>
    /// Loads the specified manifest resource, scoped by the namespace of the specified type, from the assembly containing the type as a stream.
    /// </summary>
    /// <param name="resource">The case-sensitive name of the manifest resource being requested.</param>
    /// <param name="type">The type used to locate the assembly and namespace for the resource.</param>
    /// <returns>A stream containing the manifest resource; or throws if not found. The caller is responsible for disposing the stream.</returns>
    [DebuggerStepThrough]
    public static Stream GetStream(String resource, Type type)
    {
        if (String.IsNullOrWhiteSpace(resource))
        {
            throw new ArgumentException($"'{nameof(resource)}' cannot be null or whitespace.", nameof(resource));
        }

        var assembly = type.GetTypeInfo().Assembly
            ?? throw new InvalidOperationException("Could not load the assembly.");

        var key = type.Namespace + "." + resource;
        try
        {
            return assembly.GetManifestResourceStream(key)
                ?? throw new Exception($"Failed to load the resource '{resource}' from {type.AssemblyQualifiedName}.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to load the resource '{resource}' from {type.AssemblyQualifiedName}.", ex);
        }
    }
}
