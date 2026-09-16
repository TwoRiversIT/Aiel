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

using Aiel.Domain.ValueObjects.Contacts;
using Aiel.StrongIds;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Aiel.Testing.Dummies;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasStrongIdConversion<PersonId, Guid>();
        builder.Property(x => x.FirstName).IsRequired();
        builder.Property(x => x.LastName).IsRequired();
        builder.HasData(
            Person.Create(PersonId.From(Guid.NewGuid()), "Doug", "Wilson", String.Empty, new DateOnly(1974, 10, 16), Gender.Male),
            Person.Create(PersonId.From(Guid.NewGuid()), "Shyloh", "Atlas", String.Empty, new DateOnly(2007, 10, 15), Gender.Female),
            Person.Create(PersonId.From(Guid.NewGuid()), "Piper", "Wilson", String.Empty, new DateOnly(2008, 5, 19), Gender.Female),
            Person.Create(PersonId.From(Guid.NewGuid()), "Geordi", "Wilson", String.Empty, new DateOnly(2011, 9, 14), Gender.Male)
        );
    }
}
