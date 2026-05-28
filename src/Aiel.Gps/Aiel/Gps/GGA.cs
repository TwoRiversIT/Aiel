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

using System.Buffers;
using System.Text;

namespace Aiel.Gps;

/// <summary>
/// Represents a GPGGA (Global Positioning System Fix Data) NMEA sentence.
/// </summary>
/// <remarks>
/// This sentence provides position fix data including time, position, fix quality, number of satellites,
/// altitude, and geoid height. This is one of the most commonly used GPS sentences.
/// </remarks>
public class GGA : NmeaMessage
{
    public const String Identifier = "GPGGA";

    private static readonly ReadOnlyMemory<Byte> KEY = Encoding.UTF8.GetBytes($"${Identifier}").AsMemory();

    /// <inheritdoc/>
    protected override ReadOnlyMemory<Byte> Key => KEY;

    /// <inheritdoc/>
    public override NmeaMessage Parse(ReadOnlySequence<Byte> sentence)
    {
        var lexer = LexerFactory.Create(sentence);

        // Skip over GPGGA
        lexer.SkipString();

        // $GPGGA,232608.000,5057.1975,N,11134.8332,W,2,8,1.06,781.7,M,-18.1,M,0000,0000*62
        return new GGA()
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

    /// <summary>
    /// Gets the UTC time when the fix was taken.
    /// </summary>
    public TimeOnly FixTime { get; private set; }

    /// <summary>
    /// Gets the latitude in decimal degrees. Positive values are North, negative values are South.
    /// </summary>
    public Double Latitude { get; private set; }

    /// <summary>
    /// Gets the longitude in decimal degrees. Positive values are East, negative values are West.
    /// </summary>
    public Double Longitude { get; private set; }

    /// <summary>
    /// Gets the GPS fix quality indicator.
    /// </summary>
    public FixQuality Quality { get; private set; }

    /// <summary>
    /// Gets the number of satellites being tracked.
    /// </summary>
    public Int32 NumberOfSatellites { get; private set; }

    /// <summary>
    /// Gets the horizontal dilution of precision (HDOP).
    /// </summary>
    /// <remarks>
    /// Lower values indicate better position precision. Values under 2.0 are considered excellent.
    /// </remarks>
    public Double Hdop { get; private set; }

    /// <summary>
    /// Gets the altitude above mean sea level in meters.
    /// </summary>
    public Double Altitude { get; private set; }

    /// <summary>
    /// Gets the units of altitude measurement (typically 'M' for meters).
    /// </summary>
    public Char AltitudeUnits { get; private set; }

    /// <summary>
    /// Gets the height of geoid (mean sea level) above WGS84 ellipsoid in meters.
    /// </summary>
    public Double HeightOfGeoid { get; private set; }

    /// <summary>
    /// Gets the units of geoid height measurement (typically 'M' for meters).
    /// </summary>
    public Char HeightOfGeoidUnits { get; private set; }

    /// <summary>
    /// Gets the time elapsed since the last DGPS (Differential GPS) update.
    /// </summary>
    public TimeOnly TimeSinceLastDgpsUpdate { get; private set; }

    /// <summary>
    /// Gets the DGPS station ID number.
    /// </summary>
    public Int32 DgpsStationId { get; private set; }

    /// <inheritdoc/>
    public override String ToString() => $"GPGGA {FixTime} {Latitude} {Longitude} {Quality} {NumberOfSatellites}";
}
