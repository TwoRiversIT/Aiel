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
using static AwesomeAssertions.FluentActions;

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

        result.Should().NotBeNull();
        result.Value.Should().Be("John");
    }

    [Fact]
    public void FirstOrDefault_ReturnsNull_WhenClaimDoesNotExist()
    {
        var claims = new List<Claim>
        {
            new(AielClaims.GivenName, "John")
        };

        var result = claims.FirstOrDefault(AielClaims.FamilyName);

        result.Should().BeNull();
    }

    [Fact]
    public void FirstOrDefault_IsCaseInsensitive()
    {
        var claims = new List<Claim>
        {
            new("AIEL_GIVEN_NAME", "John")
        };

        var result = claims.FirstOrDefault(AielClaims.GivenName);

        result.Should().NotBeNull();
        result.Value.Should().Be("John");
    }

    [Fact]
    public void FirstOrDefault_ThrowsArgumentNullException_WhenClaimsIsNull()
    {
        IEnumerable<Claim> claims = null!;

        Invoking(() => claims.FirstOrDefault(AielClaims.GivenName)).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FirstOrDefaultString_ReturnsValue_WhenClaimExists()
    {
        var claims = new List<Claim>
        {
            new(AielClaims.GivenName, "John")
        };

        var result = claims.FirstOrDefaultString(AielClaims.GivenName);

        result.Should().Be("John");
    }

    [Fact]
    public void FirstOrDefaultString_TrimsValue()
    {
        var claims = new List<Claim>
        {
            new(AielClaims.GivenName, "  John  ")
        };

        var result = claims.FirstOrDefaultString(AielClaims.GivenName);

        result.Should().Be("John");
    }

    [Fact]
    public void FirstOrDefaultString_ReturnsDefault_WhenClaimDoesNotExist()
    {
        var claims = new List<Claim>();

        var result = claims.FirstOrDefaultString(AielClaims.GivenName, "DefaultValue");

        result.Should().Be("DefaultValue");
    }

    [Fact]
    public void FirstOrDefaultString_ReturnsDefault_WhenClaimValueIsEmpty()
    {
        var claims = new List<Claim>
        {
            new(AielClaims.GivenName, "")
        };

        var result = claims.FirstOrDefaultString(AielClaims.GivenName, "DefaultValue");

        result.Should().Be("DefaultValue");
    }

    [Fact]
    public void FirstOrDefaultString_ReturnsDefault_WhenClaimValueIsWhitespace()
    {
        var claims = new List<Claim>
        {
            new(AielClaims.GivenName, "   ")
        };

        var result = claims.FirstOrDefaultString(AielClaims.GivenName, "DefaultValue");

        result.Should().Be("DefaultValue");
    }

    [Fact]
    public void FirstOrDefaultInt32_ReturnsValue_WhenClaimExists()
    {
        var claims = new List<Claim>
        {
            new("age", "42")
        };

        var result = claims.FirstOrDefaultInt32("age");

        result.Should().Be(42);
    }

    [Fact]
    public void FirstOrDefaultInt32_ReturnsDefault_WhenClaimDoesNotExist()
    {
        var claims = new List<Claim>();

        var result = claims.FirstOrDefaultInt32("age", 99);

        result.Should().Be(99);
    }

    [Fact]
    public void FirstOrDefaultInt32_ReturnsDefault_WhenValueCannotBeParsed()
    {
        var claims = new List<Claim>
        {
            new("age", "not-a-number")
        };

        var result = claims.FirstOrDefaultInt32("age", 99);

        result.Should().Be(99);
    }

    [Fact]
    public void FirstOrDefaultInt32_ReturnsDefault_WhenValueIsEmpty()
    {
        var claims = new List<Claim>
        {
            new("age", "")
        };

        var result = claims.FirstOrDefaultInt32("age", 99);

        result.Should().Be(99);
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

        result.Should().Be(guid);
    }

    [Fact]
    public void FirstOrDefaultGuid_ReturnsDefault_WhenClaimDoesNotExist()
    {
        var defaultGuid = Guid.NewGuid();
        var claims = new List<Claim>();

        var result = claims.FirstOrDefaultGuid("id", defaultGuid);

        result.Should().Be(defaultGuid);
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

        result.Should().Be(defaultGuid);
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

        result.Should().Be(defaultGuid);
    }
}
