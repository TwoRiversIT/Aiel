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

namespace Aiel.Domain.Contacts;

/// <summary>
/// Defines a contract for validating email addresses.
/// </summary>
public interface IEmailValidator
{
    /// <summary>
    /// Determines if the <paramref name="email"/> is a valid format.
    /// </summary>
    /// <returns><c>true</c> if the email is in a valid format; otherwise, <c>false</c>.</returns>
    /// <param name="email">An email address.</param>
    Boolean IsValid(String? email);

    /// <summary>
    /// Determines if the <paramref name="email"/> is a valid format.
    /// </summary>
    /// <param name="email">An email address.</param>
    /// <returns><c>true</c> if the email is in a valid format; otherwise, <c>false</c>.</returns>
    Boolean IsValid(Email? email);

    /// <summary>
    /// Determines if the <see cref="EmailAddress.Name"/> and <see cref="EmailAddress.Email"/> are valid.
    /// </summary>
    /// <returns><c>true</c> if the <see cref="EmailAddress.Name"/> is not null or whitespace and <see cref="EmailAddress.Email"/> is in a valid format; otherwise, <c>false</c>.</returns>
    /// <param name="emailAddress">An email address.</param>
    Boolean IsValid(EmailAddress? emailAddress);
}
