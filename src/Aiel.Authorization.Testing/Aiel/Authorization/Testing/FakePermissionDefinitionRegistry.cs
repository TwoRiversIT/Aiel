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

using Aiel.Actions;
using System.Diagnostics.CodeAnalysis;

namespace Aiel.Authorization.Testing;

/// <summary>
/// A test-only implementation of <see cref="IAuthorizationDefinitionRegistry"/> backed by an explicit
/// list of <see cref="AuthorizationDefinitionManifest"/> instances.
/// </summary>
/// <remarks>
/// Use this in unit tests to provide a controlled set of registered permissions without relying on
/// production scanning or dependency injection infrastructure.
/// Lookups are matched by <see cref="PermissionName"/> and <see cref="Type"/> respectively.
/// </remarks>
/// <remarks>
/// Initializes a registry pre-populated with the specified <paramref name="manifests"/>.
/// </remarks>
/// <param name="manifests">The manifests to register.</param>
public sealed class FakePermissionDefinitionRegistry(IEnumerable<AuthorizationDefinitionManifest> manifests) : IAuthorizationDefinitionRegistry
{
    private readonly IReadOnlyList<AuthorizationDefinitionManifest> _manifests = [.. manifests];

    /// <summary>
    /// Initializes an empty registry with no registered manifests.
    /// </summary>
    public FakePermissionDefinitionRegistry()
        : this([]) { }

    /// <inheritdoc />
    public IReadOnlyList<AuthorizationDefinitionManifest> GetAll() => _manifests;

    /// <inheritdoc />
    public Boolean TryGet(
        PermissionName permissionName,
        [NotNullWhen(true)] out AuthorizationDefinitionManifest manifest)
    {
        foreach (var m in _manifests)
        {
            if (m.PermissionName == permissionName)
            {
                manifest = m;
                return true;
            }
        }

        manifest = null!;
        return false;
    }

    /// <inheritdoc />
    public Boolean TryGetForAction<TAction>([NotNullWhen(true)] out AuthorizationDefinitionManifest manifest)
        where TAction : IAction
    {
        foreach (var m in _manifests)
        {
            if (m.ActionType == typeof(TAction))
            {
                manifest = m;
                return true;
            }
        }

        manifest = null!;
        return false;
    }
}
