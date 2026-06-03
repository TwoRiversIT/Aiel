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

using Aiel.StrongIds;
using Aiel.StrongIds.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Aiel.EntityFrameworkCore;

public class StrongIdValueConverterTests
{
    [Fact]
    public async Task HasStrongIdConversion_maps_generated_strong_id_keys_and_foreign_keys()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var orderId = OrderId.From(Guid.NewGuid());
        var customerId = CustomerId.From(Guid.NewGuid());

        await using (var writeContext = CreateDbContext(databaseName))
        {
            writeContext.Orders.Add(new StrongIdOrder
            {
                Id = orderId,
                CustomerId = customerId,
                Description = "alpha"
            });

            await writeContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var readContext = CreateDbContext(databaseName);

        var entityType = readContext.Model.FindEntityType(typeof(StrongIdOrder));
        var idProperty = entityType?.FindProperty(nameof(StrongIdOrder.Id));
        var customerIdProperty = entityType?.FindProperty(nameof(StrongIdOrder.CustomerId));

        idProperty.Should().NotBeNull();
        customerIdProperty.Should().NotBeNull();
        idProperty!.GetValueConverter().Should().NotBeNull();
        customerIdProperty!.GetValueConverter().Should().NotBeNull();
        idProperty.GetValueConverter()!.ProviderClrType.Should().Be<Guid>();
        customerIdProperty.GetValueConverter()!.ProviderClrType.Should().Be<Guid>();

        var persisted = await readContext.Orders.SingleAsync(TestContext.Current.CancellationToken);

        persisted.Id.Should().Be(orderId);
        persisted.CustomerId.Should().Be(customerId);
    }

    [Fact]
    public async Task HasStrongIdConversion_maps_nullable_generated_strong_ids()
    {
        var databaseName = Guid.NewGuid().ToString("N");
        var orderId = OrderId.From(Guid.NewGuid());
        var optionalCustomerId = CustomerId.From(Guid.NewGuid());

        await using (var writeContext = CreateDbContext(databaseName))
        {
            writeContext.Orders.Add(new StrongIdOrder
            {
                Id = orderId,
                CustomerId = optionalCustomerId,
                OptionalCustomerId = optionalCustomerId,
                Description = "beta"
            });

            writeContext.Orders.Add(new StrongIdOrder
            {
                Id = OrderId.From(Guid.NewGuid()),
                CustomerId = CustomerId.From(Guid.NewGuid()),
                OptionalCustomerId = null,
                Description = "gamma"
            });

            await writeContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var readContext = CreateDbContext(databaseName);

        var entityType = readContext.Model.FindEntityType(typeof(StrongIdOrder));
        var optionalCustomerIdProperty = entityType?.FindProperty(nameof(StrongIdOrder.OptionalCustomerId));

        optionalCustomerIdProperty.Should().NotBeNull();
        optionalCustomerIdProperty!.GetValueConverter().Should().NotBeNull();
        optionalCustomerIdProperty.GetValueConverter()!.ProviderClrType.Should().Be<Guid?>();

        var persisted = await readContext.Orders
            .OrderBy(static order => order.Description)
            .ToListAsync(TestContext.Current.CancellationToken);

        persisted.Should().HaveCount(2);
        persisted[0].OptionalCustomerId.Should().Be(optionalCustomerId);
        persisted[1].OptionalCustomerId.Should().BeNull();
    }

    private static StrongIdOrderDbContext CreateDbContext(String databaseName)
    {
        var options = new DbContextOptionsBuilder<StrongIdOrderDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new StrongIdOrderDbContext(options);
    }

    private sealed class StrongIdOrder
    {
        public OrderId Id { get; set; }

        public CustomerId CustomerId { get; set; }

        public CustomerId? OptionalCustomerId { get; set; }

        public String Description { get; set; } = String.Empty;
    }

    private sealed class StrongIdOrderDbContext(DbContextOptions<StrongIdOrderDbContext> options)
        : DbContext(options)
    {
        public DbSet<StrongIdOrder> Orders => Set<StrongIdOrder>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StrongIdOrder>(entity =>
            {
                entity.HasKey(static order => order.Id);
                entity.Property(static order => order.Id).HasStrongIdConversion<OrderId, Guid>();
                entity.Property(static order => order.CustomerId).HasStrongIdConversion<CustomerId, Guid>();
                entity.Property(static order => order.OptionalCustomerId).HasStrongIdConversion<CustomerId, Guid>();
                entity.Property(static order => order.Description).IsRequired();
            });
        }
    }
}

[StrongId<Guid>]
public readonly partial record struct OrderId;

[StrongId<Guid>]
public readonly partial record struct CustomerId;
