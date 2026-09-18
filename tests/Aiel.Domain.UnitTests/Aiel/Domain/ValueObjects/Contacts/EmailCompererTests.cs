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

namespace Aiel.Domain.ValueObjects.Contacts;

public class EmailComparerTests
{

    public class When_Mode_is_LocalDomain
    {
        [Fact]
        public void Compare_should_return_correct_comparison()
        {
            var comparer = new EmailComparer(EmailComparerMode.LocalDomain);
            var alice = Email.From("alice@BOB.org");
            var bob = Email.From("bob@alice.org");

            comparer.Compare(alice, bob).Should().BeNegative();
            comparer.Compare(bob, alice).Should().BePositive();
        }

        [Fact]
        public void And_Emails_are_equal_Compare_should_return_Zero()
        {
            // Arrange
            var comparer = new EmailComparer(EmailComparerMode.LocalDomain);
            var alice1 = Email.From("alice@BOB.org");
            var alice2 = Email.From("alice@bob.org");

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
            var alice = Email.From("alice@bob.org");
            var bob = Email.From("bob@ALICE.ORG");

            comparer.Compare(alice, bob).Should().BePositive();
            comparer.Compare(bob, alice).Should().BeNegative();
        }

        [Fact]
        public void And_Emails_are_equal_Compare_should_return_Zero()
        {
            // Arrange
            var comparer = new EmailComparer(EmailComparerMode.DomainLocal);
            var alice1 = Email.From("alice@BOB.ORG");
            var alice2 = Email.From("alice@bob.org");

            // Act
            var result = comparer.Compare(alice1, alice2);

            // Assert
            result.Should().Be(0);
        }
    }
}
