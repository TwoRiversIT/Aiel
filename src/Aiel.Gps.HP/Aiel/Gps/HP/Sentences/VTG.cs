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
/// Represents a VTG (Track Made Good and Ground Speed) NMEA message.
/// </summary>
/// <remarks>
/// <para>
/// This sentence provides velocity information including course and speed over ground.
/// The course is provided relative to both true north and magnetic north,
/// and speed is provided in both knots and kilometers per hour.
/// </para>
/// <para>
/// Example sentence: $GPVTG,054.7,T,034.4,M,005.5,N,010.2,K,A*48
/// </para>
/// </remarks>
[NmeaMessage("GPVTG")]
public struct VTG
{
    /// <summary>Track angle in degrees relative to true north (0-359.9).</summary>
    public Double TrueTrack;

    /// <summary>True track indicator (always 'T' for true).</summary>
    public Char TrueTrackIndicator;

    /// <summary>Track angle in degrees relative to magnetic north (0-359.9).</summary>
    public Double MagneticTrack;

    /// <summary>Magnetic track indicator (always 'M' for magnetic).</summary>
    public Char MagneticTrackIndicator;

    /// <summary>Ground speed in knots.</summary>
    public Double GroundSpeedN;

    /// <summary>Ground speed units indicator (always 'N' for knots).</summary>
    public Char GroundSpeedNIndicator;

    /// <summary>Ground speed in kilometers per hour.</summary>
    public Double GroundSpeedK;

    /// <summary>Ground speed units indicator (always 'K' for kilometers per hour).</summary>
    public Char GroundSpeedKIndicator;

    /// <summary>
    /// Mode indicator for NMEA 2.3 and newer.
    /// 'A' = Autonomous, 'D' = Differential, 'E' = Estimated, 'N' = Not valid.
    /// </summary>
    public Char ModeIndicator;

    /// <summary>NMEA checksum value.</summary>
    public Int32 Checksum;

    /// <inheritdoc/>
    public override readonly String ToString() => $"{nameof(VTG)} True:{TrueTrack:F1}° Mag:{MagneticTrack:F1}° Speed:{GroundSpeedN:F1}kts/{GroundSpeedK:F1}km/h";
}

/// <summary>
/// Parser for VTG (Track Made Good and Ground Speed) NMEA sentences.
/// </summary>
/// <remarks>
/// The VTG sentence provides essential navigation information including course and speed,
/// which are critical for dead reckoning and navigation calculations.
/// </remarks>
[NmeaParser(typeof(VTG))]
public readonly struct VtgParser : INmeaParser<VTG>
{
    /// <summary>
    /// Gets the NMEA sentence identifier for VTG sentences.
    /// </summary>
    public ReadOnlySpan<Byte> Identifier => "GPVTG"u8;

    /// <summary>
    /// Parses a VTG NMEA sentence into a VTG structure.
    /// </summary>
    /// <param name="lexer">The lexer positioned at the start of the sentence.</param>
    /// <param name="msg">The parsed VTG message.</param>
    /// <remarks>
    /// Expected format: $GPVTG,true_track,T,mag_track,M,speed_n,N,speed_k,K,mode*checksum
    /// </remarks>
    public void Parse(ref Lexer lexer, out VTG msg)
    {
        // Skip the sentence identifier (e.g., "GPVTG")
        lexer.ConsumeString();

        msg = new VTG()
        {
            TrueTrack = lexer.NextDouble(),
            TrueTrackIndicator = lexer.NextChar(),
            MagneticTrack = lexer.NextDouble(),
            MagneticTrackIndicator = lexer.NextChar(),
            GroundSpeedN = lexer.NextDouble(),
            GroundSpeedNIndicator = lexer.NextChar(),
            GroundSpeedK = lexer.NextDouble(),
            GroundSpeedKIndicator = lexer.NextChar(),
            ModeIndicator = lexer.NextChar(),
            Checksum = lexer.NextChecksum()
        };
    }
}
