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

public class NewNmeaMessageTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void GgaParser_ParsesValidSentence()
    {
        // Arrange
        var sentence = "$GPGGA,123519.000,4916.45,N,12311.12,W,1,5,1.5,545.4,M,46.9,M,,*42\r\n"u8;

        // Act
        var parser = new GgaParser();
        var result = NmeaSingleParser.Parse(sentence, parser);

        // Assert
        result.FixTime.Hour.Should().Be(12);
        result.FixTime.Minute.Should().Be(35);
        result.FixTime.Second.Should().Be(19);
        result.Latitude.Should().BeApproximately(49.27417, 0.00001);
        result.Longitude.Should().BeApproximately(-123.18533, 0.00001);
        result.Quality.Should().Be(FixQuality.GpsFix);
        result.NumberOfSatellites.Should().Be(5);
        result.Hdop.Should().Be(1.5);
        result.Altitude.Should().Be(545.4);
        result.AltitudeUnits.Should().Be('M');
        result.HeightOfGeoid.Should().Be(46.9);
        result.HeightOfGeoidUnits.Should().Be('M');
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void RmcParser_ParsesValidSentence()
    {
        // Arrange
        var sentence = "$GPRMC,225446.000,A,4916.45,N,12311.12,W,0.086,54.7,191194,20.3,E,A*72\r\n"u8;

        // Act
        var parser = new RmcParser();
        var result = NmeaSingleParser.Parse(sentence, parser);

        // Assert
        result.FixTime.Hour.Should().Be(22);
        result.FixTime.Minute.Should().Be(54);
        result.FixTime.Second.Should().Be(46);
        result.Status.Should().Be('A');
        result.Latitude.Should().BeApproximately(49.27417, 0.00001);
        result.Longitude.Should().BeApproximately(-123.18533, 0.00001);
        result.SpeedOverGround.Should().Be(0.086);
        result.TrackAngle.Should().Be(54.7);
        result.Date.Day.Should().Be(19);
        result.Date.Month.Should().Be(11);
        result.Date.Year.Should().Be(1994);
        result.MagneticVariation.Should().Be(20.3);
        result.Direction.Should().Be('E');
        result.Mode.Should().Be('A');
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GsaParser_ParsesValidSentence()
    {
        // Arrange
        var sentence = "$GPGSA,A,3,04,05,,09,12,,,24,,,,,2.5,1.3,2.1*39\r\n"u8;

        // Act
        var parser = new GsaParser();
        var result = NmeaSingleParser.Parse(sentence, parser);

        // Assert
        result.FixMode.Should().Be('A');
        result.FixType.Should().Be(FixType.Fix3D);
        result.Satellites[0].Should().Be(4);
        result.Satellites[1].Should().Be(5);
        result.Satellites[2].Should().Be(0); // Empty slot
        result.Satellites[3].Should().Be(9);
        result.Satellites[4].Should().Be(12);
        result.Satellites[7].Should().Be(24);
        result.Pdop.Should().Be(2.5);
        result.Hdop.Should().Be(1.3);
        result.Vdop.Should().Be(2.1);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void GsvParser_ParsesValidSentence()
    {
        // Arrange
        var sentence = "$GPGSV,2,1,08,01,40,083,46,02,17,308,41,12,07,344,39,14,22,228,45*75\r\n"u8;

        // Act
        var parser = new GsvParser();
        var result = NmeaSingleParser.Parse(sentence, parser);

        // Assert
        result.TotalMessages.Should().Be(2);
        result.MessageNumber.Should().Be(1);
        result.SatellitesInView.Should().Be(8);

        result.SV1.PRN.Should().Be(1);
        result.SV1.Elevation.Should().Be(40);
        result.SV1.Azimuth.Should().Be(83);
        result.SV1.SNR.Should().Be(46);

        result.SV2.PRN.Should().Be(2);
        result.SV2.Elevation.Should().Be(17);
        result.SV2.Azimuth.Should().Be(308);
        result.SV2.SNR.Should().Be(41);

        result.ValidSatelliteCount.Should().Be(4);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void VtgParser_ParsesValidSentence()
    {
        // Arrange
        var sentence = "$GPVTG,054.7,T,034.4,M,005.5,N,010.2,K,A*48\r\n"u8;

        // Act
        var parser = new VtgParser();
        var result = NmeaSingleParser.Parse(sentence, parser);

        // Assert
        result.TrueTrack.Should().Be(54.7);
        result.TrueTrackIndicator.Should().Be('T');
        result.MagneticTrack.Should().Be(34.4);
        result.MagneticTrackIndicator.Should().Be('M');
        result.GroundSpeedN.Should().Be(5.5);
        result.GroundSpeedNIndicator.Should().Be('N');
        result.GroundSpeedK.Should().Be(10.2);
        result.GroundSpeedKIndicator.Should().Be('K');
        result.ModeIndicator.Should().Be('A');
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void NmeaMessage_TryParse_WorksWithAllNewMessageTypes()
    {
        // Test GGA
        var ggaSentence = "$GPGGA,123519.000,4916.45,N,12311.12,W,1,5,1.5,545.4,M,46.9,M,,*42\r\n"u8;
        NmeaMessage.TryParse(ggaSentence, out var ggaMessage).Should().BeTrue();
        ggaMessage.Type.Should().Be(NmeaMessageType.GGA);
        ggaMessage.IsGGA.Should().BeTrue();

        // Test RMC
        var rmcSentence = "$GPRMC,225446.000,A,4916.45,N,12311.12,W,0.086,54.7,191194,20.3,E,A*72\r\n"u8;
        NmeaMessage.TryParse(rmcSentence, out var rmcMessage).Should().BeTrue();
        rmcMessage.Type.Should().Be(NmeaMessageType.RMC);
        rmcMessage.IsRMC.Should().BeTrue();

        // Test GSA
        var gsaSentence = "$GPGSA,A,3,04,05,,09,12,,,24,,,,,2.5,1.3,2.1*39\r\n"u8;
        NmeaMessage.TryParse(gsaSentence, out var gsaMessage).Should().BeTrue();
        gsaMessage.Type.Should().Be(NmeaMessageType.GSA);
        gsaMessage.IsGSA.Should().BeTrue();

        // Test GSV
        var gsvSentence = "$GPGSV,2,1,08,01,40,083,46,02,17,308,41,12,07,344,39,14,22,228,45*75\r\n"u8;
        NmeaMessage.TryParse(gsvSentence, out var gsvMessage).Should().BeTrue();
        gsvMessage.Type.Should().Be(NmeaMessageType.GSV);
        gsvMessage.IsGSV.Should().BeTrue();

        // Test VTG
        var vtgSentence = "$GPVTG,054.7,T,034.4,M,005.5,N,010.2,K,A*48\r\n"u8;
        NmeaMessage.TryParse(vtgSentence, out var vtgMessage).Should().BeTrue();
        vtgMessage.Type.Should().Be(NmeaMessageType.VTG);
        vtgMessage.IsVTG.Should().BeTrue();
    }
}
