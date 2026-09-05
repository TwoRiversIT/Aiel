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
using Microsoft.Extensions.Options;

namespace Aiel.Emailing;

/// <summary>
/// Represents the configuration options for email functionality.
/// </summary>
public class EmailOptions
{
    /// <summary>
    /// Gets the name of the configuration section for email options.
    /// </summary>
    public static readonly String SectionName = nameof(EmailOptions);

    /// <summary>
    /// Gets or sets the SMTP server address.
    /// </summary>
    public String SmtpServer { get; set; } = "127.0.0.1";

    /// <summary>
    /// Gets or sets the SMTP server port.
    /// </summary>
    public Int32 SmtpPort { get; set; } = 25;

    /// <summary>
    /// Gets or sets the username for SMTP authentication.
    /// </summary>
    public String Username { get; set; } = String.Empty;

    /// <summary>
    /// Gets or sets the password for SMTP authentication.
    /// </summary>
    public String Password { get; set; } = String.Empty;

    /// <summary>
    /// Gets or sets the display name for the sender.
    /// </summary>
    public String FromName { get; set; } = String.Empty;

    /// <summary>
    /// Gets or sets the email address of the sender.
    /// </summary>
    public String FromAddress { get; set; } = String.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the email functionality is in test mode.
    /// </summary>
    public Boolean TestMode { get; set; } = true;

    /// <summary>
    /// Gets or sets the email address to use for testing when TestMode is enabled.
    /// </summary>
    public String? TestAddress { get; set; }

    /// <summary>
    /// Gets or sets the display name to use for testing when TestMode is enabled.
    /// </summary>
    public String? TestName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use STARTTLS for secure email communication.
    /// </summary>
    public Boolean UseStartTls { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use SSL for secure email communication.
    /// </summary>
    public Boolean UseSSL { get; set; }
}

/// <summary>
/// Validates the <see cref="EmailOptions"/> configuration options.
/// </summary>
public class EmailOptionsValidator : IValidateOptions<EmailOptions>
{
    /// <summary>
    /// Gets the list of validation errors encountered during the validation process.
    /// </summary>
    protected readonly List<String> Errors = [];

    /// <summary>
    /// Validates the specified <see cref="EmailOptions"/> instance.
    /// </summary>
    /// <param name="name">The name of the options instance being validated.</param>
    /// <param name="options">The <see cref="EmailOptions"/> instance to validate.</param>
    /// <returns>A <see cref="ValidateOptionsResult"/> indicating the result of the validation.</returns>
    public virtual ValidateOptionsResult Validate(String? name, EmailOptions options)
    {
        var key = String.IsNullOrWhiteSpace(name) ? nameof(EmailOptions) : name;

        if (String.IsNullOrWhiteSpace(options.SmtpServer))
        {
            Errors.Add($"{key}.SmtpServer is required.");
        }

        if (options.SmtpPort <= 0)
        {
            Errors.Add($"{key}.SmtpPort must be greater than 0.");
        }

        if (String.IsNullOrEmpty(options.Username))
        {
            Errors.Add($"{key}.Username is required.");
        }

        if (String.IsNullOrEmpty(options.Password))
        {
            Errors.Add($"{key}.Password is required.");
        }

        if (options.TestMode)
        {
            if (String.IsNullOrEmpty(options.TestAddress))
            {
                Errors.Add($"{key}.TestAddress is required when TestMode is enabled.");
            }

            if (!EmailValidator.Instance.IsValid(options.TestAddress))
            {
                Errors.Add($"{key}.TestAddress is not a valid email address.");
            }
        }

        if (options.UseSSL && options.UseStartTls)
        {
            Errors.Add($"{key}.UseSSL and {key}.UseStartTls cannot both be true.");
        }

        return Errors.Count > 0
            ? ValidateOptionsResult.Fail(Errors)
            : ValidateOptionsResult.Success;
    }
}
