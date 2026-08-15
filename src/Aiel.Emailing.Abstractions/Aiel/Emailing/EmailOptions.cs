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

using Microsoft.Extensions.Options;

namespace Aiel.Emailing;

public class EmailOptions
{
    public static readonly String SectionName = nameof(EmailOptions);

    public String SmtpServer { get; set; } = "127.0.0.1";
    public Int32 SmtpPort { get; set; } = 25;
    public String Username { get; set; } = String.Empty;
    public String Password { get; set; } = String.Empty;
    public String FromName { get; set; } = String.Empty;
    public String FromAddress { get; set; } = String.Empty;

    public Boolean TestMode { get; set; } = true;
    public String? TestAddress { get; set; }
    public String? TestName { get; set; }

    public Boolean UseStartTls { get; set; }
    public Boolean UseSSL { get; set; }
}

public class EmailOptionsValidator : IValidateOptions<EmailOptions>
{
    protected readonly List<String> Errors = [];

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
