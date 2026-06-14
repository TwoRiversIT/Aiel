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

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Diagnostics;

namespace Aiel.DataAccess.EntityFrameworkCore;

[DebuggerDisplay("{DebuggerDisplay}")]
public abstract class Entity
{
    public virtual String DebuggerDisplay => GetType().Name + " [" + Id + "]";

    public Guid Id { get; set; }
    //public Byte[] RowVersion { get; set; } = Array.Empty<Byte>();
}

public abstract class EntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : Entity
{
    protected virtual String RowVersionColumnName => "RowVersion";

    public virtual void Configure(EntityTypeBuilder<TEntity> entity)
    {
        //entity.Property(e => e.RowVersion).HasColumnName(RowVersionColumnName).IsRowVersion();
    }
}

public class Customer : Entity
{
    public String Name { get; set; } = String.Empty;
    public virtual ICollection<Invoice> Invoices { get; set; } = [];

    [SuppressMessage("Usage", "BG1001:The Faker[TEntity] has missing property or field rules", Justification = "<Pending>")]
    public static Customer Fake() => new Faker<Customer>()
        .RuleFor(c => c.Name, f => f.Person.FullName);
}

public class CustomerConfiguration : EntityConfiguration<Customer>
{
    public override void Configure(EntityTypeBuilder<Customer> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Name).IsRequired(true);

        entity.HasMany(e => e.Invoices)
              .WithOne(i => i.Customer)
              .HasForeignKey(i => i.CustomerId)
              .OnDelete(DeleteBehavior.Cascade);

        base.Configure(entity);
    }
}

public class Invoice : Entity
{
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = default!;

    public DateTime InvoiceDate { get; set; }
    public String InvoiceNumber { get; set; } = String.Empty;

    public virtual ICollection<LineItem> LineItems { get; set; } = [];

    [SuppressMessage("Usage", "BG1001:The Faker[TEntity] has missing property or field rules", Justification = "<Pending>")]
    public static Invoice Fake() => new Faker<Invoice>()
                .RuleFor(i => i.InvoiceDate, f => f.Date.Past())
                .RuleFor(i => i.InvoiceNumber, f => f.Lorem.Word());
}

public class InvoiceConfiguration : EntityConfiguration<Invoice>
{
    public override void Configure(EntityTypeBuilder<Invoice> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.Id).IsRequired(true);
        entity.Property(e => e.CustomerId).IsRequired(true);

        entity.Property(e => e.InvoiceNumber).IsRequired(true);
        entity.Property(e => e.InvoiceDate).HasColumnType("date").IsRequired(true);

        entity.HasMany(p => p.LineItems)
              .WithOne(c => c.Invoice)
              .HasForeignKey(c => c.InvoiceId)
              .OnDelete(DeleteBehavior.Cascade);

        base.Configure(entity);
    }
}

public class LineItem : Entity
{
    public Guid InvoiceId { get; set; }
    public Invoice Invoice { get; set; } = default!;
    public Int32 Line { get; set; }
    public Single Quantity { get; set; }
    public String Name { get; set; } = String.Empty;
    public String Description { get; set; } = String.Empty;
    public Decimal UnitPrice { get; set; }
    public Guid ProductID { get; set; }
    public Product Product { get; set; } = default!;
}

public class LineItemConfiguration : EntityConfiguration<LineItem>
{
    public override void Configure(EntityTypeBuilder<LineItem> entity)
    {
        entity.HasKey(e => e.Id);

        entity.Property(e => e.InvoiceId).IsRequired(true);
        entity.Property(e => e.Line).IsRequired(true);

        entity.Property(e => e.Quantity).HasDefaultValue(1.0M).IsRequired(true);
        entity.Property(e => e.Name).IsRequired(true);
        entity.Property(e => e.Description).IsRequired(true);
        entity.Property(e => e.UnitPrice).HasColumnType("money").IsRequired(true);

        entity.HasOne(e => e.Invoice)
              .WithMany(p => p.LineItems)
              .HasForeignKey(p => p.InvoiceId)
              .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(i => i.Product)
              .WithMany()
              .HasForeignKey(i => i.ProductID);

        base.Configure(entity);
    }
}

public class Product : Entity
{
    public String Name { get; set; } = String.Empty;
    public String Description { get; set; } = String.Empty;
    public Decimal UnitPrice { get; set; }

    [SuppressMessage("Usage", "BG1001:The Faker[TEntity] has missing property or field rules", Justification = "<Pending>")]
    public static Product Fake() => new Faker<Product>()
        .RuleFor(p => p.Name, f => f.Commerce.Product())
        .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
        .RuleFor(p => p.UnitPrice, f => f.Random.Decimal(0.0m, 100.0m));
}

public class ProductConfiguration : EntityConfiguration<Product>
{
    public override void Configure(EntityTypeBuilder<Product> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).IsRequired(true);
        entity.Property(e => e.Description).IsRequired(true);
        entity.Property(e => e.UnitPrice).HasColumnType("money").IsRequired(true);

        base.Configure(entity);
    }
}
