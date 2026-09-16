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

using Aiel.Domain.ValueObjects.Net;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Aiel.Domain.ValueObjects.Contacts;

/// <summary>
/// Represents an email address with validation and comparison capabilities.
/// </summary>
[JsonConverter(typeof(EmailAddressJsonConverter))]
[TypeConverter(typeof(EmailAddressTypeConverter))]
public readonly record struct Email : IComparable<Email>, IEquatable<Email>
{
    /// <summary>
    /// Gets a singleton instance of an empty email address. This can be used to represent an uninitialized or default email address.
    /// </summary>
    public static readonly Email Empty = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="Email"/> struct with the specified email address string. The constructor validates the input and throws an exception if the email address is not in a valid format.
    /// </summary>
    /// <param name="input"></param>
    [JsonConstructor]
    public Email(String input)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(input);
        var parts = input.Trim().Split('@', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 2)
        {
            User = parts[0];
            Domain = new DomainName(parts[1].ToLower());
        }
        else
        {
            throw new ArgumentException($"The string '{input}' is not a valid email address.", nameof(input));
        }
    }

    /// <summary>
    /// Gets the user part of the email address (the part before the '@'
    /// symbol). This property is read-only and is set during the
    /// construction of the <see cref="Email"/> instance.
    /// </summary>
    public String User { get; }

    /// <summary>
    /// Gets the domain part of the email address (the part after the
    /// '@' symbol). This property is read-only and is set during the
    /// construction of the <see cref="Email"/> instance.
    /// </summary>
    public DomainName Domain { get; }

    /// <summary>
    /// Returns the string representation of the email address.
    /// </summary>
    /// <returns>The string representation of the email address.</returns>
    public override String ToString()
        => String.IsNullOrWhiteSpace(User) || Domain == DomainName.Empty
        ? String.Empty
        : $"{User}@{Domain}";

    /// <inheritdoc />
    public override Int32 GetHashCode() => HashCode.Combine(User, Domain);

    /// <inheritdoc />
    public Int32 CompareTo(Email other)
    {
        var domainComparison = String.Compare(Domain, other.Domain, StringComparison.OrdinalIgnoreCase);
        return domainComparison != 0 ? domainComparison : String.CompareOrdinal(User, other.User);
    }

    /// <summary>
    /// An alternative to <see cref="Parse"/>, this method creates an
    /// <see cref="Email"/> instance from the specified string. If the string
    /// is not a valid email address, an empty <see cref="Email"/> instance is
    /// returned.
    /// </summary>
    /// <param name="email">The string representation of the email address to create.</param>
    /// <returns>The created <see cref="Email"/> instance, or <see cref="Email.Empty"/> if the string is not a valid email address.</returns>
    public static Email From(String? email)
    {
        if (TryParse(email, out var result))
        {
            return result;
        }

        return Empty;
    }

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
    [UnconditionalSuppressMessage("Design", "CA1031:Do not catch general exception types",
        Justification = "Returns false to indicate failure.")]
    public static Boolean TryParse(String? value, out Email email)
    {
        if (!String.IsNullOrWhiteSpace(value))
        {
            var parts = value.Split('@', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length == 2)
            {
                email = new Email(value);
                return true;
            }
        }

        email = Empty;
        return false;
    }

    /// <summary>
    /// Defines an implicit conversion from <see cref="Email"/> to <see cref="String"/>. This allows an <see cref="Email"/> instance to be used wherever a string is expected, automatically converting it to its string representation.
    /// </summary>
    /// <param name="email">The <see cref="Email"/> instance to convert to a string.</param>
    public static implicit operator String(Email email) => email.ToString();

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
        if (other is not Email email)
        {
            return false;
        }

        return String.Equals(User, email.User, StringComparison.Ordinal)
            && String.Equals(Domain, email.Domain, StringComparison.OrdinalIgnoreCase);
    }

    [SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "See the JsonConverter attribute.")]
    private sealed class EmailAddressJsonConverter : JsonConverter<Email>
    {
        public override Boolean CanConvert(Type objectType)
            => objectType == typeof(Email);

        public override Email Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var input = reader.GetString();
            if (String.IsNullOrWhiteSpace(input) || !EmailValidator.IsValid(input))
            {
                return Empty;
            }

            return new(input);
        }

        public override void Write(Utf8JsonWriter writer, Email email, JsonSerializerOptions options)
            => writer.WriteStringValue(email.ToString());
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
}
