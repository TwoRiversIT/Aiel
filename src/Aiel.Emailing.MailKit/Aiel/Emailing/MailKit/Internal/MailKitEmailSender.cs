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
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Aiel.Emailing.MailKit.Internal;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Is instantiated by DI/IoC.")]
internal sealed class MailKitEmailSender(IOptions<EmailOptions> options, IEmailValidator validator, ILogger<MailKitEmailSender> logger) : IEmailSender
{
    private readonly MailKitOptions _options = options?.Value as MailKitOptions
        ?? throw new ArgumentNullException(nameof(options));

    private readonly IEmailValidator _validator = validator;
    private readonly ILogger<MailKitEmailSender> _logger = logger ?? NullLogger<MailKitEmailSender>.Instance;

    private Boolean ArchiveEnabled
        => _options.ArchiveSentEmail
        && !String.IsNullOrWhiteSpace(_options.ArchiveBccName)
        && _validator.IsValid(_options.ArchiveBccAddress);

    private SecureSocketOptions SSO
        => _options.UseSSL
            ? SecureSocketOptions.SslOnConnect
            : _options.UseStartTls
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.StartTlsWhenAvailable;

    public Task SendEmailAsync(String email, String subject, String htmlMessage, CancellationToken cancellationToken = default)
    {
        var mailMessage = new System.Net.Mail.MailMessage
        {
            From = new System.Net.Mail.MailAddress(_options.FromAddress, _options.FromName),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };

        mailMessage.To.Add(new System.Net.Mail.MailAddress(email));

        return SendEmailAsync(mailMessage, cancellationToken);
    }

    public async Task SendEmailAsync(System.Net.Mail.MailMessage message, CancellationToken cancellationToken = default)
    {
        var mimeMessage = message.ToMimeMessage();

        if (_options.TestMode && !AddTestModeRecipients(mimeMessage))
        {
            return;
        }
        else if (ArchiveEnabled)
        {
            mimeMessage.Bcc.Add(new MailboxAddress(_options.ArchiveBccName, _options.ArchiveBccAddress!));
        }

        await SendAsync(mimeMessage, cancellationToken);
    }

    private Boolean AddTestModeRecipients(MimeMessage mimeMessage)
    {
        mimeMessage.To.Clear();
        mimeMessage.Cc.Clear();
        mimeMessage.Bcc.Clear();

        // The EmailOptions should have been validated at startup, but we check again
        // here in case they somehow managed to provide an invalid addresses.
        if (_validator.IsValid(_options.TestAddress) && !String.IsNullOrWhiteSpace(_options.TestName))
        {
            mimeMessage.To.Add(new MailboxAddress(_options.TestName, _options.TestAddress!));

            return true;
        }
        else
        {
            _logger.LogWarning("TestMode is enabled, but TestAddress and/or TestName are invalid. No recipients will be added.");

            return false;
        }
    }

    private async Task SendAsync(MimeMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        using (var smtp = new SmtpClient())
        {
            try
            {
                var subject = message.Subject ?? String.Empty;
                var from = message.From[0].ToString();
                var to = message.To[0].ToString();
                var attachmentCount = message.Attachments.Count();

                LogSending(message);

                await smtp.ConnectAsync(_options.SmtpServer, _options.SmtpPort, SSO, cancellationToken);

                LogConnected();

                if (!String.IsNullOrWhiteSpace(_options.Username) && !String.IsNullOrWhiteSpace(_options.Password))
                {
                    await smtp.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
                    LogAuthenticated();
                }

                LogSendingToServer(message);

                await smtp.SendAsync(message, cancellationToken);

                LogMessageSent(message);

                await smtp.DisconnectAsync(true, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogSendingFailed(ex, message.From[0].ToString(), message.To[0].ToString(), message.Subject ?? String.Empty);
            }
        }
    }

    private void LogMessageSent(MimeMessage message)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogMessageSent(message.From[0].ToString(), message.To[0].ToString(), message.Subject ?? String.Empty);
        }
    }

    private void LogSendingToServer(MimeMessage message)
    {
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogSendingToServer(message.Subject ?? String.Empty, _options.SmtpServer, _options.SmtpPort, SSO.ToString());
        }
    }

    private void LogAuthenticated()
    {
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogAuthenticated(_options.SmtpServer, _options.SmtpPort, SSO.ToString());
        }
    }

    private void LogConnected()
    {
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogConnected(_options.SmtpServer, _options.SmtpPort, SSO.ToString());
        }
    }

    private void LogSending(MimeMessage message)
    {
        if (_logger.IsEnabled(LogLevel.Information))
        {
            _logger.LogSending(message.Subject!, message.From[0].ToString(), message.To[0].ToString(), _options.SmtpServer, _options.SmtpPort, SSO.ToString(), message.Attachments.Count());
        }
    }
}

internal static partial class MailKitEmailSenderLoggerExtensions
{
    [LoggerMessage(EventId = (Int32)AielEvent.Emailing_Sending, Level = LogLevel.Information, Message = "[{EventId}] Attempting to send email {Subject} from {From} to {To} through {SmtpServer}:{SmtpPort} with {SecureSocketOptions}. Attachments: {AttachmentCount}")]
    internal static partial void LogSending(this ILogger logger, String subject, String from, String to, String smtpServer, Int32 smtpPort, String secureSocketOptions, Int32 attachmentCount, Int32 eventId = (Int32)AielEvent.Emailing_Sending);

    [LoggerMessage(EventId = (Int32)AielEvent.Emailing_Connected, Level = LogLevel.Trace, Message = "[{EventId}] Connected to SMTP server {SmtpServer}:{SmtpPort} with SecureSocketOptions {SecureSocketOptions}")]
    internal static partial void LogConnected(this ILogger logger, String smtpServer, Int32 smtpPort, String secureSocketOptions, Int32 eventId = (Int32)AielEvent.Emailing_Connected);

    [LoggerMessage(EventId = (Int32)AielEvent.Emailing_Authenticated, Level = LogLevel.Trace, Message = "[{EventId}] Authenticated to SMTP server {SmtpServer}:{SmtpPort} with SecureSocketOptions {SecureSocketOptions}")]
    internal static partial void LogAuthenticated(this ILogger logger, String smtpServer, Int32 smtpPort, String secureSocketOptions, Int32 eventId = (Int32)AielEvent.Emailing_Authenticated);

    [LoggerMessage(EventId = (Int32)AielEvent.Emailing_SendingToServer, Level = LogLevel.Trace, Message = "[{EventId}] Sending {Subject} to SMTP server {SmtpServer}:{SmtpPort} with SecureSocketOptions {SecureSocketOptions}")]
    internal static partial void LogSendingToServer(this ILogger logger, String subject, String smtpServer, Int32 smtpPort, String secureSocketOptions, Int32 eventId = (Int32)AielEvent.Emailing_SendingToServer);

    [LoggerMessage(EventId = (Int32)AielEvent.Emailing_MessageSent, Level = LogLevel.Information, Message = "[{EventId}] From: {From} To: {To} Subject: {Subject}")]
    internal static partial void LogMessageSent(this ILogger logger, String from, String to, String subject, Int32 eventId = (Int32)AielEvent.Emailing_MessageSent);

    [LoggerMessage(EventId = (Int32)AielEvent.Emailing_SendingFailed, Level = LogLevel.Error, Message = "[{EventId}] From: {From} To: {To} Subject: {Subject}")]
    internal static partial void LogSendingFailed(this ILogger logger, Exception exception, String from, String to, String subject, Int32 eventId = (Int32)AielEvent.Emailing_SendingFailed);
}
