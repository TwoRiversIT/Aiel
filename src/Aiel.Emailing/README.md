# Aiel.Emailing

Provides strongly-typed `Email` and `EmailAddress` value objects, email validation, and a fluent `MailMessageBuilder` for composing emails. Defines an `IEmailSender` interface for sending email messages. Designed to be used with dependency injection and can be easily mocked for testing.

## Installation

You can install the Aiel.Emailing package via NuGet Package Manager Console:
```pwsh
Install-Package Aiel.Emailing
```
Or via .NET CLI:
```pwsh
dotnet add package Aiel.Emailing
```

## Features

### Value Objects

- **Email**: Strongly-typed email address value object
- **EmailAddress**: Combines display name and email address (compatible with `System.Net.Mail.MailAddress`)

### Email Validation

Multiple validation strategies:
- **MailAddressEmailValidator**: Uses .NET's built-in `MailAddress` parsing
- **W3CEmailValidator**: Validates against W3C email specification
- **StrictEmailValidator**: Strict validation with additional rules
- **PatternEmailValidator**: Regex-based validation
- **ParsingEmailValidator**: Configurable parsing validator

### MailMessageBuilder

Fluent API for building email messages with:
- Markdown-to-HTML rendering
- Multiple recipients (To, CC, BCC)
- Priority and importance settings
- Attachments
- Plain text and HTML bodies
- Integration with `ClaimsPrincipal` for user emails

### FluentValidation Integration

- **EmailAddressPropertyValidator**: Validates email properties in FluentValidation rules

## Usage

### Using MailMessageBuilder (Recommended)

```csharp
using Aiel.Emailing;

public class EmailService
{
	private readonly MailMessageBuilder _builder;
	private readonly IEmailSender _sender;

	public EmailService(MailMessageBuilder builder, IEmailSender sender)
	{
		_builder = builder;
		_sender = sender;
	}

	public async Task SendWelcomeEmailAsync(ClaimsPrincipal user)
	{
		var message = _builder
			.SendFrom("Support", "support@example.com")
			.To(user)  // Automatically extracts name and email from claims
			.WithSubject("Welcome!")
			.WithPriority(MailPriority.High)
			.AppendLine("# Welcome to Our Service")
			.AppendLine()
			.AppendLine("We are excited to have you on board!")
			.AppendLine()
			.Append("Get started by visiting your [dashboard](https://example.com/dashboard).")
			.Build();

		await _sender.SendAsync(message);
	}
}
```

### Implementing IEmailSender

To use the Aiel.Emailing package, create an implementation of the `IEmailSender` interface to send emails. Here's an example using SMTP:

```csharp
using System.Net.Mail;
using Aiel.Emailing;

public class SmtpEmailSender : IEmailSender
{
	private readonly SmtpClient _smtpClient;

	public SmtpEmailSender(SmtpClient smtpClient)
	{
		_smtpClient = smtpClient;
	}

	public async Task SendAsync(MailMessage message, CancellationToken cancellationToken = default)
	{
		await _smtpClient.SendMailAsync(message, cancellationToken);
	}
}
```

### Email Validation

```csharp
using Aiel.Emailing;

public class RegistrationService
{
	private readonly IEmailValidator _validator;

	public RegistrationService(IEmailValidator validator)
	{
		_validator = validator;
	}

	public Boolean IsValidEmail(String email)
	{
		return _validator.IsValid(email);
	}
}
```

### Dependency Injection Setup

```csharp
using Microsoft.Extensions.DependencyInjection;
using Aiel.Emailing;

// Configure email options
services.Configure<EmailOptions>(options =>
{
	options.DefaultFromAddress = "noreply@example.com";
	options.DefaultFromName = "Example Service";
});

// Register validators
services.AddSingleton<IEmailValidator, W3CEmailValidator>();

// Register builder (scoped for per-request isolation)
services.AddScoped<MailMessageBuilder>();

// Register email sender
services.AddSingleton<IEmailSender, SmtpEmailSender>();

// Configure SMTP client
services.AddSingleton<SmtpClient>(sp => new SmtpClient
{
	Host = "smtp.example.com",
	Port = 587,
	EnableSsl = true,
	Credentials = new NetworkCredential("username", "password")
});
```

### FluentValidation Integration

```csharp
using FluentValidation;
using Aiel.Emailing;

public class UserRegistrationRequest
{
	public String Email { get; set; }
	public String Name { get; set; }
}

public class UserRegistrationValidator : AbstractValidator<UserRegistrationRequest>
{
	public UserRegistrationValidator()
	{
		RuleFor(x => x.Email)
			.NotEmpty()
			.SetValidator(new EmailAddressPropertyValidator());
	}
}
```

## Best Practices

### Use MailMessageBuilder

The `MailMessageBuilder` provides a fluent API and handles common tasks like markdown rendering:

```csharp
// Good - fluent and readable
var message = builder
	.To("John Doe", "john@example.com")
	.WithSubject("Invoice #12345")
	.AppendLine("## Invoice Details")
	.Build();

// Avoid - manual construction
var message = new MailMessage();
message.To.Add(new MailAddress("john@example.com", "John Doe"));
message.Subject = "Invoice #12345";
message.Body = "<h2>Invoice Details</h2>";
message.IsBodyHtml = true;
```

### Leverage Value Objects

Use `Email` and `EmailAddress` types instead of strings:

```csharp
// Good - strongly typed
public void SendNotification(EmailAddress recipient)
{
	// Type safety ensures valid email addresses
}

// Avoid - stringly typed
public void SendNotification(String recipient)
{
	// No compile-time guarantee of validity
}
```

### Choose the Right Validator

Different validators for different needs:

```csharp
// Strict validation for user registration
services.AddSingleton<IEmailValidator, StrictEmailValidator>();

// Lenient validation for email imports
services.AddSingleton<IEmailValidator, MailAddressEmailValidator>();

// W3C-compliant validation
services.AddSingleton<IEmailValidator, W3CEmailValidator>();
```

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.
