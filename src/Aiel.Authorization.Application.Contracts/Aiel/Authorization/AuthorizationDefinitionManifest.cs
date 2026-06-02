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

namespace Aiel.Authorization;

/// <summary>
/// Describes a named permission as declared by the owning bounded context.
/// </summary>
/// <remarks>
/// Manifests are registered in <see cref="IAuthorizationDefinitionRegistry"/> at startup.
/// They carry the metadata needed to display, audit, and enforce permission assignments.
/// </remarks>
public sealed class AuthorizationDefinitionManifest
{
    /// <summary>Gets the stable, unique identifier for this permission definition across renames.</summary>
    public required PermissionStableId StableId { get; init; }

    /// <summary>Gets the canonical name used to resolve this permission at runtime.</summary>
    public required PermissionName PermissionName { get; init; }

    /// <summary>Gets the application action type governed by this permission definition.</summary>
    public required Type ActionType { get; init; }

    /// <summary>Gets the scope type this permission applies to.</summary>
    public required AuthorizationScopeTypeName ScopeType { get; init; }

    /// <summary>Gets the subject type this permission targets.</summary>
    public required AuthorizationSubjectTypeName SubjectType { get; init; }

    /// <summary>Gets a human-readable display name for this permission.</summary>
    public required String DisplayName { get; init; }

    /// <summary>Gets an optional description of what this permission allows or denies.</summary>
    public String Description { get; init; } = String.Empty;

    /// <summary>Gets the published lifecycle state for this permission definition.</summary>
    public PermissionLifecycle Lifecycle { get; init; } = PermissionLifecycle.Active;

    /// <summary>Gets any prior canonical permission names that still map to this stable definition.</summary>
    public IReadOnlyList<PermissionName> PreviousNames { get; init; } = [];
}
