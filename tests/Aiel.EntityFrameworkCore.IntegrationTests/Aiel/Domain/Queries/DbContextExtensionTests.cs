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
using Aiel.Domain.Specifications;
using Aiel.Testing.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Aiel.Domain.Queries;

public class DbContextExtensionTests(QueriesTestFixture fixture, ITestOutputHelper outputHelper)
    : QueriesTestBase(fixture, outputHelper)
{
    [Fact]
    public async Task ExtenstionMethodsWrappingQueriesWorkCorrectly()
    {
        // Arrange
        var dbContext = Services.GetRequiredService<TestDbContext>();

        // Act
        var list = await dbContext.ListPeople(new ListPeople()).ToListAsync(CancellationToken);

        // Assert
        list.Should().HaveCount(4);
        list.Should().BeInDescendingOrder(p => p.DateOfBirth);
    }
}

public sealed record ListPeople(SortOrder? sortRequest = null, Page? pageRequest = null)
    : QueryMultiple<PersonDto>(sortRequest ?? DefaultSort, pageRequest ?? Page.Default)
{
    public static readonly SortOrder DefaultSort = new([
        new SortField(nameof(PersonDto.DateOfBirth), SortDirection.Descending)
    ]);
}

public static class TestDbContextExtensions
{
    public static IQueryable<Person> ListPeople(this TestDbContext dbContext, ListPeople request)
    {
        var specification = new EntitySpecification<Person>(p => true);

        return dbContext.QueryMultiple(request, specification);
    }
}
