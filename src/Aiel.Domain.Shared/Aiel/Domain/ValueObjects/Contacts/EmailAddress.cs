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

using System.Net.Mail;

namespace Aiel.Domain.ValueObjects.Contacts;

/// <summary>
/// Represents an email address with an optional display name.
/// </summary>
public readonly record struct EmailAddress : IEquatable<String>
{
    /// <summary>
    /// Gets an empty <see cref="EmailAddress"/> instance.
    /// </summary>
    public static readonly EmailAddress Empty = new(String.Empty, Email.Empty);

    private static readonly Char[] AngleBrackets = ['<', '>'];

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAddress"/> class with the specified display name and email address.
    /// </summary>
    /// <param name="name">The display name associated with the email address.</param>
    /// <param name="email">The email address.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> is null.</exception>
    /// <exception cref="FormatException">Thrown when the provided string is not a valid email address format.</exception>
    private EmailAddress(String name, Email email)
    {
        ArgumentNullException.ThrowIfNull(name);

        Name = name.Trim();
        Email = email;
    }

    /// <summary>
    /// Gets the display name associated with the email address. If no name is provided, returns an empty string.
    /// </summary>
    public String Name
    {
        get => String.IsNullOrWhiteSpace(field) ? String.Empty : field;
        init => field = value is null ? String.Empty : value.Trim();
    }

    /// <summary>
    /// Gets the email address.
    /// </summary>
    public Email Email { get; init; }

    /// <summary>
    /// Returns a string representation of the email address in the format "Name &lt;Email&gt;" if a name is provided, or just the email address if no name is provided.
    /// </summary>
    /// <returns>A string representation of the email address.</returns>
    public override String ToString()
        => String.IsNullOrWhiteSpace(Name)
            ? Email
            : $"{Name} <{Email}>";

    /// <summary>
    /// Creates a new <see cref="EmailAddress"/> instance from the specified
    /// <paramref name="displayName"/> and <paramref name="emailAddress"/>
    /// string if they are both valid; otherwise returns <see cref="Empty"/>.
    /// </summary>
    /// <param name="displayName">The display name associated with the email address.</param>
    /// <param name="emailAddress">The email address string.</param>
    /// <returns>A new <see cref="EmailAddress"/> instance.</returns>
    public static EmailAddress From(String displayName, String emailAddress)
    {
        if (String.IsNullOrWhiteSpace(displayName) || String.IsNullOrWhiteSpace(emailAddress))
        {
            return Empty;
        }

        if (Email.TryParse(emailAddress, out var email))
        {
            return new(displayName.Trim(), email);
        }

        return Empty;
    }

    /// <summary>
    /// Returns a new <see cref="EmailAddress"/> from <paramref name="value"/> if it is in the form of "<c>John Doe &lt;john.doe@example.com&gt;</c>"; otherwise <see cref="Empty"/>.
    /// </summary>
    /// <param name="value">The string representation of the display name and email address.</param>
    /// <returns>A new <see cref="EmailAddress"/> instance.</returns>
    public static EmailAddress From(String value)
    {
        if (TryParse(value, out var emailAddress))
        {
            return emailAddress;
        }

        return Empty;
    }

    /// <summary>
    /// Parses the provided string representation of an email address and returns an <see cref="EmailAddress"/> instance.
    /// </summary>
    /// <param name="emailAddress">The string representation of the email address to parse.</param>
    /// <returns>An <see cref="EmailAddress"/> instance.</returns>
    public static EmailAddress Parse(String emailAddress)
    {
        if (TryParse(emailAddress, out var result))
        {
            return result;
        }

        throw new FormatException($"Invalid format. Cannot parse a Name and Email from: {emailAddress}");
    }

    /// <summary>
    /// Attempts to parse the provided string representation of an email address
    /// and returns a boolean indicating whether the parsing was successful. If
    /// successful, the parsed <see cref="EmailAddress"/> instance is returned
    /// in the <paramref name="emailAddress"/> parameter; otherwise,
    /// <paramref name="emailAddress"/> is set to <see cref="Empty"/>.
    /// </summary>
    /// <param name="value">The string representation of the email address to parse.</param>
    /// <param name="emailAddress">When this method returns, contains the parsed <see cref="EmailAddress"/> instance if the parsing was successful; otherwise, <see cref="Empty"/>.</param>
    /// <returns><c>true</c> if the parsing was successful; otherwise, <c>false</c>.</returns>
    public static Boolean TryParse(String value, out EmailAddress emailAddress)
    {
        if (!String.IsNullOrWhiteSpace(value))
        {
            var parts = value.Split(AngleBrackets, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                var name = parts[0].Trim();
                if (Email.TryParse(parts[1].Trim(), out var email))
                {
                    emailAddress = new EmailAddress(name, email);
                    return true;
                }
            }
        }

        emailAddress = Empty;
        return false;
    }

    /// <summary>
    /// Indicates whether the current <see cref="EmailAddress"/> instance is
    /// equal to the string representation of an email address. The comparison
    /// is case-insensitive for the domain part of the email address and 
    /// case-sensitive for the local part.
    /// </summary>
    /// <param name="other">The string representation of the email address to compare with the current instance.</param>
    /// <returns><c>true</c> if the current instance is equal to the specified string; otherwise, <c>false</c>.</returns>
    public Boolean Equals(String? other)
    {
        if (other is null)
        {
            return false;
        }

        if (TryParse(other, out var emailAddress))
        {
            return Equals(emailAddress);
        }

        return false;
    }

    /// <summary>
    /// Defines an explicit conversion from <see cref="String"/> to
    /// <see cref="EmailAddress"/>, allowing a string to be used wherever an
    /// <see cref="EmailAddress"/> instance is expected. The conversion parses
    /// the string into an <see cref="EmailAddress"/> instance.
    /// </summary>
    /// <param name="emailAddress">The string representation of the email address to convert.</param>
    public static explicit operator EmailAddress(String emailAddress) => Parse(emailAddress);

    /// <summary>
    /// Defines an implicit conversion from <see cref="EmailAddress"/> to <see cref="String"/>, allowing an <see cref="EmailAddress"/> instance to be used wherever a string is expected. The conversion returns the string representation of the email address.
    /// </summary>
    /// <param name="emailAddress">The <see cref="EmailAddress"/> instance to convert.</param>
    public static implicit operator String(EmailAddress emailAddress) => emailAddress.ToString();

    /// <summary>
    /// Defines an implicit conversion from <see cref="EmailAddress"/> to <see cref="MailAddress"/>, allowing an <see cref="EmailAddress"/> instance to be used wherever a <see cref="MailAddress"/> instance is expected.
    /// </summary>
    /// <param name="emailAddress">The <see cref="EmailAddress"/> instance to convert.</param>
    public static implicit operator MailAddress(EmailAddress emailAddress) => new(emailAddress.Email, emailAddress.Name);

    /// <summary>
    /// Defines an implicit conversion from <see cref="EmailAddress"/> to <see cref="Contacts.Email"/>, allowing an <see cref="EmailAddress"/> instance to be used wherever an <see cref="Contacts.Email"/> instance is expected.
    /// </summary>
    /// <param name="emailAddress">The <see cref="EmailAddress"/> instance to convert.</param>
    public static implicit operator Email(EmailAddress emailAddress) => Parse(emailAddress.Email);
}
