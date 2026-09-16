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
using Aiel.StrongIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aiel.Testing.Dummies;

public class TypicalClassConfiguration : IEntityTypeConfiguration<TypicalClass>
{
    public void Configure(EntityTypeBuilder<TypicalClass> builder)
    {
        builder.HasKey(tc => tc.Id);
        builder.Property(x => x.Id).HasStrongIdConversion<TypicalId, Guid>();

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
