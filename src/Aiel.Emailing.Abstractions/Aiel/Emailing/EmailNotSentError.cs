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

using Aiel.Framework;
using Aiel.Results;
using System.Text.Json.Serialization;

namespace Aiel.Emailing;

/// <summary>
/// Represents an error that occurs when an email fails to send.
/// </summary>
public sealed partial class EmailNotSentError : Error
{
    private const String Unknown = "Unknown";

    /// <summary>
    /// Gets a message that describes the error.
    /// </summary>
    [JsonIgnore]
    public String Message => $"Failed to send email from {From} to {To}: {Subject}.";

    /// <summary>
    /// Gets the email address of the sender.
    /// </summary>
    public String From { get; init; } = Unknown;

    /// <summary>
    /// Gets the email address of the recipient.
    /// </summary>
    public String To { get; init; } = Unknown;

    /// <summary>
    /// Gets the subject of the email.
    /// </summary>
    public String Subject { get; init; } = Unknown;

    /// <summary>
    /// Creates a new instance of the <see cref="EmailNotSentError"/> class with the specified error message.
    /// </summary>
    /// <param name="innerException">The exception that caused the email to fail to send.</param>
    /// <returns>A new instance of the <see cref="EmailNotSentError"/> class.</returns>
    public static EmailNotSentError Create(Exception innerException)
        => new(innerException.FormatException());

    /// <summary>
    /// Gets a predefined instance of <see cref="EmailNotSentError"/> that indicates email sending is in test mode, but the test address or name is not valid.
    /// </summary>
    public static readonly EmailNotSentError TestMode = new("Email sending is in test mode, but the test address or name is not valid. No email will be sent.");
}
