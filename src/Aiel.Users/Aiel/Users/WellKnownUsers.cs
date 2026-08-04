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

using System.Security.Claims;

namespace Aiel.Users;

public static class WellKnownUsers
{
    public static readonly ClaimsPrincipal AnonymousPrincipal = GetAnonymousClaimsPrincipal();
    public static readonly ClaimsPrincipal SystemPrincipal = GetSystemClaimsPrincipal();
    public static readonly CurrentUser Anonymous = PrincipalCurrentUser.FromClaimsPrincipal(AnonymousPrincipal);

    public const String AnonymousUserFirstName = "Anonymous";
    public const String AnonymousUserLastName = "CurrentUser";
    public const String AnonymousUID = "11111111-1111-1111-1111-111111111111"; // Not 0s because it would be equivalent to Guid.Empty.

    public const String SystemUserFirstName = "System";
    public const String SystemUserLastName = "CurrentUser";
    public const String SystemUID = "ffffffff-ffff-ffff-ffff-ffffffffffff";

    private static ClaimsPrincipal GetAnonymousClaimsPrincipal()
    {
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, AnonymousUID, ClaimValueTypes.String),
            new Claim(ClaimTypes.Name, AnonymousUserFirstName, ClaimValueTypes.String),
            new Claim(ClaimTypes.Surname, AnonymousUserLastName, ClaimValueTypes.String),
            new Claim(ClaimTypes.GivenName, AnonymousUserFirstName, ClaimValueTypes.String)
        ], authenticationType: "Intrinsic", nameType: ClaimTypes.Name, roleType: ClaimTypes.Role);

        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal GetSystemClaimsPrincipal()
    {
        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.Name, SystemUserFirstName, ClaimValueTypes.String),
            new Claim(ClaimTypes.NameIdentifier, SystemUID, ClaimValueTypes.String),
            new Claim(ClaimTypes.Surname, SystemUserLastName, ClaimValueTypes.String),
            new Claim(ClaimTypes.GivenName, SystemUserFirstName, ClaimValueTypes.String)
        ], authenticationType: "Intrinsic", nameType: ClaimTypes.Name, roleType: ClaimTypes.Role);

        return new ClaimsPrincipal(identity);
    }
}
