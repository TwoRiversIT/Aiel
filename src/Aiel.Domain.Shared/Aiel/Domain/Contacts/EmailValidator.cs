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
/// Provides a static class for validating email addresses using a pluggable validator implementation.
/// </summary>
public static class EmailValidator
{
    /// <summary>
    /// Gets the current email validator instance. The default implementation is <see cref="PatternEmailValidator"/>.
    /// </summary>
    public static IEmailValidator Instance { get; private set; } = new PatternEmailValidator();

    /// <summary>
    /// Determines whether the specified email string is valid.
    /// </summary>
    /// <param name="email">The email string to validate.</param>
    /// <returns><c>true</c> if the email is valid; otherwise, <c>false</c>.</returns>
    public static Boolean IsValid(String? email) => Instance.IsValid(email);

    /// <summary>
    /// Determines whether the specified <see cref="EmailAddress"/> is valid.
    /// </summary>
    /// <param name="emailAddress">The <see cref="EmailAddress"/> to validate.</param>
    /// <returns><c>true</c> if the <see cref="EmailAddress"/> is valid; otherwise, <c>false</c>.</returns>
    public static Boolean IsValid(EmailAddress? emailAddress) => Instance.IsValid(emailAddress);

    /// <summary>
    /// Sets the email validator instance.
    /// </summary>
    /// <param name="validator">The email validator to set.</param>
    public static void SetValidator(IEmailValidator validator)
    {
        ArgumentNullException.ThrowIfNull(validator);

        Instance = validator;
    }
}
