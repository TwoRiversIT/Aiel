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

using System.Net;

namespace Aiel.Internet;

public class DomainName
{
    public static readonly DomainName Empty = new();

    private readonly String _domain;

    private DomainName() { _domain = String.Empty; }

    public DomainName(String name)
    {
        IsValid(name, throwIfInvalid: true);

        _domain = Normalize(name);
    }

    private static String Normalize(String domain)
        => domain[^1] == '.' ? domain[..^1] : domain;

    public override Int32 GetHashCode() => _domain?.GetHashCode() ?? 0;

    public override String ToString() => _domain;

    public Int32 CompareTo(DomainName other)
        => other is null ? 1 : String.Compare(_domain, other._domain, StringComparison.InvariantCultureIgnoreCase);

    public Boolean Equals(DomainName other)
        => other is not null && _domain.Equals(other._domain, StringComparison.InvariantCultureIgnoreCase);

    public override Boolean Equals(Object? other)
        => other is not default(Object) && other is DomainName domainName && Equals(domainName);

    public static implicit operator DomainName(String s) => new(s);

    /// <summary>
    /// Parses a domain name string into a <see cref="DomainName" /> instance.
    /// </summary>
    /// <param name="domain">The domain name to parse.</param>
    /// <returns>A <see cref="DomainName" /> instance.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="domain" /> is not a valid domain name.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="domain" /> is <see langword="null" />.</exception>
    public static DomainName Parse(String domain) => new(domain);

    /// <summary>
    /// Attempts to parse a domain name string into a <see cref="DomainName" /> instance.
    /// </summary>
    /// <param name="domain">The domain name to parse.</param>
    /// <param name="domainName">The parsed <see cref="DomainName" /> when the method returns <see langword="true" />.</param>
    /// <returns><see langword="true" /> when the domain is valid; otherwise, <see langword="false" />.</returns>
    public static Boolean TryParse(String domain, out DomainName domainName)
    {
        if (!IsValid(domain))
        {
            domainName = Empty;
            return false;
        }

        domainName = new DomainName(domain);
        return true;
    }

    public static DomainName FromString(String s) => new(s);

    public static implicit operator String(DomainName domainName) => domainName._domain;

    public static Boolean operator !=(DomainName a, DomainName b) => !(a == b);

    public static Boolean operator ==(DomainName a, DomainName b)
    {
        // If both are null, or both are same instance, return true.
        if (ReferenceEquals(a, b))
        {
            return true;
        }

        // If one is null, but not both, return false.
        if ((a is null) || (b is null))
        {
            return false;
        }

        // Return true if the fields match:
        return a._domain == b._domain;
    }

    public static Boolean IsValid(String domain, Boolean throwIfInvalid = false)
    {
        if (domain is null)
        {
            return throwIfInvalid ? throw new ArgumentNullException(nameof(domain)) : false;
        }

        if (String.IsNullOrWhiteSpace(domain))
        {
            return throwIfInvalid ? throw new ArgumentException($"Invalid DomainName ({domain}): Must not be Empty or Whitespace.", nameof(domain)) : false;
        }

        if (domain.Length > 255)
        {
            return throwIfInvalid
                ? throw new ArgumentException($"Invalid DomainName ({domain}): The total length of a domain must not exceed 255 octets. The '{domain}' domain is {domain.Length}.", nameof(domain))
                : false;
        }

        if (domain[0] == '.')
        {
            return throwIfInvalid
                ? throw new ArgumentException($"Invalid DomainName ({domain}): A domain name MUST NOT start with a period (.).", nameof(domain))
                : false;
        }

        if (IPAddress.TryParse(domain, out _))
        {
            return true;
        }

        var start = 0;
        var labels = 0;
        do
        {
            var end = domain.IndexOf('.', start);
            if (end == -1)
            {
                end = domain.Length;
            }

            if (end == start)
            {
                return throwIfInvalid
                    ? throw new ArgumentException($"Invalid DomainName ({domain}): A domain must not contain two consecutive periods (..)", nameof(domain))
                    : false;
            }

            labels++;
            //var label = domain[start..end];
            if (end - start > 63)
            {
                return throwIfInvalid
                    ? throw new ArgumentException($"Invalid DomainName ({domain}): The length of any one label is limited to between 1 and 63 octets. '{domain[start..end]}' is {end - start} octets.", nameof(domain))
                    : false;
            }

            start = end + 1;
        } while (start < domain.Length);

        if (labels < 2)
        {
            return throwIfInvalid
                ? throw new ArgumentException($"Invalid DomainName ({domain}): A domain name must consist of two or more lables separated by a period (.)", nameof(domain))
                : false;
        }

        return true;
    }
}
