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

using Aiel.GPS;
using System.Buffers;
using System.Text;

namespace Aiel.Gps;

public class GSA : NmeaMessage
{
    public const String Identifier = "GPGSA";

    private static readonly ReadOnlyMemory<Byte> KEY = Encoding.UTF8.GetBytes($"${Identifier}").AsMemory();

    public override String ToString() => $"GPGSA {FixMode} {FixType} {Pdop} {Hdop} {Vdop}";

    public override NmeaMessage Parse(ReadOnlySequence<Byte> sentence)
    {
        var lexer = LexerFactory.Create(sentence);

        // Skip over GPGSA
        lexer.SkipString();

        // $GPGSA,A,3,04,05,,09,12,,,24,,,,,2.5,1.3,2.1*39
        var gsa = new GSA()
        {
            FixMode = lexer.NextChar(),
            FixType = (FixType)lexer.NextInteger(),
            SV = new Int32[12]
        };

        for (var i = 0; i < 12; i++)
        {
            gsa.SV[i] = lexer.NextInteger();
        }

        gsa.Pdop = lexer.NextDouble();
        gsa.Hdop = lexer.NextDouble();
        gsa.Vdop = lexer.NextDouble();
        gsa.Checksum = lexer.NextChecksum();

        return gsa;
    }

    protected override ReadOnlyMemory<Byte> Key => KEY;
    public Char FixMode { get; private set; }
    public FixType FixType { get; private set; }
    public Int32[] SV { get; private set; } = [];
    public Double Pdop { get; private set; }
    public Double Hdop { get; private set; }
    public Double Vdop { get; private set; }
}
