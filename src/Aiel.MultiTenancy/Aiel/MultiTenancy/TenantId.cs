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

using Aiel.StrongIds;

namespace Aiel.MultiTenancy;

/// <summary>
/// Identifies a tenant. Guid-backed strong identifier; never exposed as a raw <see cref="Guid"/>.
/// </summary>
public readonly record struct TenantId : IStrongId<Guid>
{
    /// <inheritdoc />
    public Guid Value { get; }

    /// <summary>Initializes a <see cref="TenantId"/> from a <see cref="Guid"/>.</summary>
    public TenantId(Guid value) => Value = value;

    /// <summary>Creates a new random <see cref="TenantId"/>.</summary>
    public static TenantId NewTenantId() => new(Guid.NewGuid());

    /// <summary>
    /// Attempts to create a <see cref="TenantId"/> from <paramref name="value"/>.
    /// Returns <see langword="false"/> when <paramref name="value"/> is <see cref="Guid.Empty"/>.
    /// </summary>
    public static Boolean TryFrom(Guid value, out TenantId tenantId)
    {
        if (value == Guid.Empty)
        {
            tenantId = default;
            return false;
        }

        tenantId = new TenantId(value);
        return true;
    }
}
