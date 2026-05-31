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

namespace Aiel.Authorization;

internal static class PermissionTextValidator
{
    private const Char QualifiedNameSeparator = '.';

    internal static Boolean TryCreatePermissionName(String? value, out String normalizedValue)
    {
        normalizedValue = String.Empty;

        if (!TryNormalizeRequired(value, out var trimmedValue) || ContainsWhitespace(trimmedValue))
        {
            return false;
        }

        var segments = trimmedValue.Split(QualifiedNameSeparator, StringSplitOptions.None);

        if (segments.Length == 0)
        {
            return false;
        }

        foreach (var segment in segments)
        {
            if (!IsIdentifier(segment))
            {
                return false;
            }
        }

        normalizedValue = trimmedValue;
        return true;
    }

    internal static Boolean TryCreateTypeName(String? value, out String normalizedValue)
    {
        normalizedValue = String.Empty;

        if (!TryNormalizeRequired(value, out var trimmedValue) || ContainsWhitespace(trimmedValue))
        {
            return false;
        }

        if (trimmedValue.Contains(QualifiedNameSeparator) || !IsIdentifier(trimmedValue))
        {
            return false;
        }

        normalizedValue = trimmedValue;
        return true;
    }

    internal static Boolean TryCreateKey(String? value, out String normalizedValue)
        => TryNormalizeRequired(value, out normalizedValue);

    private static Boolean TryNormalizeRequired(String? value, out String normalizedValue)
    {
        normalizedValue = String.Empty;

        if (String.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        normalizedValue = value.Trim();
        return normalizedValue.Length > 0;
    }

    private static Boolean ContainsWhitespace(String value)
    {
        foreach (var character in value)
        {
            if (Char.IsWhiteSpace(character))
            {
                return true;
            }
        }

        return false;
    }

    private static Boolean IsIdentifier(String value)
    {
        if (value.Length == 0)
        {
            return false;
        }

        foreach (var character in value)
        {
            if (!Char.IsLetterOrDigit(character) && character != '_')
            {
                return false;
            }
        }

        return true;
    }
}
