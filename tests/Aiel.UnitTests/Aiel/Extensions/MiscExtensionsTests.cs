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

namespace Aiel.Extensions;

public class MiscExtensionsTests
{
    [Fact]
    public void Visit_ExecutesActionOnException()
    {
        var exception = new InvalidOperationException("Test exception");
        var visited = false;

        exception.Visit(ex =>
        {
            Assert.Equal("Test exception", ex.Message);
            visited = true;
        });

        Assert.True(visited);
    }

    [Fact]
    public void Visit_ExecutesActionOnAllInnerExceptions()
    {
        var innerMost = new ArgumentException("Inner most");
        var middle = new InvalidOperationException("Middle", innerMost);
        var outer = new Exception("Outer", middle);

        var messages = new List<String>();

        outer.Visit(ex => messages.Add(ex.Message));

        Assert.Equal(3, messages.Count);
        Assert.Contains("Outer", messages);
        Assert.Contains("Middle", messages);
        Assert.Contains("Inner most", messages);
    }

    [Fact]
    public void Visit_VisitsInCorrectOrder()
    {
        var inner = new ArgumentException("Inner");
        var outer = new InvalidOperationException("Outer", inner);

        var messages = new List<String>();

        outer.Visit(ex => messages.Add(ex.Message));

        Assert.Equal("Outer", messages[0]);
        Assert.Equal("Inner", messages[1]);
    }

    [Fact]
    public void Clamp_ReturnsValue_WhenWithinRange()
    {
        var result = 5.Clamp(1, 10);

        Assert.Equal(5, result);
    }

    [Fact]
    public void Clamp_ReturnsMin_WhenBelowRange()
    {
        var result = 0.Clamp(1, 10);

        Assert.Equal(1, result);
    }

    [Fact]
    public void Clamp_ReturnsMax_WhenAboveRange()
    {
        var result = 15.Clamp(1, 10);

        Assert.Equal(10, result);
    }

    [Fact]
    public void Clamp_WorksWithDouble()
    {
        var result = 5.5.Clamp(1.0, 10.0);

        Assert.Equal(5.5, result);
    }

    [Fact]
    public void Clamp_WorksWithDateTime()
    {
        var min = new DateTime(2020, 1, 1);
        var max = new DateTime(2025, 12, 31);
        var value = new DateTime(2023, 6, 15);

        var result = value.Clamp(min, max);

        Assert.Equal(value, result);
    }

    [Fact]
    public void Clamp_HandlesEdgeCases_AtMin()
    {
        var result = 1.Clamp(1, 10);

        Assert.Equal(1, result);
    }

    [Fact]
    public void Clamp_HandlesEdgeCases_AtMax()
    {
        var result = 10.Clamp(1, 10);

        Assert.Equal(10, result);
    }
}
