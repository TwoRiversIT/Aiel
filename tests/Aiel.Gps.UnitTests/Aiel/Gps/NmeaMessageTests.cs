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

public class NmeaMessageTests
{
    [Fact]
    public void When_calling_CanHandle_short_lines_do_not_cause_exceptions()
    {
        var bytes = Encoding.UTF8.GetBytes("$NMEA");
        var buffer = new ReadOnlySequence<Byte>(bytes);
        var parser = new MyNmeaMessage();

        parser.CanHandle(buffer).Should().BeFalse();
    }

    public class MyNmeaMessage : NmeaMessage
    {
        private static readonly ReadOnlyMemory<Byte> DefaultKey = Encoding.UTF8.GetBytes("$NMEAG").AsMemory();

        protected override ReadOnlyMemory<Byte> Key => DefaultKey;

        public override NmeaMessage Parse(ReadOnlySequence<Byte> sentence)
        {
            throw new NotImplementedException();
        }
    }
}
