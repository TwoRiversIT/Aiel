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

using Riok.Mapperly.Abstractions;

namespace Aiel.Testing.Dummies;

[Mapper]
public static partial class DummyMapper
{
    /// <summary>
    /// Maps an <see cref="Person"/> to an <see cref="PersonDto"/>.
    /// </summary>
    /// <param name="person">The person to map.</param>
    /// <returns>The mapped person DTO.</returns>
    [MapperIgnoreSource(nameof(Person.Version))]
    public static partial PersonDto ToDto(this Person person);

    /// <summary>
    /// Maps a collection of <see cref="Person"/> objects to a read-only list of <see cref="PersonDto"/> objects.
    /// </summary>
    /// <param name="people">The collection of people to map.</param>
    /// <returns>The mapped read-only list of person DTOs.</returns>
    public static partial IReadOnlyCollection<PersonDto> ToListDto(this IEnumerable<Person> people);

    public static partial CustomerDto ToDto(this Customer customer);
    public static partial Customer ToEntity(this CreateCustomerCommand dto);
}
