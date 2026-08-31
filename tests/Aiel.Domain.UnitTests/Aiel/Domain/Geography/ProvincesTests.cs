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

namespace Aiel.Domain.Geography;

public class ProvincesTests
{
    [Fact]
    public void All_ShouldContainAllProvinces()
    {
        // Arrange
        var expectedProvinces = new[]
        {
            Provinces.AB, Provinces.BC, Provinces.MB, Provinces.NB, Provinces.NL,
            Provinces.NS, Provinces.ON, Provinces.PE, Provinces.QC, Provinces.SK,
            Provinces.NT, Provinces.NU, Provinces.YT
        };

        // Act
        var allProvinces = Provinces.All.ToList();

        // Assert
        allProvinces.Count.Should().Be(expectedProvinces.Length);
        expectedProvinces.Should().AllSatisfy(province => allProvinces.Should().Contain(province));
    }

    [Theory]
    [InlineData("AB", "AB")]
    [InlineData("Alberta", "AB")]
    [InlineData("Québec", "QC")]
    [InlineData("Newfoundland and Labrador", "NL")]
    [InlineData("Invalid", "Invalid")]
    [InlineData(null, "")]
    [InlineData("", "")]
    public void ToProvince_ShouldReturnCorrectCodeOrInput(String? input, String expected)
    {
        // Act
        var result = input.ToProvince();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("AB", true, "AB")]
    [InlineData("Alberta", true, "AB")]
    [InlineData("Invalid", false, "")]
    [InlineData(null, false, "")]
    public void TryParse_ShouldReturnCorrectResult(String? input, Boolean expectedSuccess, String expectedCode)
    {
        // Act
        var success = Provinces.TryParse(input, out var province);

        // Assert
        success.Should().Be(expectedSuccess);
        province.Code.Should().Be(expectedCode);
    }

    [Theory]
    [InlineData("AB", "AB")]
    [InlineData("Alberta", "AB")]
    [InlineData("Québec", "QC")]
    [InlineData("Invalid", null)]
    public void Parse_ShouldReturnCorrectProvinceOrThrow(String input, String? expectedCode)
    {
        if (expectedCode == null)
        {
            // Act
            var act = () => Provinces.Parse(input);

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>();
        }
        else
        {
            // Act
            var result = Provinces.Parse(input);

            // Assert
            result.Code.Should().Be(expectedCode);
        }
    }

    [Fact]
    public void Parse_ShouldReturnEmptyProvince_WhenInvalidAndThrowOnInvalidIsFalse()
    {
        // Act
        var result = Provinces.Parse("Invalid", throwOnInvalid: false);

        // Assert
        result.Should().Be(Province.Empty);
    }
}
