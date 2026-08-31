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

using Aiel.Testing.Errors;

namespace Aiel.Results;

public sealed class ErrorCodeRegistryTests(ResultsIntegrationTestFixture fixture, ITestOutputHelper output)
    : ResultsUnitTestBase(fixture, output)
{
    [Fact]
    public void Registry_Returns_Static_Instance_When_Available()
    {
        var code = ErrorRegistry.GetErrorCodeFor<SimpleError>();

        code.Should().BeSameAs(SimpleError.SimpleErrorCode.Instance);
    }

    public sealed class NoInstanceError(String description)
        : Error(new NoInstanceErrorCode(), description)
    {
        static NoInstanceError()
        {
            ErrorRegistry.Register<NoInstanceError>();
        }

        public sealed class NoInstanceErrorCode : ErrorCode
        {
            protected override String Name => "FakeError";
        }
    }

    [Fact]
    public void Registry_Falls_Back_To_New_When_No_Static_Instance()
    {
        var code1 = ErrorRegistry.GetErrorCodeFor<NoInstanceError>();
        var code2 = ErrorRegistry.GetErrorCodeFor<NoInstanceError>();

        code1.Should().NotBeNull();
        code1.Should().BeOfType<NoInstanceError.NoInstanceErrorCode>();

        // Should be cached — same instance returned
        code2.Should().BeSameAs(code1);
    }
}
