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

namespace Aiel.Security;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void FullName_FromClaimsPrincipal_ReturnsCombinedName()
    {
        var claims = new List<Claim>
        {
            new(AielClaims.GivenName, "John"),
            new(AielClaims.FamilyName, "Doe")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        var result = principal.FullName();

        Assert.Equal("John Doe", result);
    }

    [Fact]
    public void FullName_FromClaims_ReturnsCombinedName()
    {
        var claims = new List<Claim>
        {
            new(AielClaims.GivenName, "Jane"),
            new(AielClaims.FamilyName, "Smith")
        };

        var result = claims.FullName();

        Assert.Equal("Jane Smith", result);
    }

    [Fact]
    public void FullName_WhenOnlyGivenName_ReturnsGivenName()
    {
        var claims = new List<Claim>
        {
            new(AielClaims.GivenName, "John")
        };

        var result = claims.FullName();

        Assert.Equal("John", result);
    }

    [Fact]
    public void FullName_WhenOnlyFamilyName_ReturnsFamilyName()
    {
        var claims = new List<Claim>
        {
            new(AielClaims.FamilyName, "Doe")
        };

        var result = claims.FullName();

        Assert.Equal("Doe", result);
    }

    [Fact]
    public void FullName_WhenNoClaims_ReturnsEmpty()
    {
        var claims = new List<Claim>();

        var result = claims.FullName();

        Assert.Equal(String.Empty, result);
    }

    [Fact]
    public void FullName_TrimsWhitespace()
    {
        var claims = new List<Claim>
        {
            new(AielClaims.GivenName, "  John  "),
            new(AielClaims.FamilyName, "  Doe  ")
        };

        var result = claims.FullName();

        Assert.Equal("John Doe", result);
    }

    [Fact]
    public void Email_FromClaimsPrincipal_ReturnsEmailString()
    {
        var claims = new List<Claim>
        {
            new(AielClaims.EmailAddress, "john@example.com")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        var result = principal.Email();

        Assert.Equal("john@example.com", result);
    }

    [Fact]
    public void Email_FromClaims_ReturnsEmailString()
    {
        var claims = new List<Claim>
        {
            new(AielClaims.EmailAddress, "jane@example.com")
        };

        var result = claims.Email();

        Assert.Equal("jane@example.com", result);
    }

    [Fact]
    public void EmailAddress_FromClaimsPrincipal_ReturnsEmailAddress()
    {
        var claims = new List<Claim>
        {
            new(AielClaims.GivenName, "John"),
            new(AielClaims.FamilyName, "Doe"),
            new(AielClaims.EmailAddress, "john@example.com")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        var result = principal.EmailAddress();

        Assert.Equal("John Doe", result.Name);
        Assert.Equal("john@example.com", result.Email.ToString());
    }

    [Fact]
    public void EmailAddress_FromClaims_ReturnsEmailAddress()
    {
        var claims = new List<Claim>
        {
            new(AielClaims.GivenName, "Jane"),
            new(AielClaims.FamilyName, "Smith"),
            new(AielClaims.EmailAddress, "jane@example.com")
        };

        var result = claims.EmailAddress();

        Assert.Equal("Jane Smith", result.Name);
        Assert.Equal("jane@example.com", result.Email.ToString());
    }

    [Fact]
    public void ZoneInfo_FromClaimsPrincipal_ReturnsZoneInfo()
    {
        var claims = new List<Claim>
        {
            new(AielClaims.ZoneInfo, "America/New_York")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        var result = principal.ZoneInfo();

        Assert.Equal("America/New_York", result);
    }

    [Fact]
    public void ZoneInfo_FromClaims_ReturnsZoneInfo()
    {
        var claims = new List<Claim>
        {
            new(AielClaims.ZoneInfo, "Europe/London")
        };

        var result = claims.ZoneInfo();

        Assert.Equal("Europe/London", result);
    }

    [Fact]
    public void ZoneInfo_WhenNotProvided_ReturnsDefault()
    {
        var claims = new List<Claim>();

        var result = claims.ZoneInfo();

        Assert.Equal(AielDefaults.TimeZone, result);
    }
}
