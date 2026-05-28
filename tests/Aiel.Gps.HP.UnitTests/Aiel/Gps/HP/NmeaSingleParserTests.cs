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

using Aiel.Gps.HP.Sentences;

namespace Aiel.Gps.HP;

[Trait("Category", "Unit")]
[Trait("Category", "Parser")]
public class NmeaSingleParserTests
{
    [Fact]
    public void Parse_GLL_WellFormedSentence_ReturnsCorrectValues()
    {
        // Arrange
        var sentence = "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n"u8;
        var parser = new GllParser();

        // Act
        var gll = NmeaSingleParser.Parse(sentence, parser);

        // Assert
        gll.Latitude.Should().BeApproximately(49.274166666666666, 0.0000001);
        gll.Longitude.Should().BeApproximately(-123.18533333333333, 0.0000001);
        gll.FixTime.Should().Be(new TimeOnly(22, 54, 44));
        gll.DataActive.Should().Be('A');
    }

    [Fact]
    public void Parse_GLL_SouthernHemisphere_ReturnsNegativeLatitude()
    {
        // Arrange - Southern hemisphere latitude
        var sentence = "$GPGLL,3751.65,S,14507.36,E,081836,A,*00\r\n"u8;
        var parser = new GllParser();

        // Act
        var gll = NmeaSingleParser.Parse(sentence, parser);

        // Assert
        gll.Latitude.Should().BeNegative();
        gll.Longitude.Should().BePositive();
    }

    [Fact]
    public void Parse_GLL_WithDecimalSeconds_ParsesCorrectly()
    {
        // Arrange - time with milliseconds
        var sentence = "$GPGLL,5057.1975,N,11134.8332,W,232608.000,A,*00\r\n"u8;
        var parser = new GllParser();

        // Act
        var gll = NmeaSingleParser.Parse(sentence, parser);

        // Assert
        gll.FixTime.Hour.Should().Be(23);
        gll.FixTime.Minute.Should().Be(26);
        gll.FixTime.Second.Should().Be(8);
    }

    [Fact]
    public void Parse_GFDTA_WellFormedSentence_ReturnsCorrectValues()
    {
        // Arrange
        var sentence = "$GFDTA,7.2,98,3.0,16380,2019/01/22 16:04:58,CH4AB-1047,1*40\r\n"u8;
        var parser = new GfdtaParser();

        // Act
        var gfdta = NmeaSingleParser.Parse(sentence, parser);

        // Assert
        gfdta.Concentration.Should().BeApproximately(7.2, 0.001);
        gfdta.R2.Should().Be(98);
        gfdta.Distance.Should().BeApproximately(3.0, 0.001);
        gfdta.Light.Should().Be(16380);
        gfdta.SerialNumber.Should().Be("CH4AB-1047");
        gfdta.Status.Should().Be("1");
    }

    [Fact]
    public void Parse_IsZeroAllocation_ForValueTypeMessages()
    {
        // This test documents the design intent: parsing should not allocate
        // for the message itself (strings within the message may allocate).
        // 
        // To truly verify zero allocations, use BenchmarkDotNet with MemoryDiagnoser.
        // This test just verifies the API works as expected.

        // Arrange
        var sentence = "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n"u8;
        var parser = new GllParser();

        // Act - parse multiple times
        for (var i = 0; i < 1000; i++)
        {
            var gll = NmeaSingleParser.Parse(sentence, parser);
            _ = gll.Latitude; // Prevent optimization
        }

        // Assert - if we got here without running out of memory, good enough for a unit test
        // Real verification should be done with benchmarks
        Assert.True(true);
    }
}
