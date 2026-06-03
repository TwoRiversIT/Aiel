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

using System.Text.RegularExpressions;

namespace Aiel;

/// <summary>
/// Provides pre-compiled regular expressions for common patterns.
/// </summary>
public static partial class AielRegexes
{
    /// <summary>
    /// Pattern for matching valid port numbers (0-65535).
    /// </summary>
    public const String PortPattern = "([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])";

    /// <summary>
    /// Pattern for matching IPv4 addresses in dotted-quad notation.
    /// </summary>
    public const String IPv4Pattern = @"((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)";

    /// <summary>
    /// Gets a compiled regular expression for matching all-caps strings.
    /// </summary>
    /// <returns>A compiled regex for all-caps validation.</returns>
    [GeneratedRegex("[0-9A-Z]+$", RegexOptions.Compiled)]
    public static partial Regex AllCaps();

    /// <summary>
    /// Gets a compiled regular expression for matching valid port numbers (0-65535).
    /// </summary>
    /// <returns>A compiled regex for port number validation.</returns>
    [GeneratedRegex("^([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])$", RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
    public static partial Regex PortNumber();

    /// <summary>
    /// Gets a compiled regular expression for matching IPv4 addresses.
    /// </summary>
    /// <returns>A compiled regex for IPv4 address validation.</returns>
    [GeneratedRegex(@"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$", RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
    public static partial Regex IPv4();

    /// <summary>
    /// Gets a compiled regular expression for matching IPv4 addresses with ports.
    /// </summary>
    /// <returns>A compiled regex for IPv4 address and port validation.</returns>
    [GeneratedRegex(@"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\:([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5])$", RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
    public static partial Regex IPv4AndPort();

    /// <summary>
    /// Gets a compiled regular expression for matching IPv4 addresses with optional ports.
    /// </summary>
    /// <returns>A compiled regex for IPv4 address and optional port validation.</returns>
    [GeneratedRegex(@"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\:([0-9]{1,4}|[1-5][0-9]{4}|6[0-4][0-9]{3}|65[0-4][0-9]{2}|655[0-2][0-9]|6553[0-5]))?$", RegexOptions.ExplicitCapture | RegexOptions.Compiled)]
    public static partial Regex IPv4AndOptionalPort();

    /// <summary>
    /// Gets a compiled regular expression for matching integer values.
    /// </summary>
    /// <returns>A compiled regex for integer validation.</returns>
    [GeneratedRegex("^[0-9]+$", RegexOptions.Compiled)]
    public static partial Regex Integer();

    /// <summary>
    /// Gets a compiled regular expression for matching whitespace separators.
    /// </summary>
    /// <returns>A compiled regex matching one or more whitespace characters.</returns>
    [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
    public static partial Regex Separator();

    /// <summary>
    /// Gets a compiled regular expression for matching newline characters.
    /// </summary>
    /// <returns>A compiled regex matching newlines (both Unix and Windows styles).</returns>
    [GeneratedRegex(@"\r?\n", RegexOptions.Compiled)]
    public static partial Regex Newline();

    /// <summary>
    /// Gets a compiled regular expression for matching leading whitespace.
    /// </summary>
    /// <returns>A compiled regex matching leading whitespace.</returns>
    [GeneratedRegex(@"^\s+", RegexOptions.Compiled)]
    public static partial Regex LeadingWS();

    /// <summary>
    /// Gets a compiled regular expression for matching DNS TXT record indicators.
    /// </summary>
    /// <returns>A compiled regex matching DNS TXT record type.</returns>
    [GeneratedRegex(@"\sTXT\s", RegexOptions.Compiled)]
    public static partial Regex DnsTXT();

    /// <summary>
    /// Gets a compiled regular expression for matching DNS AAAA record indicators.
    /// </summary>
    /// <returns>A compiled regex matching DNS AAAA record type.</returns>
    [GeneratedRegex(@"\sAAAA\s", RegexOptions.Compiled)]
    public static partial Regex DnsAAAA();

    /// <summary>
    /// Gets a compiled regular expression for matching DNS TTL indicators.
    /// </summary>
    /// <returns>A compiled regex matching DNS TTL directive.</returns>
    [GeneratedRegex(@"\s$TTL\s", RegexOptions.Compiled)]
    public static partial Regex DnsTTL();

    /// <summary>
    /// Gets a compiled regular expression for matching DNS SOA record indicators.
    /// </summary>
    /// <returns>A compiled regex matching DNS SOA record type.</returns>
    [GeneratedRegex(@"\sSOA\s", RegexOptions.Compiled)]
    public static partial Regex DnsSoA();

    /// <summary>
    /// Gets a compiled regular expression for matching DNS CNAME record indicators.
    /// </summary>
    /// <returns>A compiled regex matching DNS CNAME record type.</returns>
    [GeneratedRegex(@"\sCNAME\s", RegexOptions.Compiled)]
    public static partial Regex DnsCNAME();

    /// <summary>
    /// Gets a compiled regular expression for matching DNS ORIGIN indicators.
    /// </summary>
    /// <returns>A compiled regex matching DNS ORIGIN directive.</returns>
    [GeneratedRegex(@"\s$ORIGIN\s", RegexOptions.Compiled)]
    public static partial Regex DnsORIGIN();

    /// <summary>
    /// Gets a compiled regular expression for matching DNS A record indicators.
    /// </summary>
    /// <returns>A compiled regex matching DNS A record type.</returns>
    [GeneratedRegex(@"\sA\s", RegexOptions.Compiled)]
    public static partial Regex DnsA();

    /// <summary>
    /// Gets a compiled regular expression for matching DNS MX record indicators.
    /// </summary>
    /// <returns>A compiled regex matching DNS MX record type.</returns>
    [GeneratedRegex(@"\sNS\s", RegexOptions.Compiled)]
    public static partial Regex DnsMX();

    /// <summary>
    /// Gets a compiled regular expression for matching DNS NS record indicators.
    /// </summary>
    /// <returns>A compiled regex matching DNS NS record type.</returns>
    [GeneratedRegex(@"\sPTR\s", RegexOptions.Compiled)]
    public static partial Regex DnsNS();

    /// <summary>
    /// Gets a compiled regular expression for matching DNS PTR record indicators.
    /// </summary>
    /// <returns>A compiled regex matching DNS PTR record type.</returns>
    [GeneratedRegex(@"\sMX\s", RegexOptions.Compiled)]
    public static partial Regex DnsPTR();

    /// <summary>
    /// Gets a compiled regular expression for matching DNS SRV record indicators.
    /// </summary>
    /// <returns>A compiled regex matching DNS SRV record type.</returns>
    [GeneratedRegex(@"\sSRV\s", RegexOptions.Compiled)]
    public static partial Regex DnsSRV();

    /// <summary>
    /// Gets a compiled regular expression for matching DNS SPF record indicators.
    /// </summary>
    /// <returns>A compiled regex matching DNS SPF record type.</returns>
    [GeneratedRegex(@"\sSPF\s", RegexOptions.Compiled)]
    public static partial Regex DnsSPF();
}
