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

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace Aiel.Security;

public static class ClaimExtensions
{
    public static Guid FirstOrDefaultGuid([NotNull] this IEnumerable<Claim> claims, String claimType, Guid defaultValue = default)
    {
        var claimOrNull = claims.FirstOrDefault(claimType);
        if (claimOrNull == null || String.IsNullOrWhiteSpace(claimOrNull.Value))
        {
            return defaultValue;
        }

        if (Guid.TryParse(claimOrNull.Value, out var value))
        {
            return value;
        }

        return defaultValue;
    }

    public static Int32 FirstOrDefaultInt32([NotNull] this IEnumerable<Claim> claims, String claimType, Int32 defaultValue = default)
    {
        var claimOrNull = claims.FirstOrDefault(claimType);
        if (claimOrNull == null || String.IsNullOrWhiteSpace(claimOrNull.Value))
        {
            return defaultValue;
        }

        if (Int32.TryParse(claimOrNull.Value, out var value))
        {
            return value;
        }

        return defaultValue;
    }

    public static String? FirstOrDefaultString([NotNull] this IEnumerable<Claim> claims, String claimType, String? defaultValue = default)
    {
        var claimOrNull = claims.FirstOrDefault(claimType);
        if (claimOrNull == null || String.IsNullOrWhiteSpace(claimOrNull.Value))
        {
            return defaultValue;
        }

        return claimOrNull.Value.Trim();
    }

    public static Claim? FirstOrDefault([NotNull] this IEnumerable<Claim> claims, String claimType)
    {
        ArgumentNullException.ThrowIfNull(claims);

        return claims.FirstOrDefault(c => String.Equals(c.Type, claimType, StringComparison.OrdinalIgnoreCase));
    }
}
