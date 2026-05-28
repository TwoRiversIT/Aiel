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

using Aiel.Results;

namespace Aiel.Permissions;

/// <summary>
/// Default application-layer permission manager that delegates persistence to <see cref="IPermissionStore"/>.
/// </summary>
public sealed class DefaultPermissionManager(
    IPermissionDefinitionRegistry definitionRegistry,
    IPermissionStore permissionStore) : IPermissionManager
{
    /// <inheritdoc />
    public Task<Result<IReadOnlyList<PermissionGrantSummary>>> GetGrantsForSubjectAsync(
        PermissionSubjectTypeName subjectType,
        PermissionSubjectKey subjectKey,
        CancellationToken cancellationToken = default)
        => permissionStore.GetGrantsForSubjectAsync(subjectType, subjectKey, cancellationToken);

    /// <inheritdoc />
    public Task<Result> RevokePermissionAsync(
        RevokePermissionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return permissionStore.RevokeGrantAsync(request.GrantId, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<PermissionGrantId>> GrantPermissionAsync(
        GrantPermissionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!definitionRegistry.TryGet(request.PermissionName, out _))
        {
            return Task.FromResult(
                Result<PermissionGrantId>.Failure(
                    PermissionErrors.MissingAuthorizationStory(request.PermissionName)));
        }

        return permissionStore.CreateGrantAsync(
            request.PermissionName,
            request.ScopeType,
            request.ScopeKey,
            request.SubjectType,
            request.SubjectKey,
            request.Decision,
            cancellationToken);
    }
}
