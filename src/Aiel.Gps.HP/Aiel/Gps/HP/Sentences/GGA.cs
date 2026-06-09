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

namespace Aiel.Gps.HP.Sentences;

/// <summary>
/// Represents a GGA (Global Positioning System Fix Data) NMEA message.
/// </summary>
/// <remarks>
/// <para>
/// This sentence provides comprehensive position fix data including UTC time, latitude, longitude,
/// fix quality indicator, number of satellites being tracked, horizontal dilution of precision,
/// altitude above mean sea level, height of geoid, and DGPS information.
/// </para>
/// <para>
/// Example sentence: $GPGGA,123519.000,4916.45,N,12311.12,W,1,5,1.5,545.4,M,46.9,M,,*42
/// </para>
/// </remarks>
[NmeaMessage("GPGGA")]
public struct GGA
{
    /// <summary>UTC time when the position fix was taken (HHMMSS.SSS format).</summary>
    public TimeOnly FixTime;

    /// <summary>Latitude in decimal degrees. Positive = North, Negative = South.</summary>
    public Double Latitude;

    /// <summary>Longitude in decimal degrees. Positive = East, Negative = West.</summary>
    public Double Longitude;

    /// <summary>GPS fix quality indicator.</summary>
    public FixQuality Quality;

    /// <summary>Number of satellites being tracked (00-12).</summary>
    public Int32 NumberOfSatellites;

    /// <summary>
    /// Horizontal dilution of precision (HDOP).
    /// Lower values indicate better position precision. Values under 2.0 are considered excellent.
    /// </summary>
    public Double Hdop;

    /// <summary>Altitude above mean sea level in meters.</summary>
    public Double Altitude;

    /// <summary>Units of altitude measurement (typically 'M' for meters).</summary>
    public Char AltitudeUnits;

    /// <summary>Height of geoid (mean sea level) above WGS84 ellipsoid in meters.</summary>
    public Double HeightOfGeoid;

    /// <summary>Units of geoid height measurement (typically 'M' for meters).</summary>
    public Char HeightOfGeoidUnits;

    /// <summary>Time elapsed since the last DGPS (Differential GPS) update.</summary>
    public TimeOnly TimeSinceLastDgpsUpdate;

    /// <summary>DGPS station ID number (0000-1023).</summary>
    public Int32 DgpsStationId;

    /// <summary>NMEA checksum value.</summary>
    public Int32 Checksum;

    /// <inheritdoc/>
    public override readonly String ToString() => $"{nameof(GGA)} {FixTime} {Latitude} {Longitude} Q:{Quality} Sats:{NumberOfSatellites} Alt:{Altitude:F1}m";
}

/// <summary>
/// Parser for GGA (Global Positioning System Fix Data) NMEA sentences.
/// </summary>
/// <remarks>
/// The GGA sentence is one of the most essential NMEA messages as it provides complete position
/// fix information including quality indicators and precision metrics.
/// </remarks>
[NmeaParser(typeof(GGA))]
public readonly struct GgaParser : INmeaParser<GGA>
{
    /// <summary>
    /// Gets the NMEA sentence identifier for GGA sentences.
    /// </summary>
    public ReadOnlySpan<Byte> Identifier => "GPGGA"u8;

    /// <summary>
    /// Parses a GGA NMEA sentence into a GGA structure.
    /// </summary>
    /// <param name="lexer">The lexer positioned at the start of the sentence.</param>
    /// <param name="msg">The parsed GGA message.</param>
    /// <remarks>
    /// Expected format: $GPGGA,time,lat,lat_dir,lon,lon_dir,quality,satellites,hdop,altitude,alt_units,geoid_height,geoid_units,dgps_time,dgps_id*checksum
    /// </remarks>
    public void Parse(ref Lexer lexer, out GGA msg)
    {
        // Skip the sentence identifier (e.g., "GPGGA")
        lexer.ConsumeString();

        msg = new GGA()
        {
            FixTime = lexer.NextTime(),
            Latitude = lexer.NextLatitude(),
            Longitude = lexer.NextLongitude(),
            Quality = (FixQuality)lexer.NextInteger(),
            NumberOfSatellites = lexer.NextInteger(),
            Hdop = lexer.NextDouble(),
            Altitude = lexer.NextDouble(),
            AltitudeUnits = lexer.NextChar(),
            HeightOfGeoid = lexer.NextDouble(),
            HeightOfGeoidUnits = lexer.NextChar(),
            TimeSinceLastDgpsUpdate = lexer.NextTime(),
            DgpsStationId = lexer.NextInteger(),
            Checksum = lexer.NextChecksum()
        };
    }
}
