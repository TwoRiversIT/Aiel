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

using Aiel.Actions.Queries.Specifications;
using Aiel.Framework;
using Aiel.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Aiel.Actions.Queries;

public abstract class QueriesTestBase(QueriesTestFixture fixture, ITestOutputHelper outputHelper)
    : IntegrationTestBase<QuerySpecificationRepository<Person, TestDbContext>, QueriesTestFixture>(fixture, outputHelper)
{
}

public class QueriesTestFixture : IntegrationTestFixture
{
    public override ValueTask ConfigureAsync(ConfigurationContext context, CancellationToken cancellationToken = default)
    {
        var instance = Guid.NewGuid().ToString();

        context.Services.AddDbContext<TestDbContext>(options =>
            options.UseInMemoryDatabase(instance)
                   .EnableSensitiveDataLogging(true));

        context.Services.AddScoped<QuerySpecificationRepository<Person, TestDbContext>>();

        return ValueTask.CompletedTask;
    }

    public override async ValueTask InitializeAsync(InitializationContext context, CancellationToken cancellationToken = default)
    {
        TimeProvider.SetDate(2024, 01, 01);

        var dbContext = context.Services.GetRequiredService<TestDbContext>();

        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        await dbContext.People.AddRangeAsync(
            new Person(Guid.NewGuid(), "Doug", new DateOnly(1974, 10, 16), Gender.Male),
            new Person(Guid.NewGuid(), "Shyloh", new DateOnly(2007, 10, 15), Gender.Female),
            new Person(Guid.NewGuid(), "Piper", new DateOnly(2008, 5, 19), Gender.Female),
            new Person(Guid.NewGuid(), "Geordi", new DateOnly(2011, 9, 14), Gender.Male)
        );

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
