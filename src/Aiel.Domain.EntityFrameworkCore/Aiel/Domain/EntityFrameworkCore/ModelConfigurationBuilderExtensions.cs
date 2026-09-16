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

using Aiel.Domain.Contacts;
using Aiel.Domain.EntityFrameworkCore.ValueConverters;
using Aiel.Domain.ValueObjects.Addresses;
using Aiel.Domain.ValueObjects.Contacts;
using Aiel.Domain.ValueObjects.Net;
using Microsoft.EntityFrameworkCore;

namespace Aiel.Domain.EntityFrameworkCore;

/// <summary>
/// Provides extension methods for configuring model conventions in Entity Framework Core for Aiel.Domain value objects.
/// </summary>
public static class ModelConfigurationBuilderExtensions
{
    /// <summary>
    /// Configures model conventions for Aiel.Domain value objects in Entity Framework Core.
    /// </summary>
    /// <param name="configurationBuilder">The model configuration builder.</param>
    /// <returns>The model configuration builder.</returns>
    /// <remarks>
    /// The Current converters are:
    /// <list type="bullet">
    /// <item><description>EmailValueConverter</description></item>
    /// <item><description>EmailAddressValueConverter</description></item>
    /// <item><description>PhoneNumberValueConverter</description></item>
    /// <item><description>EndPointValueConverter</description></item>
    /// <item><description>DomainNameValueConverter</description></item>
    /// <item><description>JsonValueConverter&lt;Address&gt;</description></item>
    /// </list>
    /// </remarks>
    public static ModelConfigurationBuilder ConfigureValueConverters(this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<Email>()
            .HaveConversion<EmailValueConverter>();

        configurationBuilder
            .Properties<EmailAddress>()
            .HaveConversion<EmailAddressValueConverter>();

        configurationBuilder
            .Properties<PhoneNumber>()
            .HaveConversion<PhoneNumberValueConverter>();

        configurationBuilder
            .Properties<EndPoint>()
            .HaveConversion<EndPointValueConverter>();

        configurationBuilder
            .Properties<DomainName>()
            .HaveConversion<DomainNameValueConverter>();

        configurationBuilder
            .Properties<Address>()
            .HaveConversion<JsonValueConverter<Address>>();

        return configurationBuilder;
    }
}
