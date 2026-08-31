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

using Aiel.Multitenancy.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Aiel.MultiTenancy;

public class MultitenancyTests
{
    [Fact]
    public async Task SaveChangesAsync_assigns_tenant_id_to_new_multi_tenant_entities()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tenantId = Guid.NewGuid();
        await using var dbContext = CreateTenantDbContext(TestHelper.BuildTenant(new TenantId(tenantId)), Guid.NewGuid().ToString("N"));

        var entity = new TenantScopedNote { Id = Guid.NewGuid(), Name = "alpha" };

        dbContext.Notes.Add(entity);

        await dbContext.SaveChangesAsync(cancellationToken);

        entity.TenantId.Value.Should().Be(tenantId);
    }

    [Fact]
    public async Task Query_filters_are_scoped_to_the_current_tenant_context()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var databaseName = Guid.NewGuid().ToString("N");
        var firstTenant = Guid.NewGuid();
        var secondTenant = Guid.NewGuid();

        await using (var seedContext = CreateTenantDbContext(TestHelper.BuildTenant(new TenantId(firstTenant)), databaseName))
        {
            await seedContext.Notes.AddRangeAsync(
                [
                    new TenantScopedNote { Id = Guid.NewGuid(), TenantId = new TenantId(firstTenant), Name = "first" },
                    new TenantScopedNote { Id = Guid.NewGuid(), TenantId = new TenantId(secondTenant), Name = "second" }
                ],
                cancellationToken);

            await seedContext.SaveChangesAsync(cancellationToken);
        }

        await using var firstTenantContext = CreateTenantDbContext(TestHelper.BuildTenant(new TenantId(firstTenant)), databaseName);
        await using var secondTenantContext = CreateTenantDbContext(TestHelper.BuildTenant(new TenantId(secondTenant)), databaseName);

        var firstResults = await firstTenantContext.Notes.OrderBy(static note => note.Name).Select(static note => note.Name).ToListAsync(cancellationToken);
        var secondResults = await secondTenantContext.Notes.OrderBy(static note => note.Name).Select(static note => note.Name).ToListAsync(cancellationToken);

        firstResults.Should().Equal("first");
        secondResults.Should().Equal("second");
    }

    private static TenantAwareDbContext CreateTenantDbContext(CurrentTenant currentTenant, String databaseName)
    {
        var options = new DbContextOptionsBuilder<TenantAwareDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new TenantAwareDbContext(options, currentTenant);
    }

    private sealed class TenantAwareDbContext(DbContextOptions<TenantAwareDbContext> options, CurrentTenant currentTenant)
        : MultitenancyDbContext(options, currentTenant)
    {
        public DbSet<TenantScopedNote> Notes => Set<TenantScopedNote>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TenantScopedNote>().HasKey(static note => note.Id);
            base.OnModelCreating(modelBuilder);
        }
    }

    private sealed class TenantScopedNote : IMultiTenant
    {
        public Guid Id { get; set; }

        public TenantId TenantId { get; set; }

        public String Name { get; set; } = String.Empty;
    }

}
