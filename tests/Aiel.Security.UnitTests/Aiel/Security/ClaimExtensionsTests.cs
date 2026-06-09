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

public class ClaimExtensionsTests
{
    [Fact]
    public void FirstOrDefault_ReturnsClaim_WhenClaimExists()
    {
        var claims = new List<Claim>
        {
            new(AielClaims.GivenName, "John"),
            new(AielClaims.FamilyName, "Doe")
        };

        var result = claims.FirstOrDefault(AielClaims.GivenName);

        Assert.NotNull(result);
        Assert.Equal("John", result.Value);
    }

    [Fact]
    public void FirstOrDefault_ReturnsNull_WhenClaimDoesNotExist()
    {
        var claims = new List<Claim>
        {
            new(AielClaims.GivenName, "John")
        };

        var result = claims.FirstOrDefault(AielClaims.FamilyName);

        Assert.Null(result);
    }

    [Fact]
    public void FirstOrDefault_IsCaseInsensitive()
    {
        var claims = new List<Claim>
        {
            new("TR_GIVEN_NAME", "John")
        };

        var result = claims.FirstOrDefault(AielClaims.GivenName);

        Assert.NotNull(result);
        Assert.Equal("John", result.Value);
    }

    [Fact]
    public void FirstOrDefault_ThrowsArgumentNullException_WhenClaimsIsNull()
    {
        IEnumerable<Claim> claims = null!;

        Assert.Throws<ArgumentNullException>(() => claims.FirstOrDefault(AielClaims.GivenName));
    }

    [Fact]
    public void FirstOrDefaultString_ReturnsValue_WhenClaimExists()
    {
        var claims = new List<Claim>
        {
            new(AielClaims.GivenName, "John")
        };

        var result = claims.FirstOrDefaultString(AielClaims.GivenName);

        Assert.Equal("John", result);
    }

    [Fact]
    public void FirstOrDefaultString_TrimsValue()
    {
        var claims = new List<Claim>
        {
            new(AielClaims.GivenName, "  John  ")
        };

        var result = claims.FirstOrDefaultString(AielClaims.GivenName);

        Assert.Equal("John", result);
    }

    [Fact]
    public void FirstOrDefaultString_ReturnsDefault_WhenClaimDoesNotExist()
    {
        var claims = new List<Claim>();

        var result = claims.FirstOrDefaultString(AielClaims.GivenName, "DefaultValue");

        Assert.Equal("DefaultValue", result);
    }

    [Fact]
    public void FirstOrDefaultString_ReturnsDefault_WhenClaimValueIsEmpty()
    {
        var claims = new List<Claim>
        {
            new(AielClaims.GivenName, "")
        };

        var result = claims.FirstOrDefaultString(AielClaims.GivenName, "DefaultValue");

        Assert.Equal("DefaultValue", result);
    }

    [Fact]
    public void FirstOrDefaultString_ReturnsDefault_WhenClaimValueIsWhitespace()
    {
        var claims = new List<Claim>
        {
            new(AielClaims.GivenName, "   ")
        };

        var result = claims.FirstOrDefaultString(AielClaims.GivenName, "DefaultValue");

        Assert.Equal("DefaultValue", result);
    }

    [Fact]
    public void FirstOrDefaultInt32_ReturnsValue_WhenClaimExists()
    {
        var claims = new List<Claim>
        {
            new("age", "42")
        };

        var result = claims.FirstOrDefaultInt32("age");

        Assert.Equal(42, result);
    }

    [Fact]
    public void FirstOrDefaultInt32_ReturnsDefault_WhenClaimDoesNotExist()
    {
        var claims = new List<Claim>();

        var result = claims.FirstOrDefaultInt32("age", 99);

        Assert.Equal(99, result);
    }

    [Fact]
    public void FirstOrDefaultInt32_ReturnsDefault_WhenValueCannotBeParsed()
    {
        var claims = new List<Claim>
        {
            new("age", "not-a-number")
        };

        var result = claims.FirstOrDefaultInt32("age", 99);

        Assert.Equal(99, result);
    }

    [Fact]
    public void FirstOrDefaultInt32_ReturnsDefault_WhenValueIsEmpty()
    {
        var claims = new List<Claim>
        {
            new("age", "")
        };

        var result = claims.FirstOrDefaultInt32("age", 99);

        Assert.Equal(99, result);
    }

    [Fact]
    public void FirstOrDefaultGuid_ReturnsValue_WhenClaimExists()
    {
        var guid = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new("id", guid.ToString())
        };

        var result = claims.FirstOrDefaultGuid("id");

        Assert.Equal(guid, result);
    }

    [Fact]
    public void FirstOrDefaultGuid_ReturnsDefault_WhenClaimDoesNotExist()
    {
        var defaultGuid = Guid.NewGuid();
        var claims = new List<Claim>();

        var result = claims.FirstOrDefaultGuid("id", defaultGuid);

        Assert.Equal(defaultGuid, result);
    }

    [Fact]
    public void FirstOrDefaultGuid_ReturnsDefault_WhenValueCannotBeParsed()
    {
        var defaultGuid = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new("id", "not-a-guid")
        };

        var result = claims.FirstOrDefaultGuid("id", defaultGuid);

        Assert.Equal(defaultGuid, result);
    }

    [Fact]
    public void FirstOrDefaultGuid_ReturnsDefault_WhenValueIsEmpty()
    {
        var defaultGuid = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new("id", "")
        };

        var result = claims.FirstOrDefaultGuid("id", defaultGuid);

        Assert.Equal(defaultGuid, result);
    }
}
