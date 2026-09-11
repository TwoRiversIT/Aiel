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
using Aiel.Testing.StrongIds;
using Microsoft.EntityFrameworkCore;

namespace Aiel.EntityFrameworkCore;

public class StrongIdValueConverterTests
{
    [Fact]
    public async Task HasStrongIdConversion_maps_generated_strong_id_keys_and_foreign_keys()
    {
        var databaseName = Guid.NewGuid().ToString("N");

        var guidFalseId = GuidAllowDefaultFalseId.From(Guid.NewGuid());
        var guidFalseIdNullable = GuidAllowDefaultFalseId.From(Guid.NewGuid());
        var guidTrueId = GuidAllowDefaultTrueId.From(Guid.NewGuid());
        var intFalseId = Int32AllowDefaultFalseId.From(100);
        var intFalseIdNullable = Int32AllowDefaultFalseId.From(200);
        var intTrueId = Int32AllowDefaultTrueId.From(300);
        var stringFalseId = StringAllowDefaultFalseId.From("abc");
        var stringFalseIdNullable = StringAllowDefaultFalseId.From("def");
        var stringTrueId = StringAllowDefaultTrueId.From("ghi");

        await using (var writeContext = CreateDbContext(databaseName))
        {
            writeContext.Orders.Add(new StrongIdOrder
            {
                Description = "alpha",
                GuidAllowDefaultFalse = guidFalseId,
                GuidAllowDefaultFalseNullable = guidFalseIdNullable,
                GuidAllowDefaultTrue = guidTrueId,
                Int32AllowDefaultFalse = intFalseId,
                Int32AllowDefaultFalseNullable = intFalseIdNullable,
                Int32AllowDefaultTrue = intTrueId,
                StringAllowDefaultFalse = stringFalseId,
                StringAllowDefaultFalseNullable = stringFalseIdNullable,
                StringAllowDefaultTrue = stringTrueId
            });

            await writeContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var readContext = CreateDbContext(databaseName);

        var entityType = readContext.Model.FindEntityType(typeof(StrongIdOrder));
        var idProperty = entityType?.FindProperty(nameof(StrongIdOrder.GuidAllowDefaultFalse));
        var customerIdProperty = entityType?.FindProperty(nameof(StrongIdOrder.Int32AllowDefaultFalse));

        idProperty.Should().NotBeNull();
        customerIdProperty.Should().NotBeNull();
        idProperty!.GetValueConverter().Should().NotBeNull();
        customerIdProperty!.GetValueConverter().Should().NotBeNull();
        idProperty.GetValueConverter()!.ProviderClrType.Should().Be<Guid>();
        customerIdProperty.GetValueConverter()!.ProviderClrType.Should().Be<Int32>();

        var persisted = await readContext.Orders.SingleAsync(TestContext.Current.CancellationToken);

        persisted.GuidAllowDefaultFalse.Should().Be(guidFalseId);
        persisted.Int32AllowDefaultFalse.Should().Be(intFalseId);
    }

    [Fact]
    public async Task HasStrongIdConversion_maps_nullable_generated_strong_ids()
    {
        var databaseName = Guid.NewGuid().ToString("N");

        var guidId = GuidAllowDefaultFalseId.From(Guid.NewGuid());
        var guidIdNullable = GuidAllowDefaultFalseId.From(Guid.NewGuid());
        var intId = Int32AllowDefaultFalseId.From(100);
        var intIdNullable = Int32AllowDefaultFalseId.From(200);
        var stringId = StringAllowDefaultFalseId.From("abc");
        var stringIdNullable = StringAllowDefaultFalseId.From("def");

        await using (var writeContext = CreateDbContext(databaseName))
        {
            writeContext.Orders.Add(new StrongIdOrder
            {
                GuidAllowDefaultFalse = guidId,
                GuidAllowDefaultFalseNullable = guidIdNullable,
                Int32AllowDefaultFalse = intId,
                Int32AllowDefaultFalseNullable = intIdNullable,
                StringAllowDefaultFalse = stringId,
                StringAllowDefaultFalseNullable = stringIdNullable,
                StringAllowDefaultTrue = StringAllowDefaultTrueId.From("required-beta"),
                Description = "beta"
            });

            writeContext.Orders.Add(new StrongIdOrder
            {
                GuidAllowDefaultFalse = guidId,
                Int32AllowDefaultFalse = intId,
                StringAllowDefaultFalse = stringId,
                StringAllowDefaultTrue = StringAllowDefaultTrueId.From("required-gamma"),
                Description = "gamma"
            });

            await writeContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using var readContext = CreateDbContext(databaseName);

        var entityType = readContext.Model.FindEntityType(typeof(StrongIdOrder));
        var optionalCustomerIdProperty = entityType?.FindProperty(nameof(StrongIdOrder.GuidAllowDefaultFalseNullable));

        optionalCustomerIdProperty.Should().NotBeNull();
        optionalCustomerIdProperty!.GetValueConverter().Should().NotBeNull();
        optionalCustomerIdProperty.GetValueConverter()!.ProviderClrType.Should().Be<Guid>();

        var persisted = await readContext.Orders
            .OrderBy(static order => order.Description)
            .ToListAsync(TestContext.Current.CancellationToken);

        persisted.Should().HaveCount(2);
        persisted[0].GuidAllowDefaultFalseNullable.Should().Be(guidIdNullable);
        persisted[1].GuidAllowDefaultFalseNullable.Should().BeNull();
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
        public Int32 Id { get; set; }
        public GuidAllowDefaultFalseId GuidAllowDefaultFalse { get; set; }
        public GuidAllowDefaultFalseId? GuidAllowDefaultFalseNullable { get; set; }
        public GuidAllowDefaultTrueId GuidAllowDefaultTrue { get; set; }
        public Int32AllowDefaultFalseId Int32AllowDefaultFalse { get; set; }
        public Int32AllowDefaultFalseId? Int32AllowDefaultFalseNullable { get; set; }
        public Int32AllowDefaultTrueId Int32AllowDefaultTrue { get; set; }
        public StringAllowDefaultFalseId StringAllowDefaultFalse { get; set; }
        public StringAllowDefaultFalseId? StringAllowDefaultFalseNullable { get; set; }
        public StringAllowDefaultTrueId StringAllowDefaultTrue { get; set; }

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

                entity.Property(static order => order.Description).IsRequired();

                entity.Property(static order => order.GuidAllowDefaultFalse).HasStrongIdConversion<GuidAllowDefaultFalseId, Guid>();
                entity.Property(static order => order.GuidAllowDefaultFalseNullable).HasStrongIdConversion<GuidAllowDefaultFalseId, Guid>();
                entity.Property(static order => order.GuidAllowDefaultTrue).HasStrongIdConversion<GuidAllowDefaultTrueId, Guid>();

                entity.Property(static order => order.Int32AllowDefaultFalse).HasStrongIdConversion<Int32AllowDefaultFalseId, Int32>();
                entity.Property(static order => order.Int32AllowDefaultFalseNullable).HasStrongIdConversion<Int32AllowDefaultFalseId, Int32>();
                entity.Property(static order => order.Int32AllowDefaultTrue).HasStrongIdConversion<Int32AllowDefaultTrueId, Int32>();

                entity.Property(static order => order.StringAllowDefaultFalse).HasStrongIdConversion<StringAllowDefaultFalseId, String>();
                entity.Property(static order => order.StringAllowDefaultFalseNullable).HasStrongIdConversion<StringAllowDefaultFalseId, String>();
                entity.Property(static order => order.StringAllowDefaultTrue).HasStrongIdConversion<StringAllowDefaultTrueId, String>();
            });
        }
    }
}

[StrongId<Guid>(GenerateTryFrom = true)]
public readonly partial record struct OrderId;

[StrongId<Guid>]
public readonly partial record struct CustomerId;
