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

using System.Diagnostics.CodeAnalysis;

namespace Aiel.Emailing;

public class EmailComparer(EmailComparerMode mode = EmailComparerMode.LocalDomain) : IComparer<Email>
{
    private static readonly String[] EmptyParts = [String.Empty, String.Empty];

    private readonly EmailComparerMode _mode = mode;

    private record struct Parts(Int32 Local, Int32 Domain);

    public Int32 Compare(Email? x, Email? y)
    {
        if (x is null)
        {
            if (y is not null)
            {
                return -1;
            }

            return 0;
        }

        if (y is null)
        {
            return 1;
        }

        if (x is Email xEmail)
        {
            if (y is Email yEmail)
            {
                return CompareInt(xEmail, yEmail);
            }

            return 1;
        }

        if (y is not null)
        {
            return 1;
        }

        throw new ArgumentException("Not an Email", nameof(y));
    }

    private Int32 CompareInt(Email x, Email y) => Evaluate(GetLocalAndDomain(x, y));

    private Int32 Evaluate(Parts parts) => _mode switch
    {
        EmailComparerMode.LocalDomain => parts.Local == 0 ? parts.Domain : parts.Local,
        EmailComparerMode.DomainLocal => parts.Domain == 0 ? parts.Local : parts.Domain,
        _ => throw new InvalidOperationException("Invalid email comparer mode."),
    };

    [SuppressMessage("Roslynator", "RCS1235:Optimize method call", Justification = "Local part is case sensitive")]
    private static Parts GetLocalAndDomain(Email x, Email y)
    {
        var xParts = Split(x);

        var yParts = Split(y);

        var local = String.Compare(xParts[0], yParts[0], StringComparison.Ordinal);
        var domain = String.Compare(xParts[1], yParts[1], StringComparison.OrdinalIgnoreCase);

        return new Parts(local, domain);
    }

    private static String[] Split(Email x)
    {
        var xParts = x.ToString().Split('@', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (xParts.Length != 2)
        {
            return EmptyParts;
        }

        return xParts;
    }
}
