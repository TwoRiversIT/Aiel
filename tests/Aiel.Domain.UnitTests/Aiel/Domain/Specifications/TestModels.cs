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
using Aiel.Testing.Models;

namespace Aiel.Domain.Specifications;

public class NullSpecification : ExpressionSpecification<String>
{
    public NullSpecification() : base(_ => false) { }
}

public class IsEven : ExpressionSpecification<Int32>
{
    public IsEven() : base(n => n % 2 == 0) { }
}

public class IsMultiple(Int32 factor) : ExpressionSpecification<Int32>(n => n % factor == 0)
{
}

public sealed class ZeroToNine : ExpressionSpecification<Int32>
{
    public ZeroToNine() : base()
    {
        PredicateExpression = n => n >= 0 && n < 10;
    }
}

public class HasGender(Gender gender) : EntitySpecification<Person>(user => (gender & user.Gender) != 0)
{
}

public class IsAgeOfMajority(DateOnly date, Int32 age = 18)
    : EntitySpecification<Person>(user => user.DateOfBirth <= date.AddYears(-age))
{
}
