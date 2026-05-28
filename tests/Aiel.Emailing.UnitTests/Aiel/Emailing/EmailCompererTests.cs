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

namespace Aiel.Emailing;

public class EmailComparerTests
{
    [Fact]
    public void When_both_Emails_are_Null_Compare_should_return_Zero()
    {
        // Arrange
        var comparer = new EmailComparer(EmailComparerMode.LocalDomain);

        // Act
        var result = comparer.Compare((Email)null!, (Email)null!);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void When_both_Emails_are_Null_Nullable_Compare_should_return_Zero()
    {
        // Arrange
        var comparer = new EmailComparer(EmailComparerMode.LocalDomain);
        Email? email1 = null;
        Email? email2 = null;

        // Act
        var result = comparer.Compare(email1, email2);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void When_first_Email_is_Null_Compare_should_return_NegativeValue()
    {
        // Arrange
        var comparer = new EmailComparer(EmailComparerMode.LocalDomain);
        Email? email = null;
        var bob = new Email("bob@example.org");

        // Act
        var result = comparer.Compare(email, bob);

        // Assert
        result.Should().BeLessThan(0);
    }

    [Fact]
    public void When_second_Email_isNull_Compare_should_return_PositiveValue()
    {
        // Arrange
        var comparer = new EmailComparer(EmailComparerMode.LocalDomain);
        var alice = new Email("alice@example.org");

        // Act
        var result = comparer.Compare(alice, null);

        // Assert
        result.Should().BeGreaterThan(0);
    }

    public class When_Mode_is_LocalDomain
    {
        [Fact]
        public void Compare_should_return_correct_comparison()
        {
            var comparer = new EmailComparer(EmailComparerMode.LocalDomain);
            var alice = new Email("alice@BOB.org");
            var bob = new Email("bob@alice.org");

            comparer.Compare(alice, bob).Should().BeLessThan(0);
            comparer.Compare(bob, alice).Should().BeGreaterThan(0);
        }

        [Fact]
        public void And_Emails_are_equal_Compare_should_return_Zero()
        {
            // Arrange
            var comparer = new EmailComparer(EmailComparerMode.LocalDomain);
            var alice1 = new Email("alice@BOB.org");
            var alice2 = new Email("alice@bob.org");

            // Act
            var result = comparer.Compare(alice1, alice2);

            // Assert
            result.Should().Be(0);
        }
    }

    public class When_Mode_is_DomainLocal
    {
        [Fact]
        public void Compare_should_return_correct_comparison()
        {
            var comparer = new EmailComparer(EmailComparerMode.DomainLocal);
            var alice = new Email("alice@bob.org");
            var bob = new Email("bob@ALICE.ORG");

            comparer.Compare(alice, bob).Should().BeGreaterThan(0);
            comparer.Compare(bob, alice).Should().BeLessThan(0);
        }

        [Fact]
        public void And_Emails_are_equal_Compare_should_return_Zero()
        {
            // Arrange
            var comparer = new EmailComparer(EmailComparerMode.DomainLocal);
            var alice1 = new Email("alice@BOB.ORG");
            var alice2 = new Email("alice@bob.org");

            // Act
            var result = comparer.Compare(alice1, alice2);

            // Assert
            result.Should().Be(0);
        }
    }
}
