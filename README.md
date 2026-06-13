# Two Rivers Aiel

![NuGet Version](https://img.shields.io/nuget/v/Aiel?link=https%3A%2F%2Fgithub.com%2Ftwotivers%2FAiel)

A comprehensive collection of NuGet packages for building modern .NET applications following Clean Architecture and Domain-Driven Design principles.

## Architecture Principles

The Aiel Application Framework follows these core patterns:

- **Clean Architecture** - Clear separation between Domain, Application, Infrastructure, and Presentation layers
- **Domain-Driven Design** - Rich domain models with value objects, entities, and aggregates
- **CQRS** - Separate read and write models where appropriate
- **Result Pattern** - Explicit error handling without exceptions for control flow
- **Specification Pattern** - Encapsulate query logic in reusable, composable specifications
- **Strong Typing** - Prefer value objects and enums over primitive obsession
- **Performance** - Optimized for high-performance scenarios (sequential GUIDs, efficient comparers)
- **Testability** - All components designed for easy testing with dependency injection

The following documents are the basis for the framework. While not required reading, they document the core philosophy and architectural goals that guide the framework implementation.

- [Conceptual Overview](./docs/ConceptualOverview.md)
- [Architecture](./docs/ArchitectureOverview.md)
- [Domain Primitives Contract](./docs/features/ddd/DomainPrimitives.md)
- [Aggregate Root Discussion](./docs/features/ddd/AggregateRootDiscussion.md)

## Core Packages

### [Aiel](./src/Aiel/README.md)

Fundamental utilities, value objects, and extensions used across the framework.

**Features**:

- `DisposableBase` - Safe IAsyncDisposable and IDisposable pattern implementation
- `Email` and `EmailAddress` - Strongly-typed email value objects
- `NaturalComparer<T>` - Human-friendly string sorting
- Extension methods for String, IPAddress, and more
- `ReflectionUtils` - Reflection utilities for extracting constants and metadata
- IP address comparers and enumerable comparers

### [Aiel.Results](./src/Aiel.Results/README.md)

Result Pattern implementation for representing operation outcomes without exceptions.

**Features**:

- `Result` and `Result<T>` - Type-safe operation results
- `Error` types with standard error codes (NotFound, Conflict, Validation, etc.)
- Functional operations: `Map`, `Bind`, `Match`, `Tap`
- Async support with `MapAsync`, `BindAsync`, `MatchAsync`
- Trimming and AOT compatible
- JSON serialization support

### Aiel.Results.AspNetCore

ASP.NET Core integration for the Result Pattern with ProblemDetails support.

**Features**:

- Automatic conversion of `Result<T>` to HTTP responses
- ProblemDetails integration for standardized error responses
- Minimal API and MVC controller support

### [Aiel.Results.Generators](./src/Aiel.Results.Generators/README.md)

Source generators for the Result Pattern (see [Aiel.Results](./src/Aiel.Results/README.md) for documentation).

## Security & Identity

### [Aiel.Security](./src/Aiel.Security/README.md)

Claims-based authentication extensions for extracting and working with user claims.

**Features**:

- `ClaimsPrincipalExtensions` - Extract user information (FullName, Email, TimeZone)
- `ClaimExtensions` - Type-safe claim value extraction (String, Int32, Guid)
- `AielClaims` - Standard claim type constants
- `EmailAddress` integration with claims

## Data Access

### [Aiel.DataAccess.Dapper](./src/Aiel.DataAccess.Dapper/README.md)

Column mapping for Dapper enabling property-to-column name mapping via attributes.

**Features**:

- `[HasColumnMaps]` and `[ColumnName]` attributes for declarative mapping
- `ColumnMapper` for automatic mapping discovery from assemblies
- Type-safe mapping without manual configuration

### [Aiel.EntityFrameworkCore](./src/Aiel.EntityFrameworkCore/README.md)

Entity Framework Core utilities and extensions (in development).

### [Aiel.Application](./src/Aiel.Application/README.md)

Application-layer contracts for commands, queries, specifications, and read-side shaping concerns.

**Features**:

- `ISpecification<T>` for pure business-rule composition
- `IQuerySpecification<T>` for provider-translatable read-side filtering
- `ICommand`, `IQuery<TResult>`, handlers, and dispatchers
- `PageRequest`, `SortRequest`, and `PagedResult<T>` for read-side shaping

### [Aiel.EntityFrameworkCore](./src/Aiel.EntityFrameworkCore/README.md)

Entity Framework Core integration for repositories, strong IDs, and read-side query specifications.

**Features**:

- `QuerySpecificationRepository<TEntity, TDbContext>` for read-side filtering
- `QuerySpecificationEvaluator<TEntity>` for applying query specs, sorting, and paging
- EF Core extensions and infrastructure integration points
- Async query execution over provider-translatable specifications

## ID Generation & GUIDs

### [Aiel.IdGeneration](./src/Aiel.IdGeneration/README.md)

Unique identifier generation for various scenarios including database-optimized sequential GUIDs.

**Features**:

- `TimeBasedIdGenerator` - Time-based IDs with Base36 encoding
- `KeyGenerator` - Cryptographically secure random keys
- `CombGuid` - Factory for database-specific sequential GUIDs
- `SqlServerCombGuid` - SQL Server-optimized sequential GUIDs
- `PostgreSqlCombGuid` - PostgreSQL/MySQL/Oracle-optimized sequential GUIDs
- `DatabaseType` enum for selecting appropriate GUID strategy
- `Base36` encoding/decoding utilities

## Messaging & Email

### [Aiel.Emailing](./src/Aiel.Emailing/README.md)

Email validation, composition, and sending abstractions.

**Features**:

- `MailMessageBuilder` - Fluent API for building emails with Markdown support
- `IEmailSender` - Abstraction for sending emails
- Multiple email validators (W3C, Strict, Pattern-based, Parsing)
- `Email` and `EmailAddress` value objects
- FluentValidation integration

### [Aiel.InternetTypes](./src/Aiel.InternetTypes/README.md)

Internet-related value objects and types.

**Features**:

- `DomainName` - Strongly-typed domain names
- `Serial` - DNS serial numbers with automatic incrementing
- `TTL` - Time-to-live values for DNS records
- `Label` - DNS label validation

## Testing

### [Aiel.Testing](./src/Aiel.Testing/README.md)

Integration testing framework with dependency injection support.

**Features**:

- `IntegrationTestFixture` - Base class for xUnit fixtures
- `IntegrationTestBase<TSut, TFixture>` - Base class for integration tests
- Service scope isolation per test
- Configuration management (appsettings.Testing.json)
- Lazy SUT initialization
- Proper lifetime management for fixtures and scopes

## Installation

All packages are available on NuGet. Install via Package Manager Console:

```pwsh
Install-Package Aiel
Install-Package Aiel.Results
Install-Package Aiel.IdGeneration
# ... etc
```

Or via .NET CLI:

```pwsh
dotnet add package Aiel
dotnet add package Aiel.Results
dotnet add package Aiel.IdGeneration
# ... etc
```

## Quick Start

### Using Result Pattern

```csharp
using Aiel.Results;

public class UserService
{
    public Result<User> GetById(Int32 id)
    {
        var user = _repository.Find(id);

        if (user is null)
            return Error.NotFound($"User with ID {id} was not found");

        return user; // Implicit conversion to Result<User>
    }
}
```

### Using Specifications

```csharp
using Aiel.Application.Specifications;

public sealed class ActiveCustomersSpecification : QuerySpecification<Customer>
{
    public ActiveCustomersSpecification()
        : base(customer => customer.IsActive)
    {
    }
}

// Usage
var spec = new ActiveCustomersSpecification();
var customers = await _repository.FindAsync(spec);
```

### Using Database-Specific GUIDs

```csharp
using Aiel.IdGeneration;

// For SQL Server
var sqlGuid = CombGuid.NewGuid(DatabaseType.SqlServer);

// For PostgreSQL
var pgGuid = CombGuid.NewGuid(DatabaseType.PostgreSql);
```

### Using Claims Extensions

```csharp
using Aiel.Security;

[ApiController]
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

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request with your changes.

### Building the Solution

To build the solution, ensure you have the .NET SDK installed. Then run the following command in the root directory:

```bash
dotnet build
```

Yeah, it is that simple.

### Running Tests

```bash
dotnet test
```

Currently includes 460 passing tests across all projects.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.
