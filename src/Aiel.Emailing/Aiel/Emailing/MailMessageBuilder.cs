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
using Aiel.Security;
using Aiel.UI;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;

namespace Aiel.Emailing;

/// <summary>
/// A builder class for constructing email messages with support for Markdown,
/// HTML, and plain text content. This class allows you to set the sender,
/// recipients, subject, body content, and attachments for an email message.
/// It also provides validation to ensure that the constructed message is
/// valid before sending.
/// </summary>
/// <param name="markdownRenderer">The markdown renderer used to convert markdown content to HTML.</param>
public class MailMessageBuilder(IMarkdownRenderer markdownRenderer)
    : IDisposable, IAsyncDisposable
{
    private readonly IMarkdownRenderer _markdownRenderer = markdownRenderer;
    private readonly StringBuilder _markdown = new();
    private MailMessage _message = new();
    private String? _text;
    private String? _html;
    private Boolean _built;
    private Boolean _disposed;

    /// <summary>
    /// Gets a value indicating whether the email message has any attachments.
    /// </summary>
    public Boolean HasAttachments => _message.Attachments.Count > 0;
    /// <summary>
    /// Gets the subject of the email message.
    /// </summary>
    public String Subject => _message.Subject ?? String.Empty;

    /// <summary>
    /// Sets the sender of the email message using the specified name and email address.
    /// </summary>
    /// <param name="name">The name of the sender.</param>
    /// <param name="email">The email address of the sender.</param>
    /// <returns>The current instance of <see cref="MailMessageBuilder"/> to allow method chaining.</returns>
    public MailMessageBuilder SendFrom(String name, Email email)
        => SendFrom(new EmailAddress(name, email));

    /// <summary>
    /// Sets the sender of the email message using the specified <see cref="EmailAddress"/>.
    /// </summary>
    /// <param name="emailAddress">The email address of the sender.</param>
    /// <returns>The current instance of <see cref="MailMessageBuilder"/> to allow method chaining.</returns>
    public MailMessageBuilder SendFrom(EmailAddress emailAddress)
    {
        EnsureNotDisposedOrBuilt();

        _message.From = emailAddress;

        return this;
    }

    /// <summary>
    /// Sets the reply-to address of the email message using the specified name and email address.
    /// </summary>
    /// <param name="name">The name of the reply-to address.</param>
    /// <param name="email">The email address of the reply-to address.</param>
    /// <returns>The current instance of <see cref="MailMessageBuilder"/> to allow method chaining.</returns>
    public MailMessageBuilder ReplyTo(String name, Email email)
        => ReplyTo(new EmailAddress(name, email));

    /// <summary>
    /// Sets the reply-to address of the email message using the specified <see cref="EmailAddress"/>.
    /// </summary>
    /// <param name="emailAddress">The email address of the reply-to address.</param>
    /// <returns>The current instance of <see cref="MailMessageBuilder"/> to allow method chaining.</returns>
    public MailMessageBuilder ReplyTo(EmailAddress emailAddress)
    {
        EnsureNotDisposedOrBuilt();

        _message.ReplyToList.Add(emailAddress);

        return this;
    }

    /// <summary>
    /// Sets the recipient of the email message using the specified <see cref="ClaimsPrincipal"/>.
    /// </summary>
    /// <param name="principal">The claims principal representing the recipient.</param>
    /// <returns>The current instance of <see cref="MailMessageBuilder"/> to allow method chaining.</returns>
    public MailMessageBuilder To(ClaimsPrincipal principal)
        => To(new EmailAddress(principal.FullName(), principal.Email()));

    /// <summary>
    /// Sets the recipient of the email message using the specified name and email address.
    /// </summary>
    /// <param name="name">The name of the recipient.</param>
    /// <param name="email">The email address of the recipient.</param>
    /// <returns>The current instance of <see cref="MailMessageBuilder"/> to allow method chaining.</returns>
    public MailMessageBuilder To(String name, Email email)
        => To(new EmailAddress(name, email));

    /// <summary>
    /// Sets the recipient of the email message using the specified <see cref="EmailAddress"/>.
    /// </summary>
    /// <param name="emailAddress">The email address of the recipient.</param>
    /// <returns>The current instance of <see cref="MailMessageBuilder"/> to allow method chaining.</returns>
    public MailMessageBuilder To(EmailAddress emailAddress)
    {
        EnsureNotDisposedOrBuilt();

        _message.To.Add(emailAddress);

        return this;
    }

    /// <summary>
    /// Sets the CC (carbon copy) recipient of the email message using the specified name and email address.
    /// </summary>
    /// <param name="name">The name of the CC recipient.</param>
    /// <param name="email">The email address of the CC recipient.</param>
    /// <returns>The current instance of <see cref="MailMessageBuilder"/> to allow method chaining.</returns>
    public MailMessageBuilder CC(String name, Email email)
        => CC(new EmailAddress(name, email));

    /// <summary>
    /// Sets the CC (carbon copy) recipient of the email message using the specified <see cref="EmailAddress"/>.
    /// </summary>
    /// <param name="emailAddress">The email address of the CC recipient.</param>
    /// <returns>The current instance of <see cref="MailMessageBuilder"/> to allow method chaining.</returns>
    public MailMessageBuilder CC(EmailAddress emailAddress)
    {
        EnsureNotDisposedOrBuilt();

        _message.CC.Add(emailAddress);
        return this;
    }

    /// <summary>
    /// Sets the BCC (blind carbon copy) recipient of the email message using the specified name and email address.
    /// </summary>
    /// <param name="name">The name of the BCC recipient.</param>
    /// <param name="email">The email address of the BCC recipient.</param>
    /// <returns>The current instance of <see cref="MailMessageBuilder"/> to allow method chaining.</returns>
    public MailMessageBuilder BCC(String name, Email email)
        => BCC(new EmailAddress(name, email));

    /// <summary>
    /// Sets the BCC (blind carbon copy) recipient of the email message using the specified <see cref="EmailAddress"/>.
    /// </summary>
    /// <param name="emailAddress">The email address of the BCC recipient.</param>
    /// <returns>The current instance of <see cref="MailMessageBuilder"/> to allow method chaining.</returns>
    public MailMessageBuilder BCC(EmailAddress emailAddress)
    {
        EnsureNotDisposedOrBuilt();

        _message.Bcc.Add(emailAddress);
        return this;
    }

    /// <summary>
    /// Sets the subject of the email message.
    /// </summary>
    /// <param name="subject">The subject of the email message.</param>
    /// <returns>The current instance of <see cref="MailMessageBuilder"/> to allow method chaining.</returns>
    public MailMessageBuilder WithSubject(String subject)
    {
        if (String.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException($"'{nameof(subject)}' must not be null or whitespace.", nameof(subject));
        }

        EnsureNotDisposedOrBuilt();

        _message.Subject = subject;

        return this;
    }

    /// <summary>
    /// Sets the priority of the email message.
    /// </summary>
    /// <param name="priority">The priority of the email message.</param>
    /// <returns>The current instance of <see cref="MailMessageBuilder"/> to allow method chaining.</returns>
    public MailMessageBuilder WithPriority(MailPriority priority)
    {
        EnsureNotDisposedOrBuilt();

        _message.Priority = priority;

        return this;
    }

    /// <summary>
    /// Appends the <paramref name="markdown"/> content to the email message body.
    /// </summary>
    /// <param name="markdown">The markdown content to be added. Cannot be null or whitespace.</param>
    /// <returns>The current instance of <see cref="MailMessageBuilder"/> to allow method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="markdown"/> is null or whitespace.</exception>
    /// <remarks>
    /// <para>If you use this, the markdown will be rendered to HTML and replace any HTML body that may have
    /// been provided by <see cref="WithHtmlBody"/>.</para>
    /// </remarks>
    public MailMessageBuilder Append(String markdown)
    {
        EnsureNotDisposedOrBuilt();

        if (String.IsNullOrWhiteSpace(markdown))
        {
            throw new ArgumentException($"'{nameof(markdown)}' cannot be null or whitespace.", nameof(markdown));
        }

        _markdown.Append(markdown);

        return this;
    }

    /// <summary>
    /// Appends the <paramref name="markdown"/> content to the email message body followed by the default line terminator.
    /// </summary>
    /// <param name="markdown">The markdown content to be added. Cannot be null or whitespace.</param>
    /// <returns>The current instance of <see cref="MailMessageBuilder"/> to allow method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="markdown"/> is null or whitespace.</exception>
    /// <remarks>
    /// <para>If you use this, the markdown will be rendered to HTML and replace any HTML body that may have
    /// been set by <see cref="WithHtmlBody"/>.</para>
    /// </remarks>
    public MailMessageBuilder AppendLine(String markdown)
    {
        EnsureNotDisposedOrBuilt();

        _markdown.AppendLine(markdown ?? String.Empty);

        return this;
    }

    /// <summary>
    /// Appends a blank line to the markdown content of the email message body.
    /// </summary>
    /// <returns>The current instance of <see cref="MailMessageBuilder"/> to allow method chaining.</returns>
    /// <remarks>
    /// <para>If you use this, the markdown will be rendered to HTML and replace any HTML body that may have
    /// been set by <see cref="WithHtmlBody"/>.</para>
    /// </remarks>
    public MailMessageBuilder AppendLine()
    {
        EnsureNotDisposedOrBuilt();

        _markdown.AppendLine();

        return this;
    }

    /// <summary>
    /// Sets the plain text body of the email message.
    /// </summary>
    /// <param name="text">The plain text content to be used as the email body. Must not be null or whitespace.</param>
    /// <returns>The current instance of <see cref="MailMessageBuilder"/> to allow method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="text"/> is null or consists only of whitespace.</exception>
    public MailMessageBuilder WithTextBody(String text)
    {
        EnsureNotDisposedOrBuilt();

        if (String.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException($"'{nameof(text)}' must not be null or whitespace.", nameof(text));
        }

        _text = text;

        return this;
    }

    /// <summary>
    /// Sets the HTML content of the email body.
    /// </summary>
    /// <param name="html">The HTML string to be used as the email body. Must not be null or whitespace.</param>
    /// <returns>The current instance of <see cref="MailMessageBuilder"/> to allow method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="html"/> is null or consists only of whitespace.</exception>
    /// <remarks>
    /// Using <see cref="Append(String)"/> will overwrite body content from <see cref="WithHtmlBody"/>.
    /// </remarks>
    public MailMessageBuilder WithHtmlBody(String html)
    {
        EnsureNotDisposedOrBuilt();

        if (String.IsNullOrWhiteSpace(html))
        {
            throw new ArgumentException($"'{nameof(html)}' must not be null or whitespace.", nameof(html));
        }

        _html = html;

        return this;
    }

    /// <summary>
    /// Adds an attachment to the email message with the specified name, file stream, and MIME type.
    /// </summary>
    /// <param name="name">The name of the attachment.</param>
    /// <param name="file">The file stream of the attachment.</param>
    /// <param name="mimeType">The MIME type of the attachment.</param>
    /// <returns>The current instance of <see cref="MailMessageBuilder"/> to allow method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if the name or MIME type is null or whitespace, or if the file stream is not readable or empty.</exception>
    public MailMessageBuilder AddAttachment(String name, Stream file, String mimeType)
    {
        EnsureNotDisposedOrBuilt();

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrWhiteSpace(mimeType);

        if (!file.CanRead)
        {
            throw new ArgumentException($"The stream '{nameof(file)}' must be readable.", nameof(file));
        }

        if (file.Length == 0)
        {
            throw new ArgumentException($"The stream '{nameof(file)}' must not be empty.", nameof(file));
        }

        return AddAttachment(new Attachment(file, name, mimeType));
    }

    /// <summary>
    /// Adds an attachment to the email message using the specified <see cref="Attachment"/> object.
    /// </summary>
    /// <param name="attachment">The attachment to be added to the email message.</param>
    /// <returns>The current instance of <see cref="MailMessageBuilder"/> to allow method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="attachment"/> is null.</exception>
    public MailMessageBuilder AddAttachment(Attachment attachment)
    {
        EnsureNotDisposedOrBuilt();

        _message.Attachments.Add(attachment ?? throw new ArgumentNullException(nameof(attachment)));

        return this;
    }

    /// <summary>
    /// Gets the current body of the email message based on the available content in the following
    /// order of preference: Markdown, HTML, and Plain Text. NOTE: Markdown will not be rendered to
    /// HTML until the <see cref="Build"/> method is called.
    /// </summary>
    /// <returns>A string containing the current body content. May be empty.</returns>
    public String Body()
    {
        return _markdown.Length > 0
                ? _markdown.ToString()
                : String.IsNullOrWhiteSpace(_html)
                    ? _text ?? String.Empty
                    : _html;
    }

    /// <summary>
    /// Constructs and returns a <see cref="MailMessage"/> object with the specified content.
    /// </summary>
    /// <remarks>
    /// <para>The body of the email message is based on the available content in the following
    /// order of preference: Markdown, HTML, and plain text. If the message body is set
    /// as HTML and a plain text version is available, it adds the plain text as an
    /// alternate view.</para>
    /// </remarks>
    /// <returns>A <see cref="MailMessage"/> object containing the constructed email message.</returns>
    public MailMessage Build()
    {
        EnsureNotDisposedOrBuilt();

        Validate();

        if (_markdown.Length > 0)
        {
            _message.Body = _markdownRenderer.Render(_markdown.ToString());
            _message.IsBodyHtml = true;
        }
        else if (!String.IsNullOrWhiteSpace(_html))
        {
            _message.Body = _html;
            _message.IsBodyHtml = true;
        }
        else
        {
            _message.Body = _text ?? String.Empty;
        }

        // If the message is HTML and a text body has been set, we add a plain text version.
        if (_message.IsBodyHtml && !String.IsNullOrWhiteSpace(_text))
        {
            _message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(_text, null, MediaTypeNames.Text.Plain));
        }

        _built = true;
        return _message;
    }

    private void EnsureNotDisposedOrBuilt()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(MailMessageBuilder), "Cannot use a disposed MailMessageBuilder.");
        }

        if (_built)
        {
            throw new InvalidOperationException("The MailMessage has already been built. Create a new instance to build another message.");
        }
    }

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> .
    /// </summary>
    /// <exception cref="InvalidOperationException">if the current state is invalid</exception>
    /// <remarks>
    /// This is called automatically by the <see cref="Build"/> method but can also be called manually to validate the current state.
    /// </remarks>
    public void Validate()
    {
        if (IsValid)
        {
            return;
        }

        if (_message.To.Count == 0)
        {
            throw new InvalidOperationException("The To: email address has not been set. At least one must be provided.");
        }

        if (_message.From is null && _message.ReplyToList.Count == 0)
        {
            throw new InvalidOperationException("The From: or ReplyTo: email address has not been set. One must be provided.");
        }

        if (String.IsNullOrWhiteSpace(Body()))
        {
            throw new InvalidOperationException("No message body.");
        }
    }

    /// <summary>
    /// Returns <see langword="true" /> if the current <see cref="MailMessage"/> is valid.
    /// </summary>
    /// <remarks>
    /// To be considered valid the following must be true: <br />
    /// <list type="bullet">
    /// <item>From or ReplyTo have been specified</item>
    /// <item>There are one or more recipients</item>
    /// <item>The subject has been set to a non-empty string</item>
    /// <item>The body has been provided as Markdown, HTML, or Plain Text</item>
    /// </list>
    /// </remarks>
    public Boolean IsValid
        => !_built
        && _message.To.Count > 0
        && (_message.From is not null || _message.ReplyToList.Count == 0)
        && !String.IsNullOrWhiteSpace(_message.Subject)
        && !String.IsNullOrWhiteSpace(Body());

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="MailMessageBuilder"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(Boolean disposing)
    {
        if (disposing && !_disposed)
        {
            // Dispose managed resources
            _built = true;
            _text = null;
            _html = null;
            _markdown.Clear();
            _message.Dispose();
            _message = null!;
            _disposed = true;
        }

        // Dispose unmanaged resources, if any.
    }

    /// <inheritdoc/>
    [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "This calls Dispose() which takes care of the rest.")]
    public virtual ValueTask DisposeAsync()
    {
        try
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
        catch (Exception ex)
        {
            return ValueTask.FromException(ex);
        }
    }
}
