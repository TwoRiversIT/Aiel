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

using Aiel.Actions.Queries;
using Aiel.Domain.Contacts;
using Aiel.Domain.Queries;
using Aiel.Testing.Models;

namespace Aiel.Domain.Specifications;

public class SpecificationRepositoryTests(QueriesTestFixture fixture, ITestOutputHelper outputHelper)
    : QueriesTestBase(fixture, outputHelper)
{
    private DateOnly Today => DateOnly.FromDateTime(TimeProvider.GetUtcNow().Date);

    [Fact]
    public async Task Any()
    {
        var spec = new IsAgeOfMajority(Today);

        var any = await SUT.AnyAsync(spec, CancellationToken);

        any.Should().BeTrue();
    }

    [Fact]
    public async Task Count()
    {
        var spec = new HasGender(Gender.Female);

        var count = await SUT.CountAsync(spec, CancellationToken);

        count.Should().Be(2);
    }

    [Fact]
    public async Task Not()
    {
        var spec = !new IsAgeOfMajority(Today);

        var count = await SUT.CountAsync(spec, CancellationToken);
        count.Should().Be(3);
    }

    [Fact]
    public async Task Or()
    {
        var spec = new IsAgeOfMajority(Today).Or(new HasGender(Gender.Male));

        var count = await SUT.CountAsync(spec, CancellationToken);
        count.Should().Be(2);
    }

    [Fact]
    public async Task OrderBy()
    {
        var spec = new EntitySpecification<Person>(_ => true);

        await foreach (var person in SUT.FindAsync(spec, new SortOrder([new SortField(nameof(Person.DateOfBirth), SortDirection.Descending)]), Page.Default))
        {
            person.FirstName.Should().Be("Geordi");
            break;
        }
    }

    [Fact]
    public async Task OrderByDescending()
    {
        var spec = new EntitySpecification<Person>(_ => true);

        await foreach (var person in SUT.FindAsync(spec, new SortOrder([new SortField(nameof(Person.LastName), SortDirection.Ascending), new SortField(nameof(Person.FirstName), SortDirection.Ascending)]), Page.Default))
        {
            person.FirstName.Should().Be("Shyloh");
            break;
        }
    }

    [Fact]
    public async Task Paging()
    {
        var spec = new EntitySpecification<Person>(_ => true);

        await foreach (var person in SUT.FindAsync(
            spec,
            new SortOrder([new SortField(nameof(Person.DateOfBirth))]),
            Page.Create(2, 1, 0)))
        {
            person.FirstName.Should().Be("Shyloh");
        }
    }

    [Fact]
    public async Task Query()
    {
        var query = new ListPeople() { Specification = new EntitySpecification<Person>(_ => true) };
        var result = await SUT.QueryAsync(query, CancellationToken);

        result.Records.Should().HaveCount(4);
        result.Records.Should().BeInAscendingOrder(p => p.LastName);
    }

    private record ListPeople()
        : QueryMultipleSpecification<Person>(
            specification: new EntitySpecification<Person>(_ => true),
            sortRequest: new SortOrder([new SortField(nameof(Person.LastName)), new SortField(nameof(Person.FirstName))]),
            pageRequest: Page.Default)
    {
    }
}
