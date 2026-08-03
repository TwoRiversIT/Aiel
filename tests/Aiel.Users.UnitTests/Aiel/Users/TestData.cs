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

using OpenIddict.Abstractions;
using System.Security.Claims;

namespace Aiel.Users;

public static class TestData
{
    public const String UserId = "00000000-0000-0000-0000-000000000001";
    public const String UserName = "test_user";
    public const String UserFirstName = "Test";
    public const String UserLastName = "User";
    public const String UserEmail = "user@example.com";

    public static ClaimsPrincipal GetClaimsPrincipal(UserId userId)
    {
        var identity = new ClaimsIdentity([
            new Claim(OpenIddictConstants.Claims.Subject, userId.ToString(), ClaimValueTypes.String),
            new Claim(OpenIddictConstants.Claims.Name, UserName, ClaimValueTypes.String),
            new Claim(OpenIddictConstants.Claims.FamilyName, UserLastName, ClaimValueTypes.String),
            new Claim(OpenIddictConstants.Claims.GivenName, UserFirstName, ClaimValueTypes.String),
            new Claim(OpenIddictConstants.Claims.Email, UserEmail, ClaimValueTypes.Email),
            new Claim(OpenIddictConstants.Claims.EmailVerified, "true", ClaimValueTypes.Boolean)
        ], authenticationType: "Intrinsic", nameType: OpenIddictConstants.Claims.Name, roleType: OpenIddictConstants.Claims.Role);

        return new ClaimsPrincipal(identity);
    }
}
