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
/// A domain-neutral summary of a persisted permission grant returned by <see cref="IAuthorizationGrantStore"/>.
/// </summary>
/// <remarks>
/// This DTO intentionally omits domain entity internals. Callers receive only the information
/// needed to display, audit, or revoke a grant.
/// </remarks>
public sealed class AuthorizationGrantSummary
{
    /// <summary>Gets the unique identifier for this persisted grant.</summary>
    public required AuthorizationGrantId GrantId { get; init; }

    /// <summary>Gets the name of the permission this grant covers.</summary>
    public required PermissionName PermissionName { get; init; }

    /// <summary>Gets the scope type this grant applies to.</summary>
    public required AuthorizationScopeTypeName ScopeType { get; init; }

    /// <summary>Gets the specific scope key this grant is bound to.</summary>
    public required AuthorizationScopeKey ScopeKey { get; init; }

    /// <summary>Gets the subject type this grant targets.</summary>
    public required AuthorizationSubjectTypeName SubjectType { get; init; }

    /// <summary>Gets the specific subject key this grant is bound to.</summary>
    public required AuthorizationSubjectKey SubjectKey { get; init; }

    /// <summary>Gets whether this grant explicitly allows or prohibits the permission.</summary>
    public required AuthorizationGrantDecision Decision { get; init; }
}
