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

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Aiel.Results;

/// <summary>
/// Test record for serialization tests.
/// </summary>
/// <param name="Id">The ID.</param>
/// <param name="Name">The name.</param>
/// <param name="Email">The email address.</param>
public record TestRecord(Int32 Id, String Name, String Email);

public class InvoiceDto
{
    public Guid Id { get; set; }
    public Int32 InvoiceNumber { get; set; }
    public DateTime InvoiceDate { get; set; }
    public Customer Customer { get; set; } = default!;
    public Address BillingAddress { get; set; } = default!;
    public Address ShippingAddress { get; set; } = default!;
    public Decimal AmountDue { get; set; }
    public DateTime DueDate { get; set; }
    public String InternalNote { get; set; } = default!;
    public String CustomerNote { get; set; } = default!;
    public IList<LineItemDto> LineItems { get; set; } = [];

    public static InvoiceDto BogusInvoice => new Faker<InvoiceDto>()
        .RuleFor(i => i.Id, _ => Guid.NewGuid())
        .RuleFor(i => i.InvoiceNumber, f => f.Random.Int(1000, 9999))
        .RuleFor(i => i.InvoiceDate, f => f.Date.Past())
        .RuleFor(i => i.Customer, _ => Customer.BogusCustomer)
        .RuleFor(i => i.BillingAddress, _ => Address.BogusAddress)
        .RuleFor(i => i.ShippingAddress, _ => Address.BogusAddress)
        .RuleFor(i => i.AmountDue, f => f.Finance.Amount())
        .RuleFor(i => i.DueDate, f => f.Date.Future())
        .RuleFor(i => i.InternalNote, f => f.Lorem.Sentence())
        .RuleFor(i => i.CustomerNote, f => f.Lorem.Sentence())
        .RuleFor(m => m.LineItems, (f, __) => f.Make(5, _ => LineItemDto.BogusLineItem));
}

[method: JsonConstructor]
public record LineItemDto(
    String ItemCode,
    String ShortDescription,
    String LongDescription,
    Decimal QtyOrder,
    Decimal UnitPrice,
    Decimal TaxRate,
    String InternalNote,
    String CustomerNote
)
{
    public Decimal LineNet => QtyOrder * UnitPrice;
    public Decimal LineTotal => LineNet * (1 + TaxRate);

    [SuppressMessage("Usage", "BG1001:The Faker[T] has missing property or field rules", Justification = "Records are immutable.")]
    public static LineItemDto BogusLineItem => new Faker<LineItemDto>()
        .CustomInstantiator(f => new LineItemDto(
            f.Commerce.Ean8(),
            f.Commerce.Product(),
            f.Commerce.ProductDescription(),
            f.Random.Decimal(0.0m, 20.0m),
            f.Random.Decimal(1.00m, 100.00m),
            f.Random.Decimal(0.00m, 0.15m),
            f.Lorem.Sentence(),
            f.Lorem.Sentence()
        ));
}

public class Address
{
    public Guid Id { get; set; }
    public String Street { get; set; } = String.Empty;
    public String City { get; set; } = String.Empty;
    public String State { get; set; } = String.Empty;
    public String PostalCode { get; set; } = String.Empty;
    public String Country { get; set; } = String.Empty;

    public static Address BogusAddress => new Faker<Address>()
        .RuleFor(a => a.Id, _ => Guid.NewGuid())
        .RuleFor(a => a.Street, f => f.Address.StreetAddress())
        .RuleFor(a => a.City, f => f.Address.City())
        .RuleFor(a => a.State, f => f.Address.State())
        .RuleFor(a => a.PostalCode, f => f.Address.ZipCode())
        .RuleFor(a => a.Country, f => f.Address.Country());
}

public class Customer
{
    public Guid Id { get; set; }
    public String Name { get; set; } = String.Empty;
    public String Email { get; set; } = String.Empty;
    public String Phone { get; set; } = String.Empty;

    public static Customer BogusCustomer => new Faker<Customer>()
        .RuleFor(c => c.Id, _ => Guid.NewGuid())
        .RuleFor(c => c.Name, f => f.Company.CompanyName())
        .RuleFor(c => c.Email, f => f.Internet.Email())
        .RuleFor(c => c.Phone, f => f.Phone.PhoneNumber());
}
