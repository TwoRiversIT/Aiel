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

using System.Diagnostics.CodeAnalysis;

namespace Aiel.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("", 0, "")]
    [InlineData("", 5, "")]
    [InlineData("  ", 0, "")]
    [InlineData("  ", 5, "")]
    [InlineData("Hello", 0, "")]
    [InlineData("Hello", 1, "H")]
    [InlineData("Hello", 5, "Hello")]
    [InlineData("Hello", 6, "Hello")]
    public void Left_should_return_the_correct_result(String input, Int32 length, String expected)
       => input.Left(length).Should().Be(expected);

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData(" ", " ")]
    [InlineData("  ", "  ")]
    [InlineData("Hello", "Hello")]
    [InlineData("hello", "Hello")]
    [InlineData("HelloWorld", "Hello World")]
    [InlineData("helloWorld", "Hello World")]
    [InlineData("AB", "AB")]
    [InlineData("ABC", "ABC")]
    [InlineData("IPAddress", "IP Address")]
    [SuppressMessage("Usage", "xUnit1012:Null should only be used for nullable parameters", Justification = "<Pending>")]
    public void ToProperCase_should_return_the_correct_result(String input, String expected)
       => input.ToProperCase().Should().Be(expected);
}
