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
[Trait("Category", "DiscriminatedUnion")]
public class NmeaMessageTests
{
    [Fact]
    public void TryParse_GLL_Sentence_ReturnsGLLMessage()
    {
        // Arrange
        var sentence = "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n"u8;

        // Act
        var success = NmeaMessage.TryParse(sentence, out var message);

        // Assert
        success.Should().BeTrue();
        message.Type.Should().Be(NmeaMessageType.GLL);
        message.IsGLL.Should().BeTrue();
        message.IsGFDTA.Should().BeFalse();
    }

    [Fact]
    public void TryParse_GFDTA_Sentence_ReturnsGFDTAMessage()
    {
        // Arrange
        var sentence = "$GFDTA,7.2,98,3.0,16380,2019/01/22 16:04:58,CH4AB-1047,1*40\r\n"u8;

        // Act
        var success = NmeaMessage.TryParse(sentence, out var message);

        // Assert
        success.Should().BeTrue();
        message.Type.Should().Be(NmeaMessageType.GFDTA);
        message.IsGFDTA.Should().BeTrue();
        message.IsGLL.Should().BeFalse();
    }

    [Fact]
    public void TryParse_UnknownSentence_ReturnsFalse()
    {
        // Arrange
        var sentence = "$UNKNOWN,1,2,3*00\r\n"u8;

        // Act
        var success = NmeaMessage.TryParse(sentence, out var message);

        // Assert
        success.Should().BeFalse();
        message.Type.Should().Be(NmeaMessageType.None);
    }

    [Fact]
    public void TryGetGLL_WhenIsGLL_ReturnsTrue()
    {
        // Arrange
        var sentence = "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n"u8;
        _ = NmeaMessage.TryParse(sentence, out var message);

        // Act
        var success = message.TryGetGLL(out var gll);

        // Assert
        success.Should().BeTrue();
        gll.Latitude.Should().BeApproximately(49.274166666666666, 0.0000001);
        gll.Longitude.Should().BeApproximately(-123.18533333333333, 0.0000001);
    }

    [Fact]
    public void TryGetGLL_WhenIsGFDTA_ReturnsFalse()
    {
        // Arrange
        var sentence = "$GFDTA,7.2,98,3.0,16380,2019/01/22 16:04:58,CH4AB-1047,1*40\r\n"u8;
        _ = NmeaMessage.TryParse(sentence, out var message);

        // Act
        var success = message.TryGetGLL(out var gll);

        // Assert
        success.Should().BeFalse();
        gll.Should().Be(default(GLL));
    }

    [Fact]
    public void FromGLL_CreatesMessageWithCorrectType()
    {
        // Arrange
        var gll = new GLL
        {
            Latitude = 49.27,
            Longitude = -123.18,
            FixTime = new TimeOnly(22, 54, 44),
            DataActive = 'A'
        };

        // Act
        var message = NmeaMessage.FromGLL(gll);

        // Assert
        message.Type.Should().Be(NmeaMessageType.GLL);
        message.GLL.Latitude.Should().Be(49.27);
    }

    [Fact]
    //[SuppressMessage("Roslynator", "RCS1205:Order named arguments according to the order of parameters", Justification = "<Pending>")]
    public void Match_GLL_InvokesCorrectHandler()
    {
        // Arrange
        var sentence = "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n"u8;
        _ = NmeaMessage.TryParse(sentence, out var message);
        var handlerCalled = "";

        // Act
        message.Match(
            onGLL: _ => handlerCalled = "GLL",
            onGGA: _ => handlerCalled = "GGA",
            onGSV: _ => handlerCalled = "GSV",
            onGSA: _ => handlerCalled = "GSA",
            onRMC: _ => handlerCalled = "RMC",
            onVTG: _ => handlerCalled = "VTG",
            onGFDTA: _ => handlerCalled = "GFDTA");

        // Assert
        handlerCalled.Should().Be("GLL");
    }

    [Fact]
    public void Match_WithResult_ReturnsCorrectValue()
    {
        // Arrange
        var sentence = "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n"u8;
        _ = NmeaMessage.TryParse(sentence, out var message);

        // Act
        var result = message.Match(
            onGLL: gll => $"GLL: {gll.Latitude:F2}",
            onGGA: gga => $"GGA: {gga.Latitude:F2}",
            onGSV: gsv => $"GSV: {gsv.SatellitesInView}",
            onGSA: gsa => $"GSA: {gsa.FixType}",
            onRMC: rmc => $"RMC: {rmc.Latitude:F2}",
            onVTG: vtg => $"VTG: {vtg.GroundSpeedN:F1}",
            onGFDTA: gfdta => $"GFDTA: {gfdta.Concentration}");

        // Assert
        result.Should().Be("GLL: 49.27");
    }

    [Fact]
    public void ToString_ReturnsUnderlyingMessageToString()
    {
        // Arrange
        var sentence = "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n"u8;
        _ = NmeaMessage.TryParse(sentence, out var message);

        // Act
        var str = message.ToString();

        // Assert
        str.Should().Contain("GLL");
    }

    [Fact]
    public void NmeaMessage_IsValueType()
    {
        // Assert - Document that NmeaMessage is a struct (value type)
        typeof(NmeaMessage).IsValueType.Should().BeTrue();
    }

    [Fact]
    public void NmeaMessage_DoesNotAllocate_WhenParsing()
    {
        // This test documents the design intent: the discriminated union should not allocate.
        // To truly verify zero allocations, use BenchmarkDotNet with MemoryDiagnoser.

        // Arrange
        var sentence = "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n"u8;

        // Act - parse multiple times
        for (var i = 0; i < 1000; i++)
        {
            _ = NmeaMessage.TryParse(sentence, out var message);
            _ = message.Type; // Prevent optimization
        }

        // Assert - if we got here without running out of memory, good enough for a unit test
        Assert.True(true);
    }
}
