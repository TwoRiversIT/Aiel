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

public static partial class Provinces
{
    public static readonly String[] Codes = ["AB", "BC", "MB", "NB", "NL", "NS", "ON", "PE", "QC", "SK", "NT", "NU", "YT"];

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

    public static readonly Province AB = new("AB", "Alberta");
    public static readonly Province BC = new("BC", "British Columbia");
    public static readonly Province MB = new("MB", "Manitoba");
    public static readonly Province NB = new("NB", "New Brunswick");
    public static readonly Province NL = new("NL", "Newfoundland and Labrador");
    public static readonly Province NS = new("NS", "Nova Scotia");
    public static readonly Province ON = new("ON", "Ontario");
    public static readonly Province PE = new("PE", "Prince Edward Island");
    public static readonly Province QC = new("QC", "Québec");
    public static readonly Province SK = new("SK", "Saskatchewan");
    public static readonly Province NT = new("NT", "Northwest Territories");
    public static readonly Province NU = new("NU", "Nunavut");
    public static readonly Province YT = new("YT", "Yukon");

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

        return province;
    }

    public static Boolean TryParse(String? code, out Province province)
    {
        province = Parse(code, false);
        return province != Province.Empty;
    }

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
