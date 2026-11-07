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

using Aiel.Domain.EntityFrameworkCore.ValueConverters;
using Aiel.Domain.ValueObjects;
using Aiel.Domain.ValueObjects.Net;
using Aiel.StrongIds;
using ExampleShared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Example;

/// <summary>
/// A simple class containing all of the intrinsic types to test serialization and deserialization of all of the intrinsic types.
/// </summary>
public class ExampleEntity
{
    public ExampleId Id { get; set; }

    // Value Objects
    public Address Address { get; set; } = default!;

    public Boolean BoolValue { get; set; } = true;
    public DateTime DateTimeValue { get; set; } = DateTime.Now;
    public DateTimeOffset DateTimeOffsetValue { get; set; } = DateTimeOffset.UtcNow;
    public Decimal DecimalValue { get; set; } = 3.1415m;
    public DomainName DomainName { get; set; } = default!;
    public Double DoubleValue { get; set; } = 2.71828;
    public Email Email { get; set; } = default!;
    public EmailAddress EmailAddress { get; set; } = default!;
    public EndPoint EndPoint { get; set; } = default!;
    public Single FloatValue { get; set; } = 1.618f;
    public Guid GuidValue { get; set; } = Guid.NewGuid();

    // Value Types
    public Int32 IntValue { get; set; } = 42;

    public PhoneNumber PhoneNumber { get; set; } = default!;

    // Reference Types
    public String StringValue { get; set; } = "A simple string value";
}

public class ExampleEntityConfiguration : IEntityTypeConfiguration<ExampleEntity>
{
    public void Configure(EntityTypeBuilder<ExampleEntity> builder)
    {
        builder.HasKey(tc => tc.Id);
        builder.Property(x => x.Id).HasStrongIdConversion<ExampleId, Guid>();

        builder.Property(x => x.Address).HasConversion<AddressValueConverter>();

        builder.Property(e => e.BoolValue);
        builder.Property(e => e.DateTimeValue);
        builder.Property(e => e.DateTimeOffsetValue);
        builder.Property(e => e.DecimalValue);
        builder.Property(e => e.DomainName).HasConversion<DomainNameValueConverter>();
        builder.Property(e => e.DoubleValue);
        builder.Property(e => e.Email).HasConversion<EmailValueConverter>();
        builder.Property(e => e.EmailAddress).HasConversion<EmailAddressValueConverter>();
        builder.Property(e => e.EndPoint).HasConversion<EndPointValueConverter>();
        builder.Property(e => e.FloatValue);
        builder.Property(e => e.GuidValue);
        builder.Property(e => e.IntValue);
        builder.Property(e => e.PhoneNumber).HasConversion<PhoneNumberValueConverter>();
        builder.Property(e => e.StringValue);
    }
}
