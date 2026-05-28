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

using Aiel.Results.TestErrors;

namespace Aiel.Results;

/// <summary>
/// Unit tests for the <see cref="ErrorCode"/> class and its implementations.
/// </summary>
public class ErrorCodeTests(ResultsIntegrationTestFixture fixture, ITestOutputHelper output)
    : ResultsUnitTestBase(fixture, output)
{
    public sealed class ErrorCodeAlpha : ErrorCode
    {
        public static readonly ErrorCodeAlpha Instance = new();
        protected override String Name => "Alpha";
    }

    public sealed class ErrorCodeBeta : ErrorCode
    {
        public static readonly ErrorCodeBeta Instance = new();
        protected override String Name => "Beta";
    }

    public sealed class ErrorCodeGamma : ErrorCode
    {
        public static readonly ErrorCodeGamma Instance = new();
        protected override String Name => "Alpha";
    }

    [Fact]
    public void ToString_ShouldReturnName()
    {
        var errorCode = SimpleError.SimpleErrorCode.Instance;

        var result = errorCode.ToString();

        result.Should().Be(nameof(SimpleError), "ToString should return the Name property");
    }

    [Fact]
    public void ImplicitOperatorToString_ShouldReturnName()
    {
        var errorCode = SimpleError.SimpleErrorCode.Instance;

        String result = errorCode;

        result.Should().Be(nameof(SimpleError));
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        var errorCode = SimpleError.SimpleErrorCode.Instance;

        var result = errorCode.Equals(null);

        result.Should().BeFalse();
    }

    [Fact]
    public void Equals_WithSameInstance_ShouldReturnTrue()
    {
        var errorCode = SimpleError.SimpleErrorCode.Instance;

        var result = errorCode.Equals(errorCode);

        result.Should().BeTrue("an instance should equal itself");
    }

    [Fact]
    public void Equals_WithDifferentInstance_ShouldReturnFalse()
    {
        var alpha = ErrorCodeAlpha.Instance;
        var beta = ErrorCodeBeta.Instance;

        var result = alpha.Equals(beta);

        result.Should().BeFalse("different types with different names are not equal");
    }

    [Fact]
    public void Equals_WithDifferentTypeButSameName_ShouldReturnFalse()
    {
        var alpha = ErrorCodeAlpha.Instance;
        var gamma = ErrorCodeGamma.Instance;

        var result = alpha.Equals(gamma);

        result.Should().BeFalse("different types are not equal even if names match");
    }

    [Fact]
    public void GetHashCode_ShouldBeConsistent()
    {
        var errorCode = ErrorCodeAlpha.Instance;

        var hash1 = errorCode.GetHashCode();
        var hash2 = errorCode.GetHashCode();

        hash1.Should().Be(hash2, "hash code should be consistent");
    }

    [Fact]
    public void GetHashCode_ForDifferentInstances_ShouldBeDifferent()
    {
        var alpha = ErrorCodeAlpha.Instance;
        var beta = ErrorCodeBeta.Instance;

        var hash1 = alpha.GetHashCode();
        var hash2 = beta.GetHashCode();

        hash1.Should().NotBe(hash2, "different types should have different hash codes");
    }

    [Fact]
    public void Singletons_ShouldBeReferenceSame()
    {
        var instance1 = ErrorCodeAlpha.Instance;
        var instance2 = ErrorCodeAlpha.Instance;

        instance1.Should().BeSameAs(instance2, "singleton instances should be the same reference");
    }

    [Fact]
    public void ErrorCodeSingletons_ShouldBeUnique()
    {
        var errors = new Error[]
        {
            new SimpleError("test"),
            new DatabaseConnectionError("test")
        };

        var errorCodes = errors.Select(e => e.ErrorCode).ToList();
        var distinctCodes = errorCodes.Distinct().ToList();

        distinctCodes.Should().HaveCount(errorCodes.Count, "all error codes should have unique type+name combinations");
    }
}
