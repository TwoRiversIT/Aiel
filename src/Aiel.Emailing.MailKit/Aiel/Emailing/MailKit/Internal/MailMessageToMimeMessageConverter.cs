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
using MimeKit;
using System.Net.Mail;
using System.Text;

namespace Aiel.Emailing.MailKit.Internal;

//[SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "No way to know which exception types may be thrown.")]
public static class MailMessageToMimeMessageConverter
{
    public static Result<MimeMessage> ToMimeMessage(this MailMessage mail)
    {
        ArgumentNullException.ThrowIfNull(mail);

        var mime = new MimeMessage();

        // From
        if (mail.From != null)
        {
            mime.From.Add(new MailboxAddress(mail.From.DisplayName ?? String.Empty, mail.From.Address));
        }

        // ReplyTo
        foreach (var reply in mail.ReplyToList.Cast<MailAddress>())
        {
            mime.ReplyTo.Add(new MailboxAddress(reply.DisplayName ?? String.Empty, reply.Address));
        }

        // To, CC, Bcc
        foreach (var to in mail.To.Cast<MailAddress>())
        {
            mime.To.Add(new MailboxAddress(to.DisplayName ?? String.Empty, to.Address));
        }

        foreach (var cc in mail.CC.Cast<MailAddress>())
        {
            mime.Cc.Add(new MailboxAddress(cc.DisplayName ?? String.Empty, cc.Address));
        }

        foreach (var b in mail.Bcc.Cast<MailAddress>())
        {
            mime.Bcc.Add(new MailboxAddress(b.DisplayName ?? String.Empty, b.Address));
        }

        // Subject
        mime.Subject = mail.Subject ?? String.Empty;

        var builder = new BodyBuilder();

        // Basic body
        if (!String.IsNullOrEmpty(mail.Body))
        {
            if (mail.IsBodyHtml)
            {
                builder.HtmlBody = mail.Body;
            }
            else
            {
                builder.TextBody = mail.Body;
            }
        }

        // AlternateViews and linked resources
        try
        {
            foreach (var alt in mail.AlternateViews)
            {
                // Read alternate view content
                using (var ms = new MemoryStream())
                {
                    if (alt.ContentStream.CanSeek)
                    {
                        alt.ContentStream.Position = 0;
                    }

                    alt.ContentStream.CopyTo(ms);
                    ms.Position = 0;

                    var charset = alt.ContentType?.CharSet;
                    var text = String.IsNullOrEmpty(charset)
                        ? Encoding.UTF8.GetString(ms.ToArray())
                        : Encoding.GetEncoding(charset).GetString(ms.ToArray());

                    if (String.Equals(alt.ContentType?.MediaType, "text/html", StringComparison.OrdinalIgnoreCase))
                    {
                        builder.HtmlBody = text;
                    }
                    else if (String.Equals(alt.ContentType?.MediaType, "text/plain", StringComparison.OrdinalIgnoreCase))
                    {
                        builder.TextBody = text;
                    }
                    else if (String.IsNullOrEmpty(builder.TextBody) && String.IsNullOrEmpty(builder.HtmlBody))
                    {
                        builder.TextBody = text;
                    }
                }

                // Linked resources (inline)
                foreach (var lr in alt.LinkedResources)
                {
                    using (var lrStream = new MemoryStream())
                    {
                        if (lr.ContentStream.CanSeek)
                        {
                            lr.ContentStream.Position = 0;
                        }

                        lr.ContentStream.CopyTo(lrStream);
                        var bytes = lrStream.ToArray();

                        var fileName = !String.IsNullOrEmpty(lr.ContentId)
                            ? lr.ContentId.Trim('<', '>')
                            : (lr.ContentType?.Name ?? Guid.NewGuid().ToString());

                        // Parse content type if available
                        ContentType? mimeType = null;
                        if (lr.ContentType != null)
                        {
                            try
                            {
                                mimeType = ContentType.Parse(lr.ContentType.ToString());
                            }
                            catch (ParseException)
                            {
                                mimeType = null;
                            }
                        }

                        var resource = mimeType == null
                            ? builder.LinkedResources.Add(fileName, bytes)
                            : builder.LinkedResources.Add(fileName, bytes, mimeType);

                        if (!String.IsNullOrEmpty(lr.ContentId))
                        {
                            resource.ContentId = lr.ContentId.Trim('<', '>');
                        }

                        resource.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
                    }
                }
            }
        }
        catch (InvalidOperationException ex)
        {
            return new ConversionFailedError(ex.FormatException());
        }

        // Attachments
        foreach (var att in mail.Attachments)
        {
            if (att.ContentStream.CanSeek)
            {
                att.ContentStream.Position = 0;
            }

            var name = att.Name ?? att.ContentType?.Name ?? Guid.NewGuid().ToString();

            // Parse content type if available
            if (att.ContentType != null && ContentType.TryParse(att.ContentType.ToString(), out var mimeType))
            {
                var part = mimeType == null
                    ? builder.Attachments.Add(name, att.ContentStream)
                    : builder.Attachments.Add(name, att.ContentStream, mimeType);

                // Preserve inline vs attachment disposition
                var disp = att.ContentDisposition?.DispositionType;
                part.ContentDisposition = !String.IsNullOrEmpty(disp)
                    && disp.Equals(System.Net.Mime.DispositionTypeNames.Inline, StringComparison.OrdinalIgnoreCase)
                        ? new ContentDisposition(ContentDisposition.Inline)
                        : new ContentDisposition(ContentDisposition.Attachment);
            }
        }

        // Build the body
        mime.Body = builder.ToMessageBody();

        // Priority headers
        switch (mail.Priority)
        {
            case MailPriority.High:
                mime.Headers.Add("X-Priority", "1 (Highest)");
                mime.Headers.Add("Priority", "urgent");
                mime.Headers.Add("Importance", "high");
                break;
            case MailPriority.Low:
                mime.Headers.Add("X-Priority", "5 (Lowest)");
                mime.Headers.Add("Priority", "non-urgent");
                mime.Headers.Add("Importance", "low");
                break;
        }

        // Copy other headers (skip duplicates)
        try
        {
            foreach (var key in mail.Headers.AllKeys)
            {
                if (String.IsNullOrEmpty(key))
                {
                    continue;
                }

                if (mime.Headers.Contains(key))
                {
                    continue;
                }

                if (mail.Headers[key] == null)
                {
                    continue;
                }

                mime.Headers.Add(key, mail.Headers[key]!);
            }
        }
        catch (ArgumentException)
        {
            // ignore duplicate headers
        }

        return mime;
    }
}
