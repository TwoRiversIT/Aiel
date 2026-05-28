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

using Aiel.Queries;
using Aiel.Specifications;

namespace Aiel.IntegrationTests;

public class RepositoryTests(SpecificationTestFixture fixture, ITestOutputHelper outputHelper)
    : SpecificationTestBase(fixture, outputHelper)
{
    [Fact]
    public async Task And()
    {
        var spec = new UserIsAgeOfMajority(TimeProvider).And(new UserHasGender(Gender.Male));

        var count = await CountAsync(spec);
        count.Should().Be(1);
    }

    [Fact]
    public async Task Any()
    {
        var any = await SUT.AnyAsync(p => p.DateOfBirth == new DateTime(1974, 10, 16), CancellationToken);

        any.Should().BeTrue();
    }

    [Fact]
    public async Task Count()
    {
        var count = await SUT.CountAsync(p => p.Gender == Gender.Female, CancellationToken);

        count.Should().Be(2);
    }

    [Fact]
    public async Task Not()
    {
        var spec = !new UserIsAgeOfMajority(TimeProvider);

        var count = await CountAsync(spec);
        count.Should().Be(3);
    }

    [Fact]
    public async Task Or()
    {
        var spec = new UserIsAgeOfMajority(TimeProvider).Or(new UserHasGender(Gender.Male));

        var count = await CountAsync(spec);
        count.Should().Be(2);
    }

    [Fact]
    public async Task OrderBy()
    {
        var spec = new QuerySpecification<Person>(_ => true);

        await foreach (var person in SUT.FindAsync(spec, new SortRequest([new SortField(nameof(Person.Name))])))
        {
            person.Name.Should().Be("Doug");
            break;
        }
    }

    [Fact]
    public async Task OrderByDescending()
    {
        var spec = new QuerySpecification<Person>(_ => true);

        await foreach (var person in SUT.FindAsync(spec, new SortRequest([new SortField(nameof(Person.Name), SortDirection.Descending)])))
        {
            person.Name.Should().Be("Shyloh");
            break;
        }
    }

    [Fact]
    public async Task Paging()
    {
        var spec = new QuerySpecification<Person>(_ => true);

        await foreach (var person in SUT.FindAsync(
            spec,
            new SortRequest([new SortField(nameof(Person.DateOfBirth))]),
            new PageRequest(2, 1)))
        {
            person.Name.Should().Be("Shyloh");
        }
    }

    [Fact]
    public async Task UserIsAgeOfMajority()
    {
        var spec = new UserIsAgeOfMajority(TimeProvider);

        var count = await CountAsync(spec);
        count.Should().Be(1);
    }

    [Fact]
    public async Task UserHasGender()
    {
        var spec = new UserHasGender(Gender.Female);

        var count = await CountAsync(spec);
        count.Should().Be(2);
    }

    private async Task<Int32> CountAsync(QuerySpecification<Person> spec)
    {
        var count = 0;
        await foreach (var user in SUT.FindAsync(spec))
        {
            count++;
        }

        return count;
    }
}
