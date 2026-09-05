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
/// <summary>
/// Provides information about countries and their associated cultures and regions.
/// </summary>
/// <param name="cultureTypes">The types of cultures to include.</param>
public class Countries(CultureTypes cultureTypes = CultureTypes.SpecificCultures)
{
    /// <summary>
    /// Gets all countries with their associated culture and region information.
    /// </summary>
    public IEnumerable<CountryInfo> All { get; } = GetAllCountries(cultureTypes);

    /// <summary>
    /// Gets country information by name, either in English or native language.
    /// </summary>
    /// <param name="name">The name of the country to search for.</param>
    /// <param name="nativeName">Whether to search by the native name of the country.</param>
    /// <returns>A collection of <see cref="CountryInfo"/> objects that match the specified name.</returns>
    public IEnumerable<CountryInfo> GetCountryInfoByName(String name, Boolean nativeName = false)
    {
        return nativeName
            ? All.Where(info => info.Region?.NativeName == name).ToList()
            : All.Where(info => info.Region?.EnglishName == name).ToList();
    }

    /// <summary>
    /// Gets country information by name, either in English or native language, and filters by whether the culture is neutral.
    /// </summary>
    /// <param name="name">The name of the country to search for.</param>
    /// <param name="isNeutralCulture">Whether to filter by neutral culture.</param>
    /// <param name="nativeName">Whether to search by the native name of the country.</param>
    /// <returns>A collection of <see cref="CountryInfo"/> objects that match the specified criteria.</returns>
    public IEnumerable<CountryInfo> GetCountryInfoByName(String name, Boolean isNeutralCulture, Boolean nativeName = false)
    {
        return nativeName
            ? All.Where(info => info.Region?.NativeName == name && info.Culture?.IsNeutralCulture == isNeutralCulture).ToList()
            : All.Where(info => info.Region?.EnglishName == name && info.Culture?.IsNeutralCulture == isNeutralCulture).ToList();
    }

    /// <summary>
    /// Gets the two-letter ISO region name for the specified country.
    /// </summary>
    /// <param name="name">The name of the country.</param>
    /// <param name="defaultCountry">The default country code to return if the country is not found.</param>
    /// <param name="nativeName">Whether to search by the native name of the country.</param>
    /// <returns>The two-letter ISO region name.</returns>
    public String GetTwoLettersName(String name, String? defaultCountry = null, Boolean nativeName = false)
    {
        var country = nativeName
            ? All.FirstOrDefault(info => info.Region?.NativeName == name)
            : All.FirstOrDefault(info => info.Region?.EnglishName == name);

        return country == null
            ? defaultCountry ?? String.Empty
            : country.Region?.TwoLetterISORegionName ?? String.Empty;
    }

    /// <summary>
    /// Gets the three-letter ISO region name for the specified country.
    /// </summary>
    /// <param name="name">The name of the country.</param>
    /// <param name="defaultCountry">The default country code to return if the country is not found.</param>
    /// <param name="nativeName">Whether to search by the native name of the country.</param>
    /// <returns>The three-letter ISO region name.</returns>
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

    /// <summary>
    /// Represents information about a country, including its associated culture and region.
    /// </summary>
    public class CountryInfo
    {
        /// <summary>
        /// Gets or sets the culture associated with the country.
        /// </summary>
        public CultureInfo? Culture { get; set; }

        /// <summary>
        /// Gets or sets the region associated with the country.
        /// </summary>
        public RegionInfo? Region { get; set; }
    }
}
