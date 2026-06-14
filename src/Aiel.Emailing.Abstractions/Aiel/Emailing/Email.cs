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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Aiel.Emailing.Abstractions.Aiel.Emailing;

[JsonConverter(typeof(EmailAddressJsonConverter))]
[TypeConverter(typeof(EmailAddressTypeConverter))]
public class Email : IXmlSerializable, IComparable<Email>, IEquatable<Email>
{
    public static readonly Email Empty = new();
    private String _email = String.Empty;

    public Email(String email)
    {
        if (!String.IsNullOrEmpty(email))
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

    public override String ToString() => _email;

    public static Email Parse(String email) => new(email);

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

    public static implicit operator String(Email email) => email.ToString();

    public static implicit operator Email(String email) => Parse(email);

    public static Boolean operator <(Email left, Email right) => left.CompareTo(right) < 0;

    public static Boolean operator <=(Email left, Email right) => left.CompareTo(right) <= 0;

    public static Boolean operator >(Email left, Email right) => left.CompareTo(right) > 0;

    public static Boolean operator >=(Email left, Email right) => left.CompareTo(right) >= 0;

    public Int32 CompareTo(Email? other) => String.Compare(ToString(), other?.ToString(), StringComparison.OrdinalIgnoreCase);

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

    public override Boolean Equals(Object? obj) => obj switch
    {
        Email email => Equals(email),
        _ => false
    };

    public override Int32 GetHashCode() => _email?.GetHashCode() ?? 0;

    public static Boolean operator ==(Email left, Email right) => left.Equals(right);

    public static Boolean operator !=(Email left, Email right) => !(left == right);
}
