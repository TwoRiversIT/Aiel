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

using Aiel.Security;
using Aiel.Testing.Fakes;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.Claims;

namespace Aiel.Emailing;

public class MailMessageBuilderTests
{
    [Fact]
    public void Can_be_instantiated()
    {
        using var builder = new MailMessageBuilder(new FakeMarkdownRenderer());

        builder.Should().NotBeNull();
    }

    [Fact]
    public void Build_uses_markdown_when_present()
    {
        var renderer = new FakeMarkdownRenderer("<h1>Hello</h1>");
        using var builder = new MailMessageBuilder(renderer);

        var message = builder
            .SendFrom("Sender", "sender@example.com")
            .To("Recipient", "recipient@example.com")
            .WithSubject("Subject")
            .Append("# Hello")
            .Build();

        renderer.Count.Should().Be(1);
        message.Body.Should().Be("<h1>Hello</h1>");
        message.IsBodyHtml.Should().BeTrue();
    }

    [Fact]
    public void Build_uses_html_body_when_markdown_is_not_present()
    {
        var renderer = new FakeMarkdownRenderer("<h1>Hello</h1>");
        using var builder = new MailMessageBuilder(renderer);

        var message = builder
            .SendFrom("Sender", "sender@example.com")
            .To("Recipient", "recipient@example.com")
            .WithSubject("Subject")
            .WithHtmlBody("<p>Hello</p>")
            .Build();

        message.Body.Should().Be("<p>Hello</p>");
        message.IsBodyHtml.Should().BeTrue();
        renderer.Count.Should().Be(0);
    }

    [Fact]
    public void Build_uses_text_body_when_html_and_markdown_are_not_present()
    {
        using var builder = new MailMessageBuilder(new FakeMarkdownRenderer());

        var message = builder
            .SendFrom("Sender", "sender@example.com")
            .To("Recipient", "recipient@example.com")
            .WithSubject("Subject")
            .WithTextBody("Hello")
            .Build();

        message.Body.Should().Be("Hello");
        message.IsBodyHtml.Should().BeFalse();
    }

    [Fact]
    public void Build_adds_plain_text_alternate_view_when_html_and_text_are_provided()
    {
        using var builder = new MailMessageBuilder(new FakeMarkdownRenderer());

        var message = builder
            .SendFrom("Sender", "sender@example.com")
            .To("Recipient", "recipient@example.com")
            .WithSubject("Subject")
            .WithHtmlBody("<p>Hello</p>")
            .WithTextBody("Hello")
            .Build();

        message.AlternateViews.Should().HaveCount(1);
        message.AlternateViews[0].ContentType.MediaType.Should().Be(MediaTypeNames.Text.Plain);
    }

    [Fact]
    public void Body_prefers_markdown_over_html_and_text()
    {
        var renderer = new FakeMarkdownRenderer("<p>from markdown</p>");

        using var builder = new MailMessageBuilder(renderer);
        var message = builder
            .SendFrom("Sender", "sender@example.com")
            .To("Recipient", "recipient@example.com")
            .WithHtmlBody("<p>from html</p>")
            .WithTextBody("from text")
            .Append("from markdown")
            .Build();

        message.Body.Should().Be("<p>from markdown</p>");
        renderer.Count.Should().Be(1);
    }

    [Fact]
    public void To_with_claims_principal_adds_recipient()
    {
        using var builder = new MailMessageBuilder(new FakeMarkdownRenderer());
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new(AielClaims.GivenName, "Jane"),
            new(AielClaims.FamilyName, "Doe"),
            new(AielClaims.EmailAddress, "jane.doe@example.com")
        ]));

        var message = builder
            .SendFrom("Sender", "sender@example.com")
            .To(principal)
            .WithSubject("Subject")
            .WithTextBody("Hello")
            .Build();

        message.To.Should().ContainSingle();
        message.To.Single().DisplayName.Should().Be("Jane Doe");
        message.To.Single().Address.Should().Be("jane.doe@example.com");
    }

    [Fact]
    public void AddAttachment_with_stream_adds_attachment_and_updates_has_attachments()
    {
        using var builder = new MailMessageBuilder(new FakeMarkdownRenderer());

        builder.AddAttachment("hello.txt", new MemoryStream("Hello"u8.ToArray()), MediaTypeNames.Text.Plain);

        builder.HasAttachments.Should().BeTrue();

        var message = builder
            .SendFrom("Sender", "sender@example.com")
            .To("Recipient", "recipient@example.com")
            .WithSubject("Subject")
            .WithTextBody("Body")
            .Build();

        message.Attachments.Should().ContainSingle();
        message.Attachments[0].Name.Should().Be("hello.txt");
    }

    [Fact]
    public void AddAttachment_throws_for_empty_stream()
    {
        using var builder = new MailMessageBuilder(new FakeMarkdownRenderer());

        Action action = () => builder.AddAttachment("empty.txt", new MemoryStream(), MediaTypeNames.Text.Plain);

        action.Should().Throw<ArgumentException>()
            .WithParameterName("file")
            .WithMessage("*must not be empty*");
    }

    [Fact]
    public void AddAttachment_throws_for_unreadable_stream()
    {
        using var builder = new MailMessageBuilder(new FakeMarkdownRenderer());

        Action action = () => builder.AddAttachment("file.txt", new UnreadableStream([1]), MediaTypeNames.Text.Plain);

        action.Should().Throw<ArgumentException>()
            .WithParameterName("file")
            .WithMessage("*must be readable*");
    }

    [Fact]
    public void Build_throws_when_recipient_is_missing()
    {
        using var builder = new MailMessageBuilder(new FakeMarkdownRenderer());

        Action action = () => builder
            .SendFrom("Sender", "sender@example.com")
            .WithSubject("Subject")
            .WithTextBody("Hello")
            .Build();

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*To:*");
    }

    [Fact]
    public void Build_succeeds_when_reply_to_is_set_and_from_is_missing()
    {
        using var builder = new MailMessageBuilder(new FakeMarkdownRenderer());

        var message = builder
            .ReplyTo("Reply", "reply@example.com")
            .To("Recipient", "recipient@example.com")
            .WithSubject("Subject")
            .WithTextBody("Hello")
            .Build();

        message.ReplyToList.Should().ContainSingle();
        message.ReplyToList.Single().Address.Should().Be("reply@example.com");
        message.From.Should().BeNull();
    }

    [Fact]
    public void Build_throws_when_body_is_missing()
    {
        using var builder = new MailMessageBuilder(new FakeMarkdownRenderer());

        Action action = () => builder
            .SendFrom("Sender", "sender@example.com")
            .To("Recipient", "recipient@example.com")
            .WithSubject("Subject")
            .Build();

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*No message body*");
    }

    [Fact]
    public void Cannot_use_builder_after_build()
    {
        using var builder = new MailMessageBuilder(new FakeMarkdownRenderer());

        builder
            .SendFrom("Sender", "sender@example.com")
            .To("Recipient", "recipient@example.com")
            .WithSubject("Subject")
            .WithTextBody("Hello")
            .Build();

        Action action = () => builder.WithPriority(MailPriority.High);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*already been built*");
    }

    [Fact]
    public void Cannot_use_builder_after_dispose()
    {
        using var builder = new MailMessageBuilder(new FakeMarkdownRenderer());
        builder.Dispose();

        Action action = () => builder.WithPriority(MailPriority.High);

        action.Should().Throw<ObjectDisposedException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void WithSubject_throws_for_null_or_whitespace(String? subject)
    {
        using var builder = new MailMessageBuilder(new FakeMarkdownRenderer());

        Action action = () => builder.WithSubject(subject!);

        action.Should().Throw<ArgumentException>()
            .WithParameterName("subject");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Append_throws_for_null_or_whitespace(String? markdown)
    {
        using var builder = new MailMessageBuilder(new FakeMarkdownRenderer());

        Action action = () => builder.Append(markdown!);

        action.Should().Throw<ArgumentException>()
            .WithParameterName("markdown");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AppendLine_allows_null_or_whitespace(String? markdown)
    {
        using var builder = new MailMessageBuilder(new FakeMarkdownRenderer());

        var body = builder.AppendLine(markdown!).Body();

        body.Should().NotBeNull();
        body.Should().Contain(Environment.NewLine);
    }

    private sealed class UnreadableStream(params Byte[] bytes) : MemoryStream(bytes)
    {
        public override Boolean CanRead => false;
    }
}
