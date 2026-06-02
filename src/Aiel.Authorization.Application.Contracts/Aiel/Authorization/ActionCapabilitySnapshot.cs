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
/// Represents a versioned capability snapshot for a specific scope.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ActionCapabilitySnapshot"/> class.
/// </remarks>
/// <param name="version">The snapshot version.</param>
/// <param name="scopeType">The scope type.</param>
/// <param name="scopeKey">The scope key.</param>
/// <param name="capabilities">The capability entries in the snapshot.</param>
public sealed class ActionCapabilitySnapshot(
    CapabilitySnapshotVersion version,
    AuthorizationScopeTypeName scopeType,
    AuthorizationScopeKey scopeKey,
    IEnumerable<ClientAuthorizationCapability>? capabilities)
{
    private static readonly IReadOnlyList<ClientAuthorizationCapability> EmptyCapabilities = [];

    /// <summary>
    /// Gets an empty snapshot suitable for default UI state.
    /// </summary>
    public static ActionCapabilitySnapshot Empty { get; } = new(
        CapabilitySnapshotVersion.From("empty"),
        AuthorizationScopeTypeName.From("Unknown"),
        AuthorizationScopeKey.From("empty"),
        EmptyCapabilities);

    private readonly IReadOnlyList<ClientAuthorizationCapability> _capabilities = capabilities?.ToArray() ?? EmptyCapabilities;

    /// <summary>
    /// Gets the snapshot version.
    /// </summary>
    public CapabilitySnapshotVersion Version { get; } = version.IsDefault ? Empty.Version : version;

    /// <summary>
    /// Gets the permission scope type represented by this snapshot.
    /// </summary>
    public AuthorizationScopeTypeName ScopeType { get; } = scopeType;

    /// <summary>
    /// Gets the permission scope key represented by this snapshot.
    /// </summary>
    public AuthorizationScopeKey ScopeKey { get; } = scopeKey;

    /// <summary>
    /// Gets the permission capability entries included in the snapshot.
    /// </summary>
    public IReadOnlyList<ClientAuthorizationCapability> Capabilities => _capabilities;
}
