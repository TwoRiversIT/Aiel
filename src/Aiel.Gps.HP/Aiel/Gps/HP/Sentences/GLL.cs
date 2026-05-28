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
/// Represents a GLL (Geographic Position - Latitude/Longitude) NMEA message.
/// </summary>
[NmeaMessage("GPGLL")]
public struct GLL
{
    /// <summary>Latitude in decimal degrees. Positive = North, Negative = South.</summary>
    public Double Latitude;

    /// <summary>Longitude in decimal degrees. Positive = East, Negative = West.</summary>
    public Double Longitude;

    /// <summary>UTC time of the position fix.</summary>
    public TimeOnly FixTime;

    /// <summary>Data status: 'A' = Active/Valid, 'V' = Void/Invalid.</summary>
    public Char DataActive;

    /// <summary>NMEA checksum value.</summary>
    public Int32 Checksum;

    /// <inheritdoc/>
    public override readonly String ToString() => $"{nameof(GLL)} {Latitude} {Longitude} {FixTime} {DataActive}";
}

/// <summary>
/// Parser for GLL (Geographic Position - Latitude/Longitude) NMEA sentences.
/// </summary>
[NmeaParser(typeof(GLL))]
public readonly struct GllParser : INmeaParser<GLL>
{
    /// <summary>
    /// Gets the NMEA sentence identifier for GLL sentences.
    /// </summary>
    public ReadOnlySpan<Byte> Identifier => "GPGLL"u8;

    /// <summary>
    /// Parses a GLL NMEA sentence into a GLL structure.
    /// </summary>
    /// <param name="lexer">The lexer positioned at the start of the sentence.</param>
    /// <param name="msg">The parsed GLL message.</param>
    public void Parse(ref Lexer lexer, out GLL msg)
    {
        // Skip the sentence identifier (e.g., "GPGLL")
        lexer.ConsumeString();

        msg = new GLL()
        {
            Latitude = lexer.NextLatitude(),
            Longitude = lexer.NextLongitude(),
            FixTime = lexer.NextTime(),
            DataActive = lexer.NextChar()
        };
    }
}
