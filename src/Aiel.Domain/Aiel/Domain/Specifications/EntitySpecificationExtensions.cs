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

namespace Aiel.Domain.Specifications;

/// <summary>
/// Provides extension methods for combining and manipulating entity specifications.
/// </summary>
public static class EntitySpecificationExtensions
{
    /// <summary>
    /// Combines two entity specifications using a logical AND operation.
    /// </summary>
    /// <typeparam name="T">The type of the entity to which the specifications apply.</typeparam>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A new specification that represents the logical AND of the two specifications.</returns>
    public static EntitySpecification<T> And<T>(this EntitySpecification<T> left, EntitySpecification<T> right) => left & right;

    /// <summary>
    /// Combines two entity specifications using a logical OR operation.
    /// </summary>
    /// <typeparam name="T">The type of the entity to which the specifications apply.</typeparam>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A new specification that represents the logical OR of the two specifications.</returns>
    public static EntitySpecification<T> Or<T>(this EntitySpecification<T> left, EntitySpecification<T> right) => left | right;

    /// <summary>
    /// Negates an entity specification using a logical NOT operation.
    /// </summary>
    /// <typeparam name="T">The type of the entity to which the specification applies.</typeparam>
    /// <param name="_">The specification to negate.</param>
    /// <param name="right">The specification to negate.</param>
    /// <returns>A new specification that represents the logical NOT of the specification.</returns>
    public static EntitySpecification<T> Not<T>(this EntitySpecification<T> _, EntitySpecification<T> right) => !right;
}
