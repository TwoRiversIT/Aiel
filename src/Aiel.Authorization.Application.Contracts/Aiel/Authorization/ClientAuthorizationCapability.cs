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
/// Declares a named permission that a client instance supports, together with its current grant state.
/// </summary>
/// <remarks>
/// Client capability snapshots are versioned via <see cref="CapabilitySnapshotVersion"/>.
/// They allow the server to proactively push permission state rather than requiring per-request checks.
/// </remarks>
public sealed class ClientAuthorizationCapability
{
    /// <summary>Gets the version token for the snapshot this capability belongs to.</summary>
    public required CapabilitySnapshotVersion SnapshotVersion { get; init; }

    /// <summary>Gets the name of the declared permission.</summary>
    public required PermissionName PermissionName { get; init; }

    /// <summary>Gets the scope type this capability covers.</summary>
    public required AuthorizationScopeTypeName ScopeType { get; init; }

    /// <summary>Gets the specific scope key this capability is bound to.</summary>
    public required AuthorizationScopeKey ScopeKey { get; init; }

    /// <summary>Gets the current decision the server holds for this subject+scope combination.</summary>
    public required AuthorizationGrantDecision Decision { get; init; }
}
