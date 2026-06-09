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

using Aiel.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aiel.EntityFrameworkCore;

public abstract class EFCoreIntegrationTestFixture : IntegrationTestFixture
{
    protected static readonly FamilyMember[] Family =
    [
        new FamilyMember(Guid.NewGuid(), "Doug", DateTime.SpecifyKind(new DateTime(1974, 10, 16), DateTimeKind.Utc)),
        new FamilyMember(Guid.NewGuid(), "Shyloh", DateTime.SpecifyKind(new DateTime(2007, 10, 15), DateTimeKind.Utc)),
        new FamilyMember(Guid.NewGuid(), "Piper", DateTime.SpecifyKind(new DateTime(2008, 5, 19), DateTimeKind.Utc)),
        new FamilyMember(Guid.NewGuid(), "Geordi",DateTime.SpecifyKind( new DateTime(2011, 9, 14), DateTimeKind.Utc))
    ];

    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var instance = Guid.NewGuid().ToString();
        services.AddDbContext<IntegrationTestDbContext>(options =>
            options.UseInMemoryDatabase(instance)
                   .EnableSensitiveDataLogging(true));
    }

    protected override async ValueTask InitializeFixtureAsync(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<IntegrationTestDbContext>();

        await dbContext.Database.EnsureCreatedAsync();

        await dbContext.AddRangeAsync(Family);

        await dbContext.SaveChangesAsync();
    }
}
