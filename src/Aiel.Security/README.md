# Aiel.Security

The `Aiel.Security` library provides extension methods for working with claims-based authentication in .NET applications. It simplifies extracting and working with user claims from `ClaimsPrincipal` and `Claim` collections.

## Installation

You can install the Aiel.Security package via NuGet Package Manager:

```pwsh
Install-Package Aiel.Security
```

Or via the .NET CLI:

```pwsh
dotnet add package Aiel.Security
```

## Features

### ClaimsPrincipalExtensions

Extension methods for `ClaimsPrincipal` that provide convenient access to common user information:

- **FullName**: Combines GivenName and FamilyName claims
- **Email**: Retrieves the email address claim
- **EmailAddress**: Returns a strongly-typed `EmailAddress` value object
- **ZoneInfo**: Retrieves timezone information (defaults to `AielDefaults.TimeZone`)

### ClaimExtensions

Extension methods for working with `IEnumerable<Claim>` collections:

- **FirstOrDefaultString**: Extracts a claim value as a String with optional default
- **FirstOrDefaultInt32**: Extracts a claim value as an Int32 with optional default
- **FirstOrDefaultGuid**: Extracts a claim value as a Guid with optional default
- **FirstOrDefault**: Finds a claim by type (case-insensitive)

### AielClaims

Standard claim type constants for the Aiel framework:

- **ZoneInfo**: `"tr_timezone"` - User's timezone identifier
- **GivenName**: `"tr_given_name"` - User's first name
- **FamilyName**: `"tr_family_name"` - User's last name
- **EmailAddress**: `"tr_email_address"` - User's email address

## Usage

### Extracting User Information from ClaimsPrincipal

```csharp
using Aiel.Security;

public class UserProfileService
{
    public UserProfile GetProfile(ClaimsPrincipal user)
    {
        return new UserProfile
        {
            FullName = user.FullName(),
            Email = user.Email(),
            EmailAddress = user.EmailAddress(),
            TimeZone = user.ZoneInfo()
        };
    }
}
```

### Working with Claims Collections

```csharp
using Aiel.Security;

public class ClaimsService
{
    public void ProcessClaims(IEnumerable<Claim> claims)
    {
        // Extract string values
        var givenName = claims.FirstOrDefaultString(AielClaims.GivenName, "Unknown");

        // Extract typed values
        var userId = claims.FirstOrDefaultGuid("user_id");
        var age = claims.FirstOrDefaultInt32("age", 0);

        // Get full name from claims
        var fullName = claims.FullName();

        // Get timezone or default
        var timezone = claims.ZoneInfo(); // Returns AielDefaults.TimeZone if not found
    }
}
```

### Adding Claims to Identity

```csharp
using System.Security.Claims;
using Aiel.Security;

public class AuthenticationService
{
    public ClaimsPrincipal CreatePrincipal(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(AielClaims.GivenName, user.FirstName),
            new Claim(AielClaims.FamilyName, user.LastName),
            new Claim(AielClaims.EmailAddress, user.Email),
            new Claim(AielClaims.ZoneInfo, user.TimeZone ?? "America/New_York"),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var identity = new ClaimsIdentity(claims, "Aiel");
        return new ClaimsPrincipal(identity);
    }
}
```

### ASP.NET Core Integration

```csharp
using Microsoft.AspNetCore.Mvc;
using Aiel.Security;

[ApiController]
[Route("api/[controller]")]
public class ProfileController : ControllerBase
{
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        return Ok(new
        {
            FullName = User.FullName(),
            Email = User.Email(),
            TimeZone = User.ZoneInfo()
        });
    }
}
```

## Best Practices

### Use Strongly-Typed Constants

Always use `AielClaims` constants instead of magic strings:

```csharp
// Good
var email = claims.FirstOrDefaultString(AielClaims.EmailAddress);

// Avoid
var email = claims.FirstOrDefaultString("tr_email_address");
```

### Provide Sensible Defaults

Use the default parameter to handle missing claims gracefully:

```csharp
// Provide default for optional claims
var timezone = claims.FirstOrDefaultString(AielClaims.ZoneInfo, "UTC");

// For required claims, validate separately
var userId = claims.FirstOrDefaultGuid("user_id");
if (userId == Guid.Empty)
{
    throw new UnauthorizedAccessException("User ID claim is required");
}
```

### Use EmailAddress Value Object

Prefer the `EmailAddress` value object over raw strings:

```csharp
// Good - strongly typed
var emailAddress = user.EmailAddress();
await emailSender.SendAsync(emailAddress, "Welcome!");

// Less ideal - stringly typed
var email = user.Email();
await emailSender.SendAsync(new EmailAddress("", email), "Welcome!");
```

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.
