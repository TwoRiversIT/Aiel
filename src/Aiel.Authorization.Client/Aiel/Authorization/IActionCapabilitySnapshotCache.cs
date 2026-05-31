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

namespace Aiel.Authorization;

/// <summary>
/// Caches capability snapshots for client-side visibility checks.
/// </summary>
public interface IActionCapabilitySnapshotCache
{
    /// <summary>
    /// Gets a snapshot from the cache or loads it when missing.
    /// </summary>
    /// <param name="request">The snapshot request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The cached or freshly loaded snapshot.</returns>
    ValueTask<Result<ActionCapabilitySnapshot>> GetSnapshotAsync(
        ActionCapabilityRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Forces a snapshot refresh.
    /// </summary>
    /// <param name="request">The snapshot request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The refreshed snapshot.</returns>
    ValueTask<Result<ActionCapabilitySnapshot>> RefreshSnapshotAsync(
        ActionCapabilityRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates a cached snapshot.
    /// </summary>
    /// <param name="request">The snapshot request.</param>
    /// <returns>A completed task.</returns>
    ValueTask InvalidateAsync(ActionCapabilityRequest request);

    /// <summary>
    /// Invalidates and refreshes the snapshot when an authorization failure occurs.
    /// </summary>
    /// <param name="request">The snapshot request.</param>
    /// <param name="actionResult">The action result returned from the server.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current snapshot after any required refresh.</returns>
    ValueTask<Result<ActionCapabilitySnapshot>> HandleAuthorizationFailureAsync(
        ActionCapabilityRequest request,
        Result actionResult,
        CancellationToken cancellationToken = default);
}
