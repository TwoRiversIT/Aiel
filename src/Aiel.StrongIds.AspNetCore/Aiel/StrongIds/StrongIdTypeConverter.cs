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
using System.Reflection;

namespace Aiel.StrongIds;

public sealed class StrongIdTypeConverter<TStrongId, TValue> : TypeConverter
    where TStrongId : IStrongId<TValue>
{
    private static readonly MethodInfo TryParseMethod = ResolveTryParseMethod();

    public override Boolean CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        => sourceType == typeof(String) || base.CanConvertFrom(context, sourceType);

    public override Boolean CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        => destinationType == typeof(String) || base.CanConvertTo(context, destinationType);

    public override Object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, Object value)
    {
        if (value is String stringValue)
        {
            var parameters = new Object?[] { stringValue, culture ?? CultureInfo.InvariantCulture, null };
            var success = (Boolean)TryParseMethod.Invoke(null, parameters)!;

            if (success)
            {
                return (TStrongId)parameters[2]!;
            }

            throw new FormatException($"'{stringValue}' is not a valid {typeof(TStrongId).Name}.");
        }

        return base.ConvertFrom(context, culture, value)!;
    }

    public override Object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, Object? value, Type destinationType)
    {
        if (destinationType == typeof(String) && value is TStrongId strongId)
        {
            return Convert.ToString(strongId.Value, culture ?? CultureInfo.InvariantCulture) ?? String.Empty;
        }

        return base.ConvertTo(context, culture, value, destinationType)!;
    }

    private static MethodInfo ResolveTryParseMethod()
    {
        var outParameterType = typeof(TStrongId).MakeByRefType();
        var method = typeof(TStrongId).GetMethod(
            "TryParse",
            BindingFlags.Public | BindingFlags.Static,
            binder: null,
            [typeof(String), typeof(IFormatProvider), outParameterType],
            modifiers: null);

        if (method is null || method.ReturnType != typeof(Boolean))
        {
            throw new InvalidOperationException($"Strong ID type '{typeof(TStrongId).FullName}' must expose a public static TryParse(string, IFormatProvider, out {typeof(TStrongId).Name}) method.");
        }

        return method;
    }
}
