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

namespace Aiel.Domain.Geography;

// Cribbed from https://stackoverflow.com/a/49313331/32588

public class Countries(CultureTypes cultureTypes = CultureTypes.SpecificCultures)
{
    public IEnumerable<CountryInfo> All { get; } = GetAllCountries(cultureTypes);

    public IEnumerable<CountryInfo> GetCountryInfoByName(String name, Boolean nativeName = false)
    {
        return nativeName
            ? All.Where(info => info.Region?.NativeName == name).ToList()
            : All.Where(info => info.Region?.EnglishName == name).ToList();
    }

    public IEnumerable<CountryInfo> GetCountryInfoByName(String name, Boolean isNeutralCulture, Boolean nativeName = false)
    {
        return nativeName
            ? All.Where(info => info.Region?.NativeName == name && info.Culture?.IsNeutralCulture == isNeutralCulture).ToList()
            : All.Where(info => info.Region?.EnglishName == name && info.Culture?.IsNeutralCulture == isNeutralCulture).ToList();
    }

    public String GetTwoLettersName(String name, String? defaultCountry = null, Boolean nativeName = false)
    {
        var country = nativeName
            ? All.FirstOrDefault(info => info.Region?.NativeName == name)
            : All.FirstOrDefault(info => info.Region?.EnglishName == name);

        return country == null
            ? defaultCountry ?? String.Empty
            : country.Region?.TwoLetterISORegionName ?? String.Empty;
    }

    public String GetThreeLettersName(String name, String defaultCountry = "CAN", Boolean nativeName = false)
    {
        var country = nativeName
            ? All.FirstOrDefault(info => info.Region?.NativeName.Contains(name) == true)
            : All.FirstOrDefault(info => info.Region?.EnglishName.Contains(name) == true);

        return country == null
            ? defaultCountry ?? String.Empty
            : country.Region?.ThreeLetterISORegionName ?? String.Empty;
    }

    private static IEnumerable<CountryInfo> GetAllCountries(CultureTypes cultureTypes)
    {
        foreach (var culture in CultureInfo.GetCultures(cultureTypes))
        {
            if (culture.LCID != 127)
            {
                yield return new CountryInfo()
                {
                    Culture = culture,
                    Region = new RegionInfo(culture.TextInfo.CultureName)
                };
            }
        }
    }

    public class CountryInfo
    {
        public CultureInfo? Culture { get; set; }
        public RegionInfo? Region { get; set; }
    }
}
