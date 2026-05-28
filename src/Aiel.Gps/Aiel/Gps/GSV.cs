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

public class GSV : NmeaMessage
{
    public const String Identifier = "GPGSV";

    private static readonly ReadOnlyMemory<Byte> KEY = Encoding.UTF8.GetBytes($"${Identifier}").AsMemory();
    protected override ReadOnlyMemory<Byte> Key => KEY;

    public override NmeaMessage Parse(ReadOnlySequence<Byte> sentence)
    {
        var lexer = LexerFactory.Create(sentence);

        // Skip over GPGSV
        lexer.SkipString();

        var gsv = new GSV()
        {
            TotalMessages = lexer.NextInteger(),
            MessageNumber = lexer.NextInteger(),
            SatellitesInView = lexer.NextInteger(),
            SV1 = SV.Create(lexer.NextInteger(), lexer.NextInteger(), lexer.NextInteger(), lexer.NextInteger())
        };

        if (!lexer.EOL)
        {
            gsv.SV2 = SV.Create(lexer.NextInteger(), lexer.NextInteger(), lexer.NextInteger(), lexer.NextInteger());
        }

        if (!lexer.EOL)
        {
            gsv.SV3 = SV.Create(lexer.NextInteger(), lexer.NextInteger(), lexer.NextInteger(), lexer.NextInteger());
        }

        if (!lexer.EOL)
        {
            gsv.SV4 = SV.Create(lexer.NextInteger(), lexer.NextInteger(), lexer.NextInteger(), lexer.NextInteger());
        }

        gsv.Checksum = lexer.NextChecksum();
        return gsv;
    }

    public Int32 TotalMessages { get; private set; }
    public Int32 MessageNumber { get; private set; }
    public Int32 SatellitesInView { get; private set; }
    public SV SV1 { get; private set; } = new SV();
    public SV? SV2 { get; private set; }
    public SV? SV3 { get; private set; }
    public SV? SV4 { get; private set; }

    public override String ToString() => $"GPGSV {TotalMessages} {MessageNumber} {SatellitesInView} {SV1} {SV2} {SV3}";

    public class SV
    {
        public Int32 PRN { get; private set; }
        public Int32 Elevation { get; private set; }
        public Int32 Azimuth { get; private set; }
        public Int32 SNR { get; private set; }

        public override String ToString() => $"SV {PRN} {Elevation} {Azimuth} {SNR}";

        internal static SV Create(Int32 prn, Int32 elevation, Int32 azimuth, Int32 snr)
        {
            return new SV()
            {
                PRN = prn,
                Elevation = elevation,
                Azimuth = azimuth,
                SNR = snr
            };
        }
    }
}
