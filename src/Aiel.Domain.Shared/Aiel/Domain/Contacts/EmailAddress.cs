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

namespace Aiel.Domain.Contacts;

/// <summary>
/// Represents an email address with an optional display name.
/// </summary>
public class EmailAddress
{
    /// <summary>
    /// Gets an empty <see cref="EmailAddress"/> instance.
    /// </summary>
    public static readonly EmailAddress Empty = new();

    private static readonly Char[] AngleBrackets = ['<', '>'];

    private readonly String? _name;

    private EmailAddress()
    {
        _name = String.Empty;
        Email = Email.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAddress"/> class by parsing the provided email address string.
    /// </summary>
    /// <param name="emailAddress">The string representation of the email address to parse.</param>
    /// <exception cref="FormatException">Thrown when the provided string is not a valid email address format.</exception>
    public EmailAddress(String emailAddress)
    {
        var parts = emailAddress.Split(AngleBrackets, StringSplitOptions.RemoveEmptyEntries);
        _name = parts.Length switch
        {
            0 => String.Empty,
            1 => String.Empty,
            2 => parts[0].Trim(),
            _ => throw new FormatException("Invalid email address format."),
        };

        Email = Email.Parse(parts[^1].Trim());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAddress"/> class with the specified display name and email address.
    /// </summary>
    /// <param name="name">The display name associated with the email address.</param>
    /// <param name="email">The email address.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> is null.</exception>
    /// <exception cref="FormatException">Thrown when the provided string is not a valid email address format.</exception>
    public EmailAddress(String name, Email email) : this(email)
    {
        ArgumentNullException.ThrowIfNull(name);

        _name = name.Trim();
    }

    /// <summary>
    /// Gets the display name associated with the email address. If no name is provided, returns an empty string.
    /// </summary>
    public String Name
    {
        get => _name ?? String.Empty;
        init => _name = value;
    }

    /// <summary>
    /// Gets the email address.
    /// </summary>
    public Email Email { get; }

    /// <summary>
    /// Returns a string representation of the email address in the format "Name &lt;Email&gt;" if a name is provided, or just the email address if no name is provided.
    /// </summary>
    /// <returns>A string representation of the email address.</returns>
    public override String ToString()
        => String.IsNullOrWhiteSpace(_name)
            ? Email
            : $"{_name} <{Email}>";

    /// <summary>
    /// Parses the provided string representation of an email address and returns an <see cref="EmailAddress"/> instance.
    /// </summary>
    /// <param name="emailAddress">The string representation of the email address to parse.</param>
    /// <returns>An <see cref="EmailAddress"/> instance.</returns>
    public static EmailAddress Parse(String emailAddress) => new(emailAddress);

    /// <summary>
    /// Defines an implicit conversion from <see cref="String"/> to <see cref="EmailAddress"/>, allowing a string to be used wherever an <see cref="EmailAddress"/> instance is expected. The conversion parses the string into an <see cref="EmailAddress"/> instance.
    /// </summary>
    /// <param name="emailAddress">The string representation of the email address to convert.</param>
    public static implicit operator EmailAddress(String emailAddress) => Parse(emailAddress);

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
    public static implicit operator Email(EmailAddress emailAddress) => new(emailAddress.Email);
}
