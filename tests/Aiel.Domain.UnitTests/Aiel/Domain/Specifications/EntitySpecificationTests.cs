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

using Aiel.Domain.Contacts;
using Aiel.Testing.Models;

namespace Aiel.Domain.Specifications;

public class EntitySpecificationTests
{
    [Fact]
    public void Behaves_appropriately_when_Specification_is_null()
    {
        var spec = new NullEntitySpecification();
        spec.IsSatisfiedBy(null!).Should().BeFalse();
        spec.IsSatisfiedBy(Person.Empty).Should().BeFalse();
        spec.IsSatisfiedBy(Person.Create()).Should().BeFalse();
    }

    [Fact]
    public void Throws_when_constructed_with_null_parameters()
    {
        var ex = Record.Exception(() => new EntitySpecification<Person>(null!));

        ex.Should().BeOfType<ArgumentNullException>();
    }

    [Theory]
    [InlineData("Doug", Gender.Male, true)]
    [InlineData("Doug", Gender.Other, false)]
    [InlineData("Shyloh", Gender.Male, false)]
    public void Can_be_combined(String firstName, Gender gender, Boolean expected)
    {
        // Arrange
        var person = Person.Create(firstName: "Doug", gender: Gender.Male);
        var spec = new HasFirstName(firstName).And(new HasGender(gender));

        // Act & Assert
        spec.IsSatisfiedBy(person).Should().Be(expected);
    }

    [Theory]
    [InlineData(16, false)]
    [InlineData(18, true)]
    [InlineData(20, true)]
    public void InternalExpression_Still_Works(Int32 input, Boolean expected)
    {
        var person = Person.Create(dateOfBirth: DateOnly.FromDateTime(DateTime.Today.AddYears(-input)));
        var spec = new IsAgeOfMajority();

        spec.IsSatisfiedBy(person).Should().Be(expected);
    }

    [Fact]
    public async Task And()
    {
        // Arrange
        var id = PersonId.From(Guid.NewGuid());
        var person = Person.Create(id, "John", "Doe", "", DateOnly.FromDateTime(DateTime.Today.AddYears(-20)), Gender.Male);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var spec = new IsAgeOfMajority(today).And(new HasGender(Gender.Male));

        // Act & Assert
        spec.IsSatisfiedBy(person).Should().BeTrue();
    }

    [Fact]
    public async Task PersonIsAgeOfMajority()
    {
        var id = PersonId.From(Guid.NewGuid());
        var person = Person.Create(id, "John", "Doe", "", DateOnly.FromDateTime(DateTime.Today.AddYears(-20)), Gender.Male);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var spec = new IsAgeOfMajority(today);

        // Act & Assert
        spec.IsSatisfiedBy(person).Should().BeTrue();
    }

    [Fact]
    public async Task PersonHasGender()
    {
        var id = PersonId.From(Guid.NewGuid());
        var person = Person.Create(id, "John", "Doe", "", DateOnly.FromDateTime(DateTime.Today.AddYears(-20)), Gender.Male);
        var spec = new HasGender(Gender.Female);

        // Act & Assert
        spec.IsSatisfiedBy(person).Should().BeFalse();
    }
}
