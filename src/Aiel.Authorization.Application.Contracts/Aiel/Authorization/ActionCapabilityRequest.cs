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
/// Requests a capability snapshot for a scope and permission selection mode.
/// </summary>
public sealed class ActionCapabilityRequest
{
    private static readonly IReadOnlyList<PermissionName> EmptyRequestedPermissions = [];

    private ActionCapabilityRequest(
        AuthorizationScopeTypeName scopeType,
        AuthorizationScopeKey scopeKey,
        ActionCapabilityRequestMode mode,
        IEnumerable<PermissionName>? requestedPermissions,
        CapabilityContinuationToken continuationToken)
    {
        ScopeType = scopeType;
        ScopeKey = scopeKey;
        Mode = mode;
        RequestedPermissions = NormalizeRequestedPermissions(requestedPermissions);
        ContinuationToken = continuationToken;
    }

    /// <summary>
    /// Gets the permission scope type the snapshot is bound to.
    /// </summary>
    public AuthorizationScopeTypeName ScopeType { get; }

    /// <summary>
    /// Gets the permission scope key the snapshot is bound to.
    /// </summary>
    public AuthorizationScopeKey ScopeKey { get; }

    /// <summary>
    /// Gets the selection mode for this request.
    /// </summary>
    public ActionCapabilityRequestMode Mode { get; }

    /// <summary>
    /// Gets the explicitly requested permissions.
    /// </summary>
    public IReadOnlyList<PermissionName> RequestedPermissions { get; }

    /// <summary>
    /// Gets the continuation token for the requested page.
    /// </summary>
    public CapabilityContinuationToken ContinuationToken { get; }

    /// <summary>
    /// Creates a request for the full capability catalog in a scope.
    /// </summary>
    /// <param name="scopeType">The scope type.</param>
    /// <param name="scopeKey">The scope key.</param>
    /// <param name="continuationToken">The continuation token.</param>
    /// <returns>A request for all permissions in the supplied scope.</returns>
    public static ActionCapabilityRequest ForAllPermissions(
        AuthorizationScopeTypeName scopeType,
        AuthorizationScopeKey scopeKey,
        CapabilityContinuationToken continuationToken)
        => new(scopeType, scopeKey, ActionCapabilityRequestMode.AllPermissions, EmptyRequestedPermissions, continuationToken);

    /// <summary>
    /// Creates a request for a selected permission subset in a scope.
    /// </summary>
    /// <param name="scopeType">The scope type.</param>
    /// <param name="scopeKey">The scope key.</param>
    /// <param name="requestedPermissions">The selected permissions.</param>
    /// <param name="continuationToken">The continuation token.</param>
    /// <returns>A request for the selected permissions.</returns>
    public static ActionCapabilityRequest ForSelectedPermissions(
        AuthorizationScopeTypeName scopeType,
        AuthorizationScopeKey scopeKey,
        IEnumerable<PermissionName>? requestedPermissions,
        CapabilityContinuationToken continuationToken)
        => new(scopeType, scopeKey, ActionCapabilityRequestMode.SelectedPermissions, requestedPermissions, continuationToken);

    private static IReadOnlyList<PermissionName> NormalizeRequestedPermissions(IEnumerable<PermissionName>? requestedPermissions)
    {
        if (requestedPermissions is null)
        {
            return EmptyRequestedPermissions;
        }

        var normalizedPermissions = requestedPermissions
            .Where(static permission => !String.IsNullOrWhiteSpace(permission.Value))
            .Distinct()
            .OrderBy(static permission => permission.Value, StringComparer.Ordinal)
            .ToArray();

        return normalizedPermissions.Length == 0 ? EmptyRequestedPermissions : normalizedPermissions;
    }
}
