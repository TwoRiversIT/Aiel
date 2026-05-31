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
/// Default client-side capability snapshot cache.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ActionCapabilitySnapshotCache"/> class.
/// </remarks>
/// <param name="capabilityService">The remote capability service.</param>
public sealed class ActionCapabilitySnapshotCache(IActionCapabilityService capabilityService) : IActionCapabilitySnapshotCache
{
    private readonly Dictionary<ActionCapabilityRequestCacheKey, ActionCapabilitySnapshot> _cache = [];
    private readonly IActionCapabilityService _capabilityService = capabilityService ?? throw new ArgumentNullException(nameof(capabilityService));

    /// <inheritdoc />
    public async ValueTask<Result<ActionCapabilitySnapshot>> GetSnapshotAsync(
        ActionCapabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = ActionCapabilityRequestCacheKey.Create(request);
        if (_cache.TryGetValue(cacheKey, out var snapshot))
        {
            return Result.Success(snapshot);
        }

        return await RefreshSnapshotAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async ValueTask<Result<ActionCapabilitySnapshot>> RefreshSnapshotAsync(
        ActionCapabilityRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await _capabilityService.GetSnapshotAsync(request, cancellationToken).ConfigureAwait(false);
        if (result.IsSuccess)
        {
            _cache[ActionCapabilityRequestCacheKey.Create(request)] = result.Value;
        }

        return result;
    }

    /// <inheritdoc />
    public ValueTask InvalidateAsync(ActionCapabilityRequest request)
    {
        _cache.Remove(ActionCapabilityRequestCacheKey.Create(request));
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public async ValueTask<Result<ActionCapabilitySnapshot>> HandleAuthorizationFailureAsync(
        ActionCapabilityRequest request,
        Result actionResult,
        CancellationToken cancellationToken = default)
    {
        if (!actionResult.IsSuccess && actionResult.Error.IsErrorType<PermissionDeniedError>())
        {
            await InvalidateAsync(request).ConfigureAwait(false);
            return await RefreshSnapshotAsync(request, cancellationToken).ConfigureAwait(false);
        }

        return await GetSnapshotAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private readonly record struct ActionCapabilityRequestCacheKey(
        ActionCapabilityRequestMode Mode,
        String ScopeType,
        String ScopeKey,
        String ContinuationToken,
        String RequestedPermissions)
    {
        private const String PermissionsSeparator = "\u001F";

        public static ActionCapabilityRequestCacheKey Create(ActionCapabilityRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            return new ActionCapabilityRequestCacheKey(
                request.Mode,
                request.ScopeType.Value,
                request.ScopeKey.Value,
                request.ContinuationToken.Value,
                String.Join(PermissionsSeparator, request.RequestedPermissions.Select(static permission => permission.Value)));
        }
    }
}
