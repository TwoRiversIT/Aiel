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

namespace Aiel.Results;

/// <summary>
/// Unit tests for <see cref="Maybe{T}"/>.
/// </summary>
public class MaybeTests
{
    [Fact]
    public void Default_Should_BeNone()
    {
        // Act
        Maybe<TestRecord> maybe = default;

        // Assert
        maybe.HasValue.Should().BeFalse();
        maybe.IsNone.Should().BeTrue();
        maybe.Should().Be(Maybe<TestRecord>.None);
    }

    [Fact]
    public void None_Should_NotHaveValue()
    {
        // Act
        var maybe = Maybe<TestRecord>.None;

        // Assert
        maybe.HasValue.Should().BeFalse();
        maybe.IsNone.Should().BeTrue();
    }

    [Fact]
    public void Some_Should_HoldTheValue()
    {
        // Arrange
        var record = new TestRecord(42, "Bart Simpson", "bart@thesimpsons.com");

        // Act
        var maybe = Maybe<TestRecord>.Some(record);

        // Assert
        maybe.HasValue.Should().BeTrue();
        maybe.IsNone.Should().BeFalse();
        maybe.Value.Should().Be(record);
    }

    [Fact]
    public void Some_Should_ThrowArgumentNullException_When_ValueIsNull()
    {
        // Act
        Action act = () => Maybe<TestRecord>.Some(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Value_Should_ThrowInvalidOperationException_When_None()
    {
        // Arrange
        var maybe = Maybe<TestRecord>.None;

        // Act
        Action act = () => _ = maybe.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*has no value*");
    }

    [Fact]
    public void TryGetValue_Should_ReturnTrueAndValue_When_Some()
    {
        // Arrange
        var record = new TestRecord(42, "Bart Simpson", "bart@thesimpsons.com");
        var maybe = Maybe<TestRecord>.Some(record);

        // Act
        var got = maybe.TryGetValue(out var value);

        // Assert
        got.Should().BeTrue();
        value.Should().Be(record);
    }

    [Fact]
    public void TryGetValue_Should_ReturnFalse_When_None()
    {
        // Arrange
        var maybe = Maybe<TestRecord>.None;

        // Act
        var got = maybe.TryGetValue(out var value);

        // Assert
        got.Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void GetValueOrDefault_Should_ReturnValue_When_Some()
    {
        // Arrange
        var record = new TestRecord(42, "Bart Simpson", "bart@thesimpsons.com");
        var maybe = Maybe<TestRecord>.Some(record);

        // Act & Assert
        maybe.GetValueOrDefault(TestRecord.Empty).Should().Be(record);
    }

    [Fact]
    public void GetValueOrDefault_Should_ReturnFallback_When_None()
    {
        // Arrange
        var maybe = Maybe<TestRecord>.None;

        // Act & Assert
        maybe.GetValueOrDefault(TestRecord.Empty).Should().Be(TestRecord.Empty);
    }

    [Fact]
    public void GetValueOrDefault_Should_ThrowArgumentNullException_When_FallbackIsNull()
    {
        // Arrange
        var maybe = Maybe<TestRecord>.None;

        // Act
        Action act = () => maybe.GetValueOrDefault(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void FromNullable_Should_ReturnNone_When_Null()
    {
        // Act
        var maybe = Maybe<TestRecord>.FromNullable(null);

        // Assert
        maybe.IsNone.Should().BeTrue();
    }

    [Fact]
    public void FromNullable_Should_ReturnSome_When_NotNull()
    {
        // Arrange
        var record = new TestRecord(42, "Bart Simpson", "bart@thesimpsons.com");

        // Act
        var maybe = Maybe<TestRecord>.FromNullable(record);

        // Assert
        maybe.HasValue.Should().BeTrue();
        maybe.Value.Should().Be(record);
    }

    [Fact]
    public void ImplicitConversion_Should_ProduceSome()
    {
        // Arrange
        var record = new TestRecord(42, "Bart Simpson", "bart@thesimpsons.com");

        // Act
        Maybe<TestRecord> maybe = record;

        // Assert
        maybe.HasValue.Should().BeTrue();
        maybe.Value.Should().Be(record);
    }

    [Fact]
    public void Equality_Should_BeValueBased()
    {
        // Arrange
        var record = new TestRecord(42, "Bart Simpson", "bart@thesimpsons.com");
        var other = new TestRecord(7, "Lisa Simpson", "lisa@thesimpsons.com");

        // Act & Assert
        Maybe<TestRecord>.Some(record).Should().Be(Maybe<TestRecord>.Some(record));
        Maybe<TestRecord>.Some(record).Should().NotBe(Maybe<TestRecord>.Some(other));
        Maybe<TestRecord>.Some(record).Should().NotBe(Maybe<TestRecord>.None);
        Maybe<TestRecord>.None.Should().Be(Maybe<TestRecord>.None);
    }

    [Fact]
    public void ToString_Should_DistinguishSomeFromNone()
    {
        // Act & Assert
        Maybe<Int32>.None.ToString().Should().Be("None");
        Maybe<Int32>.Some(42).ToString().Should().Be("Some(42)");
    }

    /// <summary>
    /// The regression this type exists to prevent: a value type whose value happens to equal
    /// <see langword="default"/> is still a present value, and must not read as absent.
    /// </summary>
    [Fact]
    public void Some_Should_BeDistinctFromNone_When_ValueEqualsDefault()
    {
        // Act
        var zero = Maybe<Int32>.Some(0);
        var none = Maybe<Int32>.None;

        // Assert
        zero.HasValue.Should().BeTrue();
        zero.Value.Should().Be(0);
        zero.GetValueOrDefault(99).Should().Be(0);
        zero.Should().NotBe(none);

        none.HasValue.Should().BeFalse();
        none.GetValueOrDefault(99).Should().Be(99);
    }

    [Fact]
    public void Default_Should_BeNone_ForValueTypes()
    {
        // Act
        Maybe<Int32> maybe = default;

        // Assert
        maybe.IsNone.Should().BeTrue("default(Maybe<Int32>) must be None, not Some(0)");
    }
}
