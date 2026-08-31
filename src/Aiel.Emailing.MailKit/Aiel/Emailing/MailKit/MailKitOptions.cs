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

namespace Aiel.Emailing.MailKit;

public class MailKitOptions : EmailOptions
{
    public Boolean ArchiveSentEmail { get; set; }
    public String? ArchiveBccName { get; set; }
    public String? ArchiveBccAddress { get; set; }
}

public class MailKitOptionsValidator : EmailOptionsValidator
{
    public override ValidateOptionsResult Validate(String? name, EmailOptions options)
    {
        if (options is not MailKitOptions mailKitOptions)
        {
            Errors.Add($"{name} is not a valid MailKitOptions instance.");

            return base.Validate(name, options);
        }

        var key = String.IsNullOrWhiteSpace(name) ? EmailOptions.SectionName : name;

        if (mailKitOptions.ArchiveSentEmail)
        {
            if (String.IsNullOrWhiteSpace(mailKitOptions.ArchiveBccName))
            {
                Errors.Add($"{key}.ArchiveBccName is required when ArchiveSentEmail is true.");
            }

            if (EmailValidator.IsValid(mailKitOptions.ArchiveBccAddress))
            {
                Errors.Add($"{key}.ArchiveBccAddress is invalid.");
            }
        }

        return base.Validate(name, options);
    }
}
