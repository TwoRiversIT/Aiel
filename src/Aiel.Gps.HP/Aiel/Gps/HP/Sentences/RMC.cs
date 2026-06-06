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
/// Represents an RMC (Recommended Minimum) NMEA message.
/// </summary>
/// <remarks>
/// <para>
/// This sentence contains the minimum recommended navigation data including time, date,
/// position, course, speed, and magnetic variation. This is often called the "recommended
/// minimum" sentence as it contains essential navigation information.
/// </para>
/// <para>
/// Example sentence: $GPRMC,225446.000,A,4916.45,N,12311.12,W,0.086,54.7,191194,20.3,E,A*72
/// </para>
/// </remarks>
[NmeaMessage("GPRMC")]
public struct RMC
{
    /// <summary>UTC time of the position fix (HHMMSS.SSS format).</summary>
    public TimeOnly FixTime;

    /// <summary>Navigation receiver warning. 'A' = OK, 'V' = Warning (void).</summary>
    public Char Status;

    /// <summary>Latitude in decimal degrees. Positive = North, Negative = South.</summary>
    public Double Latitude;

    /// <summary>Longitude in decimal degrees. Positive = East, Negative = West.</summary>
    public Double Longitude;

    /// <summary>Speed over ground in knots.</summary>
    public Double SpeedOverGround;

    /// <summary>
    /// Track angle in degrees True North.
    /// The direction of travel relative to true north (0-359.9 degrees).
    /// </summary>
    public Double TrackAngle;

    /// <summary>Date of the fix (DDMMYY format).</summary>
    public DateOnly Date;

    /// <summary>
    /// Magnetic variation in degrees.
    /// The angular difference between magnetic north and true north.
    /// </summary>
    public Double MagneticVariation;

    /// <summary>Direction of magnetic variation. 'E' = East, 'W' = West.</summary>
    public Char Direction;

    /// <summary>
    /// Mode indicator for NMEA 2.3 and newer.
    /// 'A' = Autonomous, 'D' = Differential, 'E' = Estimated, 'N' = Not valid.
    /// </summary>
    public Char Mode;

    /// <summary>NMEA checksum value.</summary>
    public Int32 Checksum;

    /// <inheritdoc/>
    public override readonly String ToString() => $"{nameof(RMC)} {FixTime} {Date} {Latitude} {Longitude} SOG:{SpeedOverGround:F1}kts COG:{TrackAngle:F1}°";
}

/// <summary>
/// Parser for RMC (Recommended Minimum) NMEA sentences.
/// </summary>
/// <remarks>
/// The RMC sentence is one of the most commonly used NMEA messages as it provides
/// essential navigation data in a single sentence.
/// </remarks>
[NmeaParser(typeof(RMC))]
public readonly struct RmcParser : INmeaParser<RMC>
{
    /// <summary>
    /// Gets the NMEA sentence identifier for RMC sentences.
    /// </summary>
    public ReadOnlySpan<Byte> Identifier => "GPRMC"u8;

    /// <summary>
    /// Parses an RMC NMEA sentence into an RMC structure.
    /// </summary>
    /// <param name="lexer">The lexer positioned at the start of the sentence.</param>
    /// <param name="msg">The parsed RMC message.</param>
    /// <remarks>
    /// Expected format: $GPRMC,time,status,lat,lat_dir,lon,lon_dir,speed,course,date,mag_var,var_dir,mode*checksum
    /// </remarks>
    public void Parse(ref Lexer lexer, out RMC msg)
    {
        // Skip the sentence identifier (e.g., "GPRMC")
        lexer.ConsumeString();

        msg = new RMC()
        {
            FixTime = lexer.NextTime(),
            Status = lexer.NextChar(),
            Latitude = lexer.NextLatitude(),
            Longitude = lexer.NextLongitude(),
            SpeedOverGround = lexer.NextDouble(),
            TrackAngle = lexer.NextDouble(),
            Date = lexer.NextDate(),
            MagneticVariation = lexer.NextDouble(),
            Direction = lexer.NextChar(),
            Mode = lexer.NextChar(),
            Checksum = lexer.NextChecksum()
        };
    }
}
