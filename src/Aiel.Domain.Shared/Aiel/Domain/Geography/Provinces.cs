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

using System.Globalization;
using System.Text.RegularExpressions;

namespace Aiel.Domain.Geography;

/// <summary>
/// Provides a collection of Canadian provinces and methods for parsing and validating province codes and names.
/// </summary>
public static partial class Provinces
{
    /// <summary>
    /// Gets an array of all two letter Canadian province codes.
    /// </summary>
    public static readonly String[] Codes = ["AB", "BC", "MB", "NB", "NL", "NS", "ON", "PE", "QC", "SK", "NT", "NU", "YT"];

    /// <summary>
    /// Gets an enumerable collection of all Canadian provinces.
    /// </summary>
    public static IEnumerable<Province> All
    {
        get
        {
            yield return AB;
            yield return BC;
            yield return MB;
            yield return NB;
            yield return NL;
            yield return NS;
            yield return ON;
            yield return PE;
            yield return QC;
            yield return SK;
            yield return NT;
            yield return NU;
            yield return YT;
        }
    }

    /// <summary>
    /// Gets the province of Alberta.
    /// </summary>
    public static readonly Province AB = new("AB", "Alberta");

    /// <summary>
    /// Gets the province of British Columbia.
    /// </summary>
    public static readonly Province BC = new("BC", "British Columbia");

    /// <summary>
    /// Gets the province of Manitoba.
    /// </summary>
    public static readonly Province MB = new("MB", "Manitoba");

    /// <summary>
    /// Gets the province of New Brunswick.
    /// </summary>
    public static readonly Province NB = new("NB", "New Brunswick");

    /// <summary>
    /// Gets the province of Newfoundland and Labrador.
    /// </summary>
    public static readonly Province NL = new("NL", "Newfoundland and Labrador");

    /// <summary>
    /// Gets the province of Nova Scotia.
    /// </summary>
    public static readonly Province NS = new("NS", "Nova Scotia");

    /// <summary>
    /// Gets the province of Ontario.
    /// </summary>
    public static readonly Province ON = new("ON", "Ontario");

    /// <summary>
    /// Gets the province of Prince Edward Island.
    /// </summary>
    public static readonly Province PE = new("PE", "Prince Edward Island");

    /// <summary>
    /// Gets the province of Québec.
    /// </summary>
    public static readonly Province QC = new("QC", "Québec");

    /// <summary>
    /// Gets the province of Saskatchewan.
    /// </summary>
    public static readonly Province SK = new("SK", "Saskatchewan");

    /// <summary>
    /// Gets the province of Northwest Territories.
    /// </summary>
    public static readonly Province NT = new("NT", "Northwest Territories");

    /// <summary>
    /// Gets the province of Nunavut.
    /// </summary>
    public static readonly Province NU = new("NU", "Nunavut");

    /// <summary>
    /// Gets the province of Yukon.
    /// </summary>
    public static readonly Province YT = new("YT", "Yukon");

    /// <summary>
    /// Converts a string representation of a province code or name to its corresponding two-letter province code.
    /// </summary>
    /// <param name="province">The string representation of the province code or name.</param>
    /// <returns>The corresponding two-letter province code if it was successfully parsed; otherwise an empty string.</returns>
    public static String ToProvince(this String? province)
    {
        if (String.IsNullOrWhiteSpace(province))
        {
            return String.Empty;
        }

        var prov = Parse(province, false);
        foreach (var p in All)
        {
            if (p == prov)
            {
                return p.Code;
            }
        }

        return province ?? String.Empty;
    }

    /// <summary>
    /// Attempts to parse a string representation of a province code or name into its corresponding Province object.
    /// </summary>
    /// <param name="code">The string representation of the province code or name.</param>
    /// <param name="province">When this method returns, contains the parsed Province object if the parse operation was successful; otherwise, Province.Empty.</param>
    /// <returns>true if the parse operation was successful; otherwise, false.</returns>
    public static Boolean TryParse(String? code, out Province province)
    {
        province = Parse(code, false);
        return province != Province.Empty;
    }

    /// <summary>
    /// Parses a string representation of a province code or name into its corresponding Province object.
    /// </summary>
    /// <param name="code">The string representation of the province code or name.</param>
    /// <param name="throwOnInvalid">A boolean value indicating whether to throw an exception if the parse operation is unsuccessful.</param>
    /// <returns>The corresponding Province object if the parse operation was successful; otherwise, Province.Empty.</returns>
    /// <exception cref="ArgumentException">Thrown when the code is null or whitespace and throwOnInvalid is true.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the code is invalid and throwOnInvalid is true.</exception>
    public static Province Parse(String? code, Boolean throwOnInvalid = true)
    {
        if (String.IsNullOrWhiteSpace(code))
        {
            return throwOnInvalid
                ? throw new ArgumentException($"'{nameof(code)}' cannot be null or whitespace.", nameof(code))
                : Province.Empty;
        }

        var normalized = Replacer().Replace(code, String.Empty).ToUpper(CultureInfo.InvariantCulture);

        return normalized switch
        {
            "AB" => AB,
            "ALBERA" => AB,
            "ALBERTA" => AB,
            "BC" => BC,
            "BRITISH COLUMBIA" => BC,
            "MB" => MB,
            "MANITOBA" => MB,
            "NB" => NB,
            "NEW BRUNSWICK" => NB,
            "NL" => NL,
            "NEWFOUNDLAND" => NL,
            "NEWFOUNDLAND AND LABRADOR" => NL,
            "NS" => NS,
            "NOVA SCOTIA" => NS,
            "ON" => ON,
            "ONTARIO" => ON,
            "PE" => PE,
            "PRINCE EDWARD ISLAND" => PE,
            "QC" => QC,
            "QUBEC" => QC,
            "QUÉBEC" => QC,
            "QUEBEC" => QC,
            "SK" => SK,
            "SASKATCHEWAN" => SK,
            "NT" => NT,
            "NORTHWEST TERRITORIES" => NT,
            "NU" => NU,
            "NUNAVUT" => NU,
            "YT" => YT,
            "YUKON" => YT,
            _ => throwOnInvalid
                ? throw new ArgumentOutOfRangeException(nameof(code), code, "Invalid province code")
                : Province.Empty
        };
    }

    [GeneratedRegex("[^A-Z ]", RegexOptions.IgnoreCase)]
    private static partial Regex Replacer();
}
