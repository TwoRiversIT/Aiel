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

using Aiel.Domain;
using Aiel.Domain.Contacts;
using Aiel.StrongIds;
using System.Text.Json.Serialization;

namespace Aiel.Testing.Models;

[StrongId<Guid>(AllowDefault = true)]
public readonly partial record struct PersonId;

public sealed class Person : Entity<PersonId>
{
    [JsonConstructor]
    private Person(PersonId id, String firstName, String lastName, String middleName = "", Gender gender = Gender.Other, DateOnly? dateOfBirth = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName, nameof(firstName));
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName, nameof(lastName));

        Id = id;
        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName?.Trim() ?? String.Empty;
        DateOfBirth = dateOfBirth;
        Gender = gender;
    }

    public String FirstName { get; private set; }
    public String LastName { get; private set; }
    public String MiddleName { get; private set; }
    public DateOnly? DateOfBirth { get; private set; }
    public Gender Gender { get; private set; } = Gender.NonBinary;

    public String Initials
        => $"{(FirstName.Length > 0 ? FirstName[0] : ' ')}{(LastName.Length > 0 ? LastName[0] : ' ')}"
            .Trim()
            .ToUpperInvariant();

    public static Person Create(PersonId id, String firstName, String lastName, String middleName, DateOnly? dateOfBirth, Gender gender)
        => new(id, firstName, lastName, middleName, gender, dateOfBirth);
}

public record PersonDto(PersonId Id, String FirstName, String LastName, String MiddleName, DateOnly DateOfBirth, Gender Gender);
