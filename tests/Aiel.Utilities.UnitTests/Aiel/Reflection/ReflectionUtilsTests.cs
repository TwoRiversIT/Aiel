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

namespace Aiel.Reflection;

public class ReflectionUtilsTests
{
    public static class TestConstants
    {
        public const String Value1 = "Constant1";
        public const String Value2 = "Constant2";
        public const Int32 Number = 42;

        public static class Nested
        {
            public const String Value3 = "NestedConstant";
            public const String Value4 = "AnotherNested";

            public static class DoublyNested
            {
                public const String Value5 = "DoublyNestedConstant";
            }
        }
    }

    [Fact]
    public void GetConstants_ReturnsTopLevelConstants()
    {
        var constants = typeof(TestConstants).GetConstants();

        Assert.Contains("Constant1", constants);
        Assert.Contains("Constant2", constants);
        Assert.Contains("42", constants);
    }

    [Fact]
    public void GetConstants_ReturnsNestedConstants()
    {
        var constants = typeof(TestConstants).GetConstants();

        Assert.Contains("NestedConstant", constants);
        Assert.Contains("AnotherNested", constants);
    }

    [Fact]
    public void GetConstants_ReturnsDoublyNestedConstants()
    {
        var constants = typeof(TestConstants).GetConstants();

        Assert.Contains("DoublyNestedConstant", constants);
    }

    [Fact]
    public void GetConstants_ReturnsAllConstants()
    {
        var constants = typeof(TestConstants).GetConstants();

        Assert.Equal(6, constants.Length);
    }

    public static class EmptyClass;

    [Fact]
    public void GetConstants_ReturnsEmptyArray_WhenNoConstants()
    {
        var constants = typeof(EmptyClass).GetConstants();

        Assert.Empty(constants);
    }

    public static class MixedMembers
    {
        public const String Constant = "Const";
        public static readonly String ReadOnly = "ReadOnly";
        public static String Property => "Property";
    }

    [Fact]
    public void GetConstants_OnlyReturnsConstants_NotReadOnlyOrProperties()
    {
        var constants = typeof(MixedMembers).GetConstants();

        Assert.Single(constants);
        Assert.Contains("Const", constants);
        Assert.DoesNotContain("ReadOnly", constants);
        Assert.DoesNotContain("Property", constants);
    }
}
