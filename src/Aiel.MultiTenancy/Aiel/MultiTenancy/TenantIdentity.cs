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
/// Immutable, framework-owned tenant identity. Carries only what is needed for identification
/// and routing hint propagation. Storage topology, connection data, and membership details
/// belong in separate runtime binding contracts.
/// </summary>
/// <param name="TenantId">The unique tenant identifier.</param>
/// <param name="HostHint">
/// Optional host or subdomain routing hint. Treat as a hint only — not as an authorization
/// primitive. Empty string when absent.
/// </param>
public sealed record TenantIdentity(TenantId TenantId, String HostHint = "")
{
    public static readonly TenantIdentity Empty = new(new TenantId(Guid.Empty));
}
