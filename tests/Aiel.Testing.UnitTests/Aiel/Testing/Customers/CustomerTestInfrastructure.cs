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

using Aiel.Framework;
using Aiel.Testing.Dummies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Aiel.Testing.Customers;

public abstract class CustomerTestBase<TSut>(CustomersFixture<TSut> fixture, ITestOutputHelper output)
    : SpecificationTestBase<CustomersFixture<TSut>, TSut>(fixture, output)
    where TSut : class
{
}

public class CustomersFixture<TSut> : SpecificationFixture<TSut>
    where TSut : class
{
    public override ValueTask ConfigureAsync(ConfigurationContext context, CancellationToken cancellationToken = default)
    {
        // Register data access
        context.Services.AddDbContext<DummyDbContext>(options =>
            options.UseInMemoryDatabase("CustomerTests")
                   .EnableSensitiveDataLogging(true));

        // Register repositories
        context.Services.AddScoped<ICustomerRepository, CustomerRepository>();

        // Register services
        context.Services.AddScoped<CustomerApplicationService>();

        return ValueTask.CompletedTask;
    }

    public override async ValueTask InitializeAsync(InitializationContext context, CancellationToken cancellationToken = default)
    {
        // Ensure database schema exists
        var dbContext = context.Services.GetRequiredService<DummyDbContext>();
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);

        // And no data left from previous tests
        dbContext.Customers.RemoveRange(dbContext.Customers);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
