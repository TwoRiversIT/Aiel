# Aiel

The `Aiel` library provides fundamental utilities, value objects, and extensions used across the Aiel Application Framework. This package includes common types for email handling, string manipulation, natural sorting, IP address utilities, and base classes for common patterns.

## Installation

You can install the Aiel package via NuGet Package Manager Console:

```pwsh
Install-Package Aiel
```

Or via .NET CLI:

```pwsh
dotnet add package Aiel
```

## Features

- [DisposableBase - Safe IAsyncDisposable and IDisposable pattern](#disposablebase---safe-iasyncdisposable-and-idisposable-pattern)
- [Value Objects (Email, EmailAddress)](#value-objects)
- [NaturalComparer](#naturalcomparer)
- [Extension Methods](#extension-methods)
- [Comparers](#comparers)
- [Reflection Utilities](#reflection-utilities)

### DisposableBase - Safe IAsyncDisposable and IDisposable pattern

Base class for implementing the dispose pattern correctly:

```csharp
public class MyTestFixture : DisposableBase
{
    private HttpClient? _httpClient;           // IDisposable only
    private DbConnection? _dbConnection;       // IAsyncDisposable

    protected override void Dispose(Boolean disposing)
    {
        if (disposing)
        {
            _httpClient?.Dispose();
            _httpClient = null;
        }

        base.Dispose(disposing);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        if (_dbConnection is not null)
        {
            await _dbConnection.DisposeAsync();
            _dbConnection = null;
        }

        // HttpClient also implements IAsyncDisposable in modern .NET
        if (_httpClient is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else
        {
            _httpClient?.Dispose();
        }

        _httpClient = null;

        await base.DisposeAsyncCore();
    }
}
```

**How does a derived class know which dispose to override?**

| Scenario                                                            | Override             |
| ------------------------------------------------------------------- | -------------------- |
| Async resources (e.g., `IAsyncDisposable` fields, async streams)    | `DisposeAsyncCore()` |
| Sync-only resources (e.g., `IDisposable` fields, unmanaged handles) | `Dispose(Boolean)`   |
| Mixed resources                                                     | Both methods         |

The key insight: when `DisposeAsync()` is called, it calls `DisposeAsyncCore()` first (cleaning
managed resources async), then `Dispose(false)` (cleaning only unmanaged resources). This
prevents double-disposal of managed resources.

### Value Objects

#### Email

Strongly-typed email address value object with validation:

```csharp
using Aiel.Emailing;

var email = new Email("user@example.com");
Console.WriteLine(email.Domain);  // "example.com"
Console.WriteLine(email.LocalPart);  // "user"
```

#### EmailAddress

Combines display name and email address (compatible with `System.Net.Mail.MailAddress`):

```csharp
using Aiel.Emailing;

var emailAddress = new EmailAddress("John Doe", "john@example.com");
var mailAddress = (MailAddress)emailAddress;  // Implicit conversion

// Or parse from string
var parsed = new EmailAddress("John Doe <john@example.com>");
```

### NaturalComparer

Compares strings using natural (human-friendly) ordering where numbers are compared numerically:

```csharp
using Aiel;

var files = new[] { "file10.txt", "file2.txt", "file1.txt" };
Array.Sort(files, new NaturalComparer<String>());
// Result: ["file1.txt", "file2.txt", "file10.txt"]
```

### Extension Methods

#### StringExtensions

Common string manipulation utilities:

```csharp
using Aiel.Extensions;

// Left substring
var text = "Hello World";
var left = text.Left(5);  // "Hello"

// Truncate with ellipsis
var truncated = text.Truncate(8);  // "Hello ..."

// Remove whitespace
var clean = "  lots   of   spaces  ".RemoveWhitespace();  // "lotsofspaces"
```

#### IPAddressExtensions

IP address utilities:

```csharp
using Aiel.Extensions;
using System.Net;

var ip = IPAddress.Parse("192.168.1.100");
var network = IPAddress.Parse("192.168.1.0");

if (ip.IsInSameSubnet(network, 24))
{
    Console.WriteLine("Same subnet");
}
```

#### MiscExtensions

Miscellaneous extension methods:

```csharp
using Aiel.Extensions;

// Clamp values
var value = 150;
var clamped = value.Clamp(0, 100);  // 100

// Visit exception hierarchy
try
{
    // code
}
catch (Exception ex)
{
    ex.Visit(e => Console.WriteLine($"Exception: {e.Message}"));
}
```

### Comparers

#### IPAddressComparer

Compares IP addresses for sorting:

```csharp
using Aiel.Net;

var addresses = new[]
{
    IPAddress.Parse("192.168.1.100"),
    IPAddress.Parse("10.0.0.1"),
    IPAddress.Parse("192.168.1.10")
};

Array.Sort(addresses, new IPAddressComparer());
```

#### EnumerableComparer

Compares sequences element-by-element:

```csharp
using Aiel;

var comparer = new EnumerableComparer<Int32>();
var list1 = new[] { 1, 2, 3 };
var list2 = new[] { 1, 2, 4 };

var result = comparer.Compare(list1, list2);  // < 0
```

### Reflection Utilities

#### ReflectionUtils

Extract constants and metadata using reflection:

```csharp
using Aiel.Reflection;

public static class ErrorCodes
{
    public const String NotFound = "NOT_FOUND";
    public const String Unauthorized = "UNAUTHORIZED";
}

// Get all constant values
var codes = ReflectionUtils.GetConstants<String>(typeof(ErrorCodes));
// Returns: ["NOT_FOUND", "UNAUTHORIZED"]
```

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.
