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

using Aiel.Actions.Queries.EntityFrameworkCore;
using Aiel.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aiel.Actions.Queries.Specifications;

public class SpecificationTestFixture : IntegrationTestFixture
{
    protected override void ConfigureConfiguration(IConfigurationBuilder builder)
    {
        // By default, the base IntegrationTestFixutre tries to load appsettings.Testing.json.
        // We do not need that for these tests, so we override this method to do nothing,
        // preventing the base class from trying to load a configuration file that does not
        // exist and throwing an exception.
    }

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var instance = Guid.NewGuid().ToString();

        services.AddDbContext<TestDbContext>(options =>
            options.UseInMemoryDatabase(instance)
                   .EnableSensitiveDataLogging(true));

        services.AddScoped<QuerySpecificationRepository<Person, TestDbContext>>();
    }

    protected override async ValueTask InitializeFixtureAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        TimeProvider.SetDate(2024, 01, 01);

        var dbContext = services.GetRequiredService<TestDbContext>();

        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        await dbContext.People.AddRangeAsync(
            new Person(Guid.NewGuid(), "Doug", new DateTime(1974, 10, 16), Gender.Male),
            new Person(Guid.NewGuid(), "Shyloh", new DateTime(2007, 10, 15), Gender.Female),
            new Person(Guid.NewGuid(), "Piper", new DateTime(2008, 5, 19), Gender.Female),
            new Person(Guid.NewGuid(), "Geordi", new DateTime(2011, 9, 14), Gender.Male)
        );

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

public abstract class SpecificationTestBase(SpecificationTestFixture fixture, ITestOutputHelper outputHelper)
    : IntegrationTestBase<QuerySpecificationRepository<Person, TestDbContext>, SpecificationTestFixture>(fixture, outputHelper)
{
}
