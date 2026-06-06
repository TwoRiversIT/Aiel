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
/// Discriminated union representing all possible outcomes of tenant resolution.
/// Consumers must match exhaustively; no <c>None</c> or null escape hatch exists.
/// </summary>
public abstract record TenantResolution
{
    private TenantResolution() { }

    /// <summary>Tenant resolved successfully.</summary>
    /// <param name="TenantIdentity">The resolved tenant identity.</param>
    public sealed record Resolved(TenantIdentity TenantIdentity) : TenantResolution;

    /// <summary>No tenant could be inferred from the current request context.</summary>
    public sealed record Missing : TenantResolution;

    /// <summary>
    /// Multiple tenants match the resolution context; disambiguation is required before
    /// a <see cref="TenantIdentity"/> can be materialized.
    /// </summary>
    public sealed record Ambiguous : TenantResolution;

    /// <summary>A tenant was identified but access was explicitly denied.</summary>
    /// <param name="Reason">Typed rejection reason code.</param>
    public sealed record Rejected(TenantRejectionReason Reason) : TenantResolution;

    /// <summary>Tenant resolution failed due to a system or infrastructure error.</summary>
    /// <param name="Reason">Typed error reason code.</param>
    public sealed record Error(TenantResolutionErrorReason Reason) : TenantResolution;
}
