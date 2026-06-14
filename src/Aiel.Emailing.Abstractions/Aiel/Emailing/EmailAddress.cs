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

namespace Aiel.Emailing.Abstractions.Aiel.Emailing;

public class EmailAddress
{
    public static readonly EmailAddress Empty = new();

    private static readonly Char[] AngleBrackets = ['<', '>'];

    private readonly String? _name;

    private EmailAddress()
    {
        _name = String.Empty;
        Email = Email.Empty;
    }

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

    public EmailAddress(String name, Email email)
    {
        ArgumentNullException.ThrowIfNull(name);

        _name = name.Trim();
        Email = email;
    }

    public String Name
    {
        get => _name ?? String.Empty;
        init => _name = value;
    }

    public Email Email { get; init; }

    public override String ToString()
        => String.IsNullOrWhiteSpace(_name)
            ? Email
            : $"{_name} <{Email}>";

    public static EmailAddress Parse(String emailAddress) => new(emailAddress);

    public static implicit operator String(EmailAddress emailAddress) => emailAddress.ToString();

    public static implicit operator EmailAddress(String emailAddress) => Parse(emailAddress);

    public static implicit operator MailAddress(EmailAddress emailAddress) => new(emailAddress.Email, emailAddress.Name);
}
