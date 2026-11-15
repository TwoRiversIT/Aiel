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

using Aiel.ValueObjects.Net;
using Aiel.ValueObjects.ValueConverters;
using Microsoft.EntityFrameworkCore;

namespace Aiel.ValueObjects.EntityFrameworkCore;

/// <summary>
/// Provides extension methods for configuring model conventions in Entity Framework Core for Aiel.Domain value objects.
/// </summary>
public static class ModelConfigurationBuilderExtensions
{
    public static ModelConfigurationBuilder AddAddressValueConverter(this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<Address>()
            .HaveConversion<JsonValueConverter<Address>>();

        return configurationBuilder;
    }

    public static ModelConfigurationBuilder AddDomainNameValueConverter(this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<DomainName>()
            .HaveConversion<DomainNameValueConverter>();

        return configurationBuilder;
    }

    public static ModelConfigurationBuilder AddEmailAddressValueConverter(this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<EmailAddress>()
            .HaveConversion<EmailAddressValueConverter>();

        return configurationBuilder;
    }

    public static ModelConfigurationBuilder AddEmailValueConverter(this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<Email>()
            .HaveConversion<EmailValueConverter>();

        return configurationBuilder;
    }

    public static ModelConfigurationBuilder AddEndPointValueConverter(this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<EndPoint>()
            .HaveConversion<EndPointValueConverter>();

        return configurationBuilder;
    }

    public static ModelConfigurationBuilder AddGenderValueConverter(this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<Gender>()
            .HaveConversion<GenderValueConverter>();

        return configurationBuilder;
    }

    public static ModelConfigurationBuilder AddPhoneNumberValueConverter(this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<PhoneNumber>()
            .HaveConversion<PhoneNumberValueConverter>();

        return configurationBuilder;
    }

    /// <summary>
    /// Configures model conventions for Aiel.Domain value objects in Entity Framework Core.
    /// </summary>
    /// <param name="configurationBuilder">The model configuration builder.</param>
    /// <returns>The model configuration builder.</returns>
    /// <remarks>
    /// The Current converters are:
    /// <list type="bullet">
    /// <item><description><see cref="AddressValueConverter"/></description></item>
    /// <item><description><see cref="DomainNameValueConverter"/></description></item>
    /// <item><description><see cref="EmailAddressValueConverter"/></description></item>
    /// <item><description><see cref="EmailValueConverter"/></description></item>
    /// <item><description><see cref="EndPointValueConverter"/></description></item>
    /// <item><description><see cref="JsonValueConverter&lt;Address&gt;"/></description></item>
    /// <item><description><see cref="PhoneNumberValueConverter"/></description></item>
    /// </list>
    /// </remarks>
    public static ModelConfigurationBuilder AddValueObjectConverters(this ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddAddressValueConverter();
        configurationBuilder.AddDomainNameValueConverter();
        configurationBuilder.AddEmailAddressValueConverter();
        configurationBuilder.AddEmailValueConverter();
        configurationBuilder.AddEndPointValueConverter();
        configurationBuilder.AddGenderValueConverter();
        configurationBuilder.AddPhoneNumberValueConverter();

        return configurationBuilder;
    }
}
