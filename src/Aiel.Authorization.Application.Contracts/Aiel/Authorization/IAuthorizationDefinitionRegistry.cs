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

namespace Aiel.Authorization;

/// <summary>
/// Provides access to the set of <see cref="AuthorizationDefinitionManifest"/> instances registered at startup.
/// </summary>
/// <remarks>
/// Implementations are typically populated during application bootstrap by scanning
/// bounded-context assemblies that contribute <see cref="AuthorizationDefinitionManifest"/> registrations.
/// </remarks>
public interface IAuthorizationDefinitionRegistry
{
    /// <summary>
    /// Returns all registered permission definition manifests.
    /// </summary>
    /// <returns>A non-null, possibly empty read-only list of manifests.</returns>
    IReadOnlyList<AuthorizationDefinitionManifest> GetAll();

    /// <summary>
    /// Attempts to retrieve the manifest for the specified <paramref name="permissionName"/>.
    /// </summary>
    /// <param name="permissionName">The permission name to look up.</param>
    /// <param name="manifest">
    /// When this method returns <see langword="true"/>, contains the matching manifest;
    /// otherwise <see langword="default"/>.
    /// </param>
    /// <returns><see langword="true"/> if a matching manifest was found; otherwise <see langword="false"/>.</returns>
    Boolean TryGet(
        PermissionName permissionName,
        [NotNullWhen(true)] out AuthorizationDefinitionManifest manifest);

    /// <summary>
    /// Attempts to retrieve the manifest that defines the authorization story for <typeparamref name="TAction"/>.
    /// </summary>
    /// <typeparam name="TAction">The action type to resolve.</typeparam>
    /// <param name="manifest">
    /// When this method returns <see langword="true"/>, contains the matching manifest.
    /// </param>
    /// <returns><see langword="true"/> if a matching manifest was found; otherwise <see langword="false"/>.</returns>
    Boolean TryGetForAction<TAction>([NotNullWhen(true)] out AuthorizationDefinitionManifest manifest)
        where TAction : IAction;
}
