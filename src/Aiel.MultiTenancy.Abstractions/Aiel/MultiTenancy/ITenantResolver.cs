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

namespace Aiel.MultiTenancy;

/// <summary>
/// Resolves the tenant for the current request context and returns an explicit outcome.
/// </summary>
/// <remarks>
/// Returns one of <see cref="TenantResolution.Resolved"/>, <see cref="TenantResolution.Missing"/>,
/// <see cref="TenantResolution.Ambiguous"/>, <see cref="TenantResolution.Rejected"/>, or
/// <see cref="TenantResolution.Error"/>. Never returns a nullable result.
/// </remarks>
public interface ITenantResolver
{
    /// <summary>Returns the tenant resolution outcome for the current context.</summary>
    /// <param name="cancellationToken">Propagates notification that the operation should be cancelled.</param>
    /// <returns>
    /// A <see cref="TenantResolution"/> describing the outcome. The result is never nullable;
    /// the absence of a tenant is expressed as <see cref="TenantResolution.Missing"/>.
    /// </returns>
    ValueTask<TenantResolution> ResolveAsync(CancellationToken cancellationToken = default);
}
