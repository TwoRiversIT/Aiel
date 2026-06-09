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

using System.Runtime.InteropServices;

namespace Aiel.Gps.HP.Sentences;

/// <summary>
/// Represents a GSA (GPS DOP and Active Satellites) NMEA message.
/// </summary>
/// <remarks>
/// <para>
/// This sentence provides information about the fix mode, fix type, satellite IDs used for the fix,
/// and dilution of precision (DOP) values. DOP values are measures of positioning accuracy.
/// </para>
/// <para>
/// Example sentence: $GPGSA,A,3,04,05,,09,12,,,24,,,,,2.5,1.3,2.1*39
/// </para>
/// </remarks>
[NmeaMessage("GPGSA")]
public struct GSA
{
    /// <summary>Fix mode selection. 'A' = Automatic (allowed to switch 2D/3D), 'M' = Manual (forced to operate in 2D or 3D).</summary>
    public Char FixMode;

    /// <summary>Fix type. 1 = No fix, 2 = 2D fix, 3 = 3D fix.</summary>
    public FixType FixType;

    /// <summary>
    /// Satellite IDs used for the position fix.
    /// Up to 12 satellite IDs are possible. Unused slots contain 0.
    /// </summary>
    public SatelliteArray Satellites;

    /// <summary>
    /// Position Dilution of Precision (PDOP).
    /// A measure of the geometric quality of the satellite constellation.
    /// Lower values indicate better position precision.
    /// </summary>
    public Double Pdop;

    /// <summary>
    /// Horizontal Dilution of Precision (HDOP).
    /// A measure of the precision of horizontal position (latitude/longitude).
    /// Values under 2.0 are considered excellent.
    /// </summary>
    public Double Hdop;

    /// <summary>
    /// Vertical Dilution of Precision (VDOP).
    /// A measure of the precision of vertical position (altitude).
    /// Values under 2.0 are considered excellent.
    /// </summary>
    public Double Vdop;

    /// <summary>NMEA checksum value.</summary>
    public Int32 Checksum;

    /// <inheritdoc/>
    public override readonly String ToString() => $"{nameof(GSA)} Mode:{FixMode} Type:{FixType} PDOP:{Pdop:F1} HDOP:{Hdop:F1} VDOP:{Vdop:F1}";
}

/// <summary>
/// Fixed-size array for storing up to 12 satellite IDs in GSA messages.
/// This avoids heap allocation while providing array-like access.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct SatelliteArray
{
    private Int32 _satellite1;
    private Int32 _satellite2;
    private Int32 _satellite3;
    private Int32 _satellite4;
    private Int32 _satellite5;
    private Int32 _satellite6;
    private Int32 _satellite7;
    private Int32 _satellite8;
    private Int32 _satellite9;
    private Int32 _satellite10;
    private Int32 _satellite11;
    private Int32 _satellite12;

    /// <summary>
    /// Gets or sets the satellite ID at the specified index.
    /// </summary>
    /// <param name="index">Index from 0 to 11.</param>
    /// <returns>The satellite ID (PRN), or 0 if not used.</returns>
    public readonly Int32 this[Int32 index] => index switch
    {
        0 => _satellite1,
        1 => _satellite2,
        2 => _satellite3,
        3 => _satellite4,
        4 => _satellite5,
        5 => _satellite6,
        6 => _satellite7,
        7 => _satellite8,
        8 => _satellite9,
        9 => _satellite10,
        10 => _satellite11,
        11 => _satellite12,
        _ => throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be between 0 and 11")
    };

    /// <summary>
    /// Sets the satellite ID at the specified index.
    /// </summary>
    /// <param name="index">Index from 0 to 11.</param>
    /// <param name="value">The satellite ID (PRN).</param>
    public void Set(Int32 index, Int32 value)
    {
        switch (index)
        {
            case 0:
                _satellite1 = value;
                break;
            case 1:
                _satellite2 = value;
                break;
            case 2:
                _satellite3 = value;
                break;
            case 3:
                _satellite4 = value;
                break;
            case 4:
                _satellite5 = value;
                break;
            case 5:
                _satellite6 = value;
                break;
            case 6:
                _satellite7 = value;
                break;
            case 7:
                _satellite8 = value;
                break;
            case 8:
                _satellite9 = value;
                break;
            case 9:
                _satellite10 = value;
                break;
            case 10:
                _satellite11 = value;
                break;
            case 11:
                _satellite12 = value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(index), index, "Index must be between 0 and 11");
        }
    }

    /// <summary>Gets the count of active satellites (non-zero IDs).</summary>
    public readonly Int32 Count
    {
        get
        {
            var count = 0;
            for (var i = 0; i < 12; i++)
            {
                if (this[i] != 0)
                {
                    count++;
                }
            }

            return count;
        }
    }
}

/// <summary>
/// Parser for GSA (GPS DOP and Active Satellites) NMEA sentences.
/// </summary>
/// <remarks>
/// The GSA sentence provides important quality metrics for GPS positioning through
/// the dilution of precision values.
/// </remarks>
[NmeaParser(typeof(GSA))]
public readonly struct GsaParser : INmeaParser<GSA>
{
    /// <summary>
    /// Gets the NMEA sentence identifier for GSA sentences.
    /// </summary>
    public ReadOnlySpan<Byte> Identifier => "GPGSA"u8;

    /// <summary>
    /// Parses a GSA NMEA sentence into a GSA structure.
    /// </summary>
    /// <param name="lexer">The lexer positioned at the start of the sentence.</param>
    /// <param name="msg">The parsed GSA message.</param>
    /// <remarks>
    /// Expected format: $GPGSA,mode,type,prn1,prn2,...,prn12,pdop,hdop,vdop*checksum
    /// </remarks>
    public void Parse(ref Lexer lexer, out GSA msg)
    {
        // Skip the sentence identifier (e.g., "GPGSA")
        lexer.ConsumeString();

        msg = new GSA()
        {
            FixMode = lexer.NextChar(),
            FixType = (FixType)lexer.NextInteger()
        };

        // Parse the 12 satellite IDs
        for (var i = 0; i < 12; i++)
        {
            msg.Satellites.Set(i, lexer.NextInteger());
        }

        msg.Pdop = lexer.NextDouble();
        msg.Hdop = lexer.NextDouble();
        msg.Vdop = lexer.NextDouble();
        msg.Checksum = lexer.NextChecksum();
    }
}
