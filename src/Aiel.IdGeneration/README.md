# Aiel.IdGeneration

The `Aiel.IdGeneration` library provides tools for generating unique identifiers tailored to various scenarios, including time-based IDs, cryptographic keys, and database-optimized sequential GUIDs. These generators ensure uniqueness, security, and optimal database performance across distributed systems.

## Table of Contents

- [Installation](#installation)
- [Components](#components)
  - [TimeBasedIdGenerator](#timebasedidgenerator)
  - [KeyGenerator](#keygenerator)
  - [CombGuid](#combguid)
  - [SqlServerCombGuid](#sqlservercombguid)
  - [PostgreSqlCombGuid](#postgresqlcombguid)
  - [Base36 Encoding](#base36-encoding)
- [Usage](#usage)
- [License](#license)

## Installation

You can install the Aiel.IdGeneration package via NuGet Package Manager:

```pwsh
Install-Package Aiel.IdGeneration
```

Or via the .NET CLI:

```pwsh
dotnet add package Aiel.IdGeneration
```

## Components

### TimeBasedIdGenerator

Generates unique identifiers based on the current timestamp, encoded in Base36 format. This generator is thread-safe and ensures that IDs generated within the same millisecond are incremented sequentially.

**Key Features**:
- Time-based uniqueness ensuring chronological ordering
- Base36 encoding for compact, alphanumeric strings
- Thread-safe with locking mechanisms
- Sequential increment for concurrent generation

**Usage**:

```csharp
using Aiel.IdGeneration;

// Register as singleton in DI container
services.AddSingleton<IIdGenerator, TimeBasedIdGenerator>(sp => 
	new TimeBasedIdGenerator(TimeProvider.System));

// Generate unique IDs
var id = idGenerator.NextId();  // Returns: "ABC123XYZ"

// Decode back to timestamp
var timestamp = TimeBasedIdGenerator.Decode(id);
Console.WriteLine($"Created at: {timestamp}");
```

### KeyGenerator

Generates cryptographically secure random keys using uppercase letters and numbers (A-Z, 0-9). Ideal for API keys, tokens, and other security-sensitive identifiers.

**Key Features**:
- Cryptographically secure using `RandomNumberGenerator`
- Configurable length
- Uppercase alphanumeric characters only (excludes lowercase for clarity)
- Implements `IDisposable` for proper resource cleanup

**Usage**:

```csharp
using Aiel.IdGeneration;

using var keyGen = new KeyGenerator();

// Generate a 32-character key
var apiKey = keyGen.Generate(32);  // Returns: "K7M9X2QWERTY4P8N3VCXZ1ASJKLF6H"
```

### CombGuid

Factory for generating database-optimized sequential GUIDs. Uses `DatabaseType` enum to select the appropriate implementation for your database engine.

**Database-Specific Optimization**: Different databases compare GUID bytes in different orders, so sequential GUIDs MUST be optimized for your specific database to avoid fragmentation.

**Usage**:

```csharp
using Aiel.IdGeneration;

// For SQL Server
var sqlGuid = CombGuid.NewGuid(DatabaseType.SqlServer);

// For PostgreSQL, MySQL, Oracle
var pgGuid = CombGuid.NewGuid(DatabaseType.PostgreSql);
```

### SqlServerCombGuid

Sequential GUID generator optimized for SQL Server's non-standard byte comparison order. Places timestamp in the last 6 bytes (10-15) which SQL Server compares first.

**When to Use**: SQL Server databases using UNIQUEIDENTIFIER primary keys or clustered indexes.

**Benefits**:
- Reduces index fragmentation
- Improves INSERT performance
- Maintains chronological ordering in SQL Server

**Usage**:

```csharp
using Aiel.IdGeneration;

var guid = SqlServerCombGuid.NewGuid();
// Example: "a1b2c3d4-e5f6-4789-abcd-123456789abc"
// Last 6 bytes contain timestamp for SQL Server ordering
```

### PostgreSqlCombGuid

Sequential GUID generator optimized for PostgreSQL, MySQL, Oracle, and other databases using RFC 4122 lexicographic ordering. Places timestamp in the first 6 bytes (0-5).

**When to Use**: PostgreSQL, MySQL, Oracle, or any database using standard left-to-right GUID comparison.

**Benefits**:
- Reduces index fragmentation
- Improves INSERT performance
- Maintains chronological ordering in standard-compliant databases

**Usage**:

```csharp
using Aiel.IdGeneration;

var guid = PostgreSqlCombGuid.NewGuid();
// Example: "123456789abc-e5f6-4789-abcd-a1b2c3d4"
// First 6 bytes contain timestamp for lexicographic ordering
```

### Base36 Encoding

Internal utility for encoding/decoding Int64 values to/from Base36 format. Used by `TimeBasedIdGenerator` for compact ID representation.

**Features**:
- Encodes Int64 to alphanumeric strings (0-9, A-Z)
- Decodes Base36 strings back to Int64
- Supports negative values
- Overflow detection

## Usage

### Complete Example: Entity with Database-Specific GUIDs

```csharp
using Microsoft.EntityFrameworkCore;
using Aiel.IdGeneration;

public class Order
{
	// Use database-specific GUID generation
	public Guid Id { get; private set; }
	public String OrderNumber { get; private set; }

	public Order(DatabaseType dbType)
	{
		Id = CombGuid.NewGuid(dbType);
		OrderNumber = GenerateOrderNumber();
	}

	private String GenerateOrderNumber()
	{
		using var keyGen = new KeyGenerator();
		return keyGen.Generate(12);
	}
}

// In your DbContext configuration
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
	modelBuilder.Entity<Order>(entity =>
	{
		// SQL Server
		entity.Property(e => e.Id)
			  .HasDefaultValueSql("NEWSEQUENTIALID()");

		// Or in code (preferred for testing)
		entity.HasData(new Order(DatabaseType.SqlServer));
	});
}
```

### Dependency Injection Setup

```csharp
using Microsoft.Extensions.DependencyInjection;
using Aiel.IdGeneration;

services.AddSingleton<IIdGenerator, TimeBasedIdGenerator>(sp => 
	new TimeBasedIdGenerator(TimeProvider.System));

services.AddScoped<IKeyGenerator, KeyGenerator>();

// Store database type in configuration
services.Configure<DatabaseOptions>(options => 
	options.Type = DatabaseType.PostgreSql);
```

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.
