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

using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Aiel.Domain.Contacts;

/// <summary>
/// Represents an email address with validation and comparison capabilities.
/// </summary>
[JsonConverter(typeof(EmailAddressJsonConverter))]
[TypeConverter(typeof(EmailAddressTypeConverter))]
public class Email : IXmlSerializable, IComparable<Email>, IEquatable<Email>
{
    /// <summary>
    /// Gets a singleton instance of an empty email address. This can be used to represent an uninitialized or default email address.
    /// </summary>
    public static readonly Email Empty = new();
    private String _email = String.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="Email"/> class with the specified email address.
    /// </summary>
    /// <param name="email">The email address to initialize the instance with.</param>
    /// <exception cref="ArgumentException">Thrown when the provided email is not valid.</exception>
    public Email(String email)
    {
        if (!String.IsNullOrWhiteSpace(email))
        {
            var parts = email.Split('@', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length != 2)
            {
                throw new ArgumentException($"The string '{email}' is not a valid email.", nameof(email));
            }

            _email = email;
        }
    }

    private Email() { _email = String.Empty; }

    /// <summary>
    /// Returns the string representation of the email address.
    /// </summary>
    /// <returns></returns>
    public override String ToString() => _email;

    /// <summary>
    /// Parses the specified string into an <see cref="Email"/> instance. If the string is not a valid email address, an <see cref="ArgumentException"/> is thrown.
    /// </summary>
    /// <param name="email">The string representation of the email address to parse.</param>
    /// <returns>The parsed <see cref="Email"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided string is not a valid email address.</exception>
    public static Email Parse(String? email)
    {
        if (TryParse(email, out var result))
        {
            return result;
        }

        throw new ArgumentException($"The string '{email}' is not a valid email.", nameof(email));
    }

    /// <summary>
    /// Attempts to parse the specified string into an <see cref="Email"/> instance. Returns true if the parsing was successful; otherwise, false.
    /// </summary>
    /// <param name="value">The string representation of the email address to parse.</param>
    /// <param name="email">When this method returns, contains the parsed <see cref="Email"/> instance if the parsing was successful; otherwise, <see cref="Email.Empty"/>.</param>
    /// <returns><c>true</c> if the parsing was successful; otherwise, <c>false</c>.</returns>
    public static Boolean TryParse(String? value, out Email email)
    {
        email = Empty;
        try
        {
            email = new Email(value!);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Defines an implicit conversion from <see cref="Email"/> to <see cref="String"/>. This allows an <see cref="Email"/> instance to be used wherever a string is expected, automatically converting it to its string representation.
    /// </summary>
    /// <param name="email">The <see cref="Email"/> instance to convert to a string.</param>
    public static implicit operator String(Email email) => email.ToString();

    /// <summary>
    /// Defines an implicit conversion from <see cref="String"/> to <see cref="Email"/>. This allows a string to be used wherever an <see cref="Email"/> instance is expected, automatically converting it to an <see cref="Email"/> instance.
    /// </summary>
    /// <param name="email">The string representation of the email address to convert to an <see cref="Email"/> instance.</param>
    public static implicit operator Email(String? email) => email is null ? Empty : Parse(email);

    /// <inheritdoc />
    public static Boolean operator <(Email left, Email right) => left.CompareTo(right) < 0;

    /// <inheritdoc />
    public static Boolean operator <=(Email left, Email right) => left.CompareTo(right) <= 0;

    /// <inheritdoc />
    public static Boolean operator >(Email left, Email right) => left.CompareTo(right) > 0;

    /// <inheritdoc />
    public static Boolean operator >=(Email left, Email right) => left.CompareTo(right) >= 0;

    /// <summary>
    /// Compares the current <see cref="Email"/> instance with another <see cref="Email"/> instance and returns an integer that indicates their relative order. The comparison is case-insensitive and based on the string representations of the email addresses.
    /// </summary>
    /// <param name="other">The other <see cref="Email"/> instance to compare with the current instance.</param>
    /// <returns>A signed integer that indicates the relative order of the instances being compared. Less than zero if the current instance is less than <paramref name="other"/>, zero if they are equal, and greater than zero if the current instance is greater than <paramref name="other"/>.</returns>
    public Int32 CompareTo(Email? other) => String.Compare(ToString(), other?.ToString(), StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Determines whether the current <see cref="Email"/> instance is equal to another <see cref="Email"/> instance. The comparison is case-insensitive and based on the string representations of the email addresses.
    /// </summary>
    /// <param name="other">The other <see cref="Email"/> instance to compare with the current instance.</param>
    /// <returns><c>true</c> if the current instance is equal to the <paramref name="other"/> instance; otherwise, <c>false</c>.</returns>
    public Boolean Equals(Email? other)
    {
        if (other is null)
        {
            return false;
        }

        if (String.IsNullOrWhiteSpace(_email))
        {
            return String.IsNullOrWhiteSpace(other._email);
        }

        var myParts = _email.Split('@', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var otherParts = other._email.Split('@', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (otherParts.Length != 2 || myParts.Length != otherParts.Length)
        {
            return false;
        }

        return String.Equals(myParts[0], otherParts[0], StringComparison.Ordinal)
            && String.Equals(myParts[1], otherParts[1], StringComparison.OrdinalIgnoreCase);
    }

    XmlSchema? IXmlSerializable.GetSchema() => null;

    void IXmlSerializable.ReadXml(XmlReader reader)
    {
        if (reader.ReadToDescendant("Email"))
        {
            _email = reader.ReadElementContentAsString();
        }
    }

    void IXmlSerializable.WriteXml(XmlWriter writer)
        => writer.WriteElementString("Email", _email);

    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "See the JsonConverter attribute.")]
    private sealed class EmailAddressJsonConverter : JsonConverter<Email>
    {
        public override Boolean CanConvert(Type objectType)
            => objectType == typeof(Email);

        public override Email Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => new(reader.GetString() ?? String.Empty);

        public override void Write(Utf8JsonWriter writer, Email email, JsonSerializerOptions options)
            => writer.WriteStringValue(email);
    }

    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "See the TypeConverter attribute.")]
    private sealed class EmailAddressTypeConverter : TypeConverter
    {
        public override Boolean CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
            => sourceType == typeof(String) || base.CanConvertFrom(context, sourceType);

        public override Object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, Object value)
        {
            var email = value as String;

            return String.IsNullOrEmpty(email)
                ? base.ConvertFrom(context, culture, value)!
                : new Email(email);
        }
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="Email"/> instance. The comparison is case-insensitive and based on the string representations of the email addresses.
    /// </summary>
    /// <param name="obj">The object to compare with the current <see cref="Email"/> instance.</param>
    /// <returns><c>true</c> if the specified object is equal to the current <see cref="Email"/> instance; otherwise, <c>false</c>.</returns>
    public override Boolean Equals(Object? obj) => obj switch
    {
        Email email => Equals(email),
        _ => false
    };

    /// <inheritdoc />
    public override Int32 GetHashCode() => _email?.GetHashCode() ?? 0;

    /// <inheritdoc />
    public static Boolean operator ==(Email left, Email right) => left.Equals(right);

    /// <inheritdoc />
    public static Boolean operator !=(Email left, Email right) => !(left == right);
}
