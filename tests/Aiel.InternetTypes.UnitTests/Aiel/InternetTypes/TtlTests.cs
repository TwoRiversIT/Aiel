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

using Aiel.Internet;

namespace Aiel.InternetTypes;

public class TtlTests
{
    [Fact]
    public void Must_allow_zero()
    {
        Int32 i = new TTL(0);
        i.Should().Be(0);
    }

    [Fact]
    public void Must_be_constructable_from_Int32()
    {
        Int32 a = new TTL(3600);
        a.Should().Be(3600);
    }

    [Fact]
    public void Must_be_assignable_from_Int32()
    {
        TTL a = 1200;
        Int32 b = a;
        b.Should().Be(1200);
    }

    [Fact]
    public void Must_be_assignable_to_Int32()
    {
        Int32 i = new TTL(1200);
        i.Should().Be(1200);
    }

    [Fact]
    public void Must_be_assignable_to_String()
    {
        String s = new TTL(1200);
        s.Should().Be("1200");
    }

    [Fact]
    public void Must_be_Equatable()
    {
        var a = new TTL(1200);
        var b = new TTL(1200);
        var c = new TTL(3600);

        a.Equals(a).Should().BeTrue();
        a.Equals(b).Should().BeTrue();
        b.Equals(a).Should().BeTrue();
        a.Equals(c).Should().BeFalse();
        c.Equals(a).Should().BeFalse();
    }

    [Fact]
    public void Must_be_Comparable()
    {
        var a = new TTL(1200);
        var b = new TTL(2400);
        var c = new TTL(3600);

        b.CompareTo(a).Should().Be(1);
        b.CompareTo(b).Should().Be(0);
        b.CompareTo(c).Should().Be(-1);
    }

    [Fact]
    public void Must_implement_comparison_operators()
    {
        var a = new TTL(1200);
        var b = new TTL(1200);

        (a == b).Should().BeTrue();
        (b == a).Should().BeTrue();

        (a != b).Should().BeFalse();
        (b != a).Should().BeFalse();

        (a >= b).Should().BeTrue();
        (b >= a).Should().BeTrue();

        (a <= b).Should().BeTrue();
        (b <= a).Should().BeTrue();

        (a > b).Should().BeFalse();
        (b > a).Should().BeFalse();

        (a < b).Should().BeFalse();
        (b < a).Should().BeFalse();

        b = new TTL(2400);

        (a == b).Should().BeFalse();
        (b == a).Should().BeFalse();

        (a != b).Should().BeTrue();
        (b != a).Should().BeTrue();

        (a >= b).Should().BeFalse();
        (b >= a).Should().BeTrue();

        (a <= b).Should().BeTrue();
        (b <= a).Should().BeFalse();

        (a > b).Should().BeFalse();
        (b > a).Should().BeTrue();

        (a < b).Should().BeTrue();
        (b < a).Should().BeFalse();
    }

    [Fact]
    public void Must_not_allow_negative_values()
    {
        var a = Record.Exception(() => new TTL(-1));
        a.Should().BeOfType<ArgumentOutOfRangeException>();

        var b = Record.Exception(() => { TTL a = -1; });
        b.Should().BeOfType<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Must_implement_ToString()
    {
        new TTL(0).ToString().Should().Be("0");
        new TTL(1).ToString().Should().Be("1");
        new TTL(60).ToString().Should().Be("60");
        new TTL(3600).ToString().Should().Be("3600");
        new TTL(86400).ToString().Should().Be("86400");
        new TTL(604800).ToString().Should().Be("604800");
        new TTL(694861).ToString().Should().Be("694861");
    }

    [Fact]
    public void Must_output_Human_Readable_strings()
    {
        new TTL(0).HumanReadable().Should().Be("0");
        new TTL(1).HumanReadable().Should().Be("1s");
        new TTL(60).HumanReadable().Should().Be("1m");
        new TTL(3600).HumanReadable().Should().Be("1h");
        new TTL(86400).HumanReadable().Should().Be("1d");
        new TTL(604800).HumanReadable().Should().Be("1w");
        new TTL(694861).HumanReadable().Should().Be("1w1d1h1m1s");
    }
}
