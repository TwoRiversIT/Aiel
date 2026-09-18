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

using Aiel.Domain.Contacts;
using Aiel.Domain.Geography;
using Aiel.Domain.ValueObjects.Addresses;
using Aiel.Domain.ValueObjects.Geography;
using Aiel.Domain.ValueObjects.Net;
using Aiel.Testing;
using Aiel.Testing.Dummies;
using Microsoft.EntityFrameworkCore;

namespace Aiel.Domain.ValueObjects;

public class ValueConverterTests(SystemUnderTestConfiguratorTestFixture<AielDomainIntegrationTests, DummyDbContext> fixture, ITestOutputHelper output)
        : SystemUnderTestConfiguratorTestBase<AielDomainIntegrationTests, SystemUnderTestConfiguratorTestFixture<AielDomainIntegrationTests, DummyDbContext>, DummyDbContext>(fixture, output)
{

    [Fact]
    public async Task Can_Write_TypicalClass_To_DbContext()
    {
        // Arrange
        var timestamp = DateTimeOffset.UtcNow;
        var person = Person.Create();
        var typicalId = TypicalId.From(Guid.NewGuid());
        var typicalClass = new TypicalClass()
        {
            Id = typicalId,
            Address = new Address()
            {
                Addressee = person.FullName,
                Line1 = "123 Main St",
                City = "Anytown",
                Province = Provinces.BC,
                PostalCode = new PostalCode("V1V", "1V1"),
                Country = new Country("Canada", "CAN")
            },
            BoolValue = true,
            DateTimeValue = timestamp.DateTime,
            DateTimeOffsetValue = timestamp,
            DecimalValue = 123.45m,
            DomainName = (DomainName)"example.com",
            DoubleValue = 123.45,
            Email = person.Email,
            EmailAddress = person.EmailAddress,
            EndPoint = new EndPoint("127.0.0.1", 8080),
            FloatValue = 123.45f,
            GuidValue = Guid.NewGuid(),
            IntValue = 42,
            PhoneNumber = PhoneNumber.Parse("(234) 567-8901 ext 123"),
            StringValue = "Hello, World!"
        };

        // Act
        SUT.Add(typicalClass);
        await SUT.SaveChangesAsync(CancellationToken);

        // Assert
        var inserted = await SUT.TypicalClasses.SingleAsync(tc => tc.Id == typicalId, CancellationToken);

        inserted.Address.Addressee.Should().Be(person.FullName);
        inserted.Address.Province.Should().Be(Provinces.BC);
        inserted.Address.PostalCode.Should().Be(typicalClass.Address.PostalCode);
        inserted.BoolValue.Should().Be(true);
        inserted.DateTimeValue.Should().BeCloseTo(timestamp.DateTime, TimeSpan.FromSeconds(1));
        inserted.DateTimeOffsetValue.Should().BeCloseTo(timestamp, TimeSpan.FromSeconds(1));
        inserted.DecimalValue.Should().Be(123.45m);
        inserted.DomainName.Should().Be(DomainName.From("example.com"));
        inserted.DoubleValue.Should().Be(123.45);
        inserted.Email.Should().Be(person.Email);
        inserted.EmailAddress.Should().Be(person.EmailAddress);
        inserted.EndPoint.Should().Be(new EndPoint("127.0.0.1", 8080));
        inserted.FloatValue.Should().Be(123.45f);
        inserted.GuidValue.Should().Be(typicalClass.GuidValue);
        inserted.IntValue.Should().Be(42);
        inserted.PhoneNumber.Should().Be(PhoneNumber.Parse("(234) 567-8901 ext 123"));
        inserted.StringValue.Should().Be("Hello, World!");
    }
}
