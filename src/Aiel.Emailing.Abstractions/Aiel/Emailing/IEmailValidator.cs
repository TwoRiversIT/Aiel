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

namespace Aiel.Emailing.Abstractions.Aiel.Emailing;

public interface IEmailValidator
{
    /// <summary>
    /// Determines if the <paramref name="email"/> is a valid format.
    /// </summary>
    /// <remarks>
    /// <returns><c>true</c> if the email is in a valid format; otherwise, <c>false</c>.</returns>
    /// <param name="email">An email address.</param>
    Boolean IsValid(String email);

    /// <summary>
    /// Determines if the <paramref name="emailAddress"/> is a valid format.
    /// </summary>
    /// <remarks>
    /// <para>Validates the syntax of an email address.</para>
    /// <para>If <paramref name="allowTopLevelDomains"/> is <c>true</c>, then the validator will
    /// allow addresses with top-level domains like <c>postmaster@dk</c>.</para>
    /// <para>If <paramref name="allowInternational"/> is <c>true</c>, then the validator
    /// will use the newer International Email standards for validating the email address.</para>
    /// </remarks>
    /// <returns><c>true</c> if the name and email are valid; otherwise, <c>false</c>.</returns>
    /// <param name="emailAddress">An email address.</param>
    Boolean IsValid(EmailAddress emailAddress);
}
