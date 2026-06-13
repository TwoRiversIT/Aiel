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

using Aiel.Emailing.Abstractions.Aiel.Emailing;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace Aiel.Security;

public static class ClaimsPrincipalExtensions
{
    public static String FullName([NotNull] this ClaimsPrincipal principal)
        => principal.Claims.FullName();
    public static String FullName([NotNull] this IEnumerable<Claim> claims)
        => $"{claims.FirstOrDefaultString(AielClaims.GivenName)} {claims.FirstOrDefaultString(AielClaims.FamilyName)}".Trim();

    public static String Email([NotNull] this ClaimsPrincipal principal)
        => principal.Claims.Email();
    public static String Email([NotNull] this IEnumerable<Claim> claims)
        => $"{claims.FirstOrDefaultString(AielClaims.EmailAddress)}";

    public static EmailAddress EmailAddress([NotNull] this ClaimsPrincipal principal)
        => principal.Claims.EmailAddress();
    public static EmailAddress EmailAddress([NotNull] this IEnumerable<Claim> claims)
        => new(claims.FullName(), claims.Email());

    public static String ZoneInfo([NotNull] this ClaimsPrincipal principal)
        => principal.Claims.ZoneInfo();
    public static String ZoneInfo(this IEnumerable<Claim> claims)
        => claims.FirstOrDefaultString(AielClaims.ZoneInfo)
        ?? AielDefaults.TimeZone;
}

