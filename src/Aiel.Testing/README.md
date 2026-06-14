# Aiel.Testing.Integration

## Overview

`Aiel.Testing` is an testing framework designed for .NET applications that rely on dependency
injection. It simplifies the process of setting up and tearing down test environments, managing
service lifetimes, and isolating dependencies for reliable integration tests.

This framework leverages xUnit's `IClassFixture<T>` pattern to share expensive setup operations across
multiple tests while ensuring proper isolation between individual test runs through scoped service
providers.

## Installation

To use the `Aiel.Testing` framework, install the NuGet package:

```bash
dotnet add package Aiel.Testing
```

Or via the Package Manager Console:

```pwsh
Install-Package Aiel.Testing
```

## Key Concepts

### Test Fixture Lifecycle

The framework uses a two-level lifecycle model:

1. **Fixture Level** - Created once per test class and shared across all tests
   - Hosts the application container (`IHost`)
   - Loads configuration (typically from `appsettings.Testing.json`)
   - Registers all services in the DI container

2. **Test Level** - Created for each individual test method

- Creates a new service scope per test via `InitializeAsync()`
- Provides isolated instances of scoped services
- Disposes the scope after test completion via `DisposeAsync()`

This design ensures expensive operations (like loading configuration or setting up database connections)
happen once, while each test gets fresh instances of scoped dependencies for proper isolation.

### Why Lazy Initialization for SUT?

The System Under Test (SUT) is initialized lazily (`Lazy<TSut>`) to support tests that don't need it:

```csharp
protected TSut SUT => _lazySut.Value;
```

This allows you to write tests that use the fixture's services directly without requiring the SUT to
be constructible. For example, repository tests might only need the repository from the service
provider, not a complete service layer.

## Basic Usage

### Step 1: Create a Test Fixture

Create a fixture class that derives from `IntegrationTestFixture` and configures your services:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Aiel.Testing;

namespace Aiel.Testing.Integration.Examples.Customers;

public class CustomersFixture : IntegrationTestFixture
{
    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register data access
        services.AddDbContext<CustomerDbContext>(options =>
            options.UseInMemoryDatabase("CustomerTests")
                   .EnableSensitiveDataLogging(true));

        // Register repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        // Register services being tested: 
        // IMPORTANT: Register the implementation, not the interface.
        services.AddScoped<CustomerApplicationService>();
    }

    // One-time initialization per test class
    protected override async ValueTask InitializeFixtureAsync(IServiceProvider services)
    {
        // Ensure database schema exists
        var dbContext = services.GetRequiredService<CustomerDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }
}
```

**Why override `ConfigureServices`?**

This is where you register all the services your application needs. It should mirror your application's
startup configuration but allows you to substitute implementations for testing (e.g., use an in-memory
database instead of a real one).

### Step 2: Create a Test Base Class (Optional)

For better organization, create an intermediate test base class for your fixture:

```csharp
using System.Diagnostics.CodeAnalysis;

namespace Aiel.Testing.Integration.Examples.Customers;

public class CustomerTestBase<TSUT>(
    CustomersFixture fixture)
    : IntegrationTestBase<TSUT, CustomersFixture>(fixture)
    where TSUT : class
{
}
```

### Step 3: Create Your Test Class

Create a test class that inherits from your test base (or directly from `IntegrationTestBase<TSut, TFixture>`):

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace Aiel.Testing.Integration.Examples.Customers;

public class CustomerServiceTests(CustomersFixture fixture)
    : CustomerTestBase<CustomerApplicationService>(fixture)
{
    [Fact]
    public async Task CreateCustomer_WithValidData_ShouldReturn_Success()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var command = new CreateCustomerCommand(id, "Acme Corporation", "contact@acme.com");
        var result = await SUT.CreateCustomerAsync(command, CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value);

        var repository = Services.GetRequiredService<ICustomerRepository>();
        var created = await repository.GetByIdAsync(id, CancellationToken);
        Assert.Equal("Acme Corporation", created.Name);
        Assert.Equal("contact@acme.com", created.Email);
    }
}
```

**Why pass the fixture to the base constructor?**

The base class needs the fixture to access the service provider and configuration. The xUnit framework
automatically calls `InitializeAsync()` before each test and `DisposeAsync()` after each test,
ensuring each test gets a fresh service scope.

### Step 4: Create Configuration File

Create an `appsettings.Testing.json` file in your test project output directory:

```json
{
  "ConnectionStrings": {
    "TestDatabase": "Server=(localdb)\\mssqllocaldb;Database=MyApp_Tests;Trusted_Connection=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

**Why `appsettings.Testing.json`?**

This file should be excluded from source control (via `.gitignore`) to prevent sensitive data like
connection strings or API keys from being committed. Each developer maintains their own local
testing configuration. The framework automatically loads this file using the `"Testing"` environment
name.

## Advanced Usage

### Custom Fixture Initialization

Override `InitializeFixtureAsync` to perform one-time setup after the host is built but before tests run:

```csharp
public class CustomersFixture : IntegrationTestFixture
{
    protected override async ValueTask InitializeFixtureAsync(IServiceProvider services)
    {
        // Ensure database exists and is migrated
        var dbContext = services.GetRequiredService<CustomerDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }
}
```

**Why use `InitializeFixtureAsync`?**

This runs once per test class (not per test), making it ideal for expensive operations like database
migrations or seeding test data. The service provider passed to this method already has a scope created,
so you can directly resolve services without creating your own scope.

### Accessing Services Directly

You can access services directly without using the SUT:

```csharp
[Fact]
public async Task GetCustomer_WhenExists_ShouldReturnCustomer()
{
    // Arrange - Create test data using the repository directly
    var id = Guid.NewGuid();
    var repository = Services.GetRequiredService<ICustomerRepository>();
    var customer = new Customer(id, "Test Corp");
    await repository.CreateAsync(customer, CancellationToken);

    // Act - Use the SUT to retrieve it
    var query = new GetCustomerByIdQuery(id);
    var result = await SUT.GetCustomerByIdAsync(query, CancellationToken);

    // Assert
    Assert.NotNull(result);
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value);
    Assert.Equal(id, result.Value.Id);
    Assert.Equal("Test Corp", result.Value.Name);
}
```

**Why access services directly?**

For testing lower-level components (like repositories), you don't need a full service layer. Accessing
services directly keeps tests focused and reduces unnecessary dependencies.

### Custom Configuration Loading

Override `ConfigureBuilder` to customize how configuration is loaded:

```csharp
public class CustomConfigurationFixture : IntegrationTestFixture
{
    protected override void ConfigureBuilder(IConfigurationBuilder builder)
    {
        base.ConfigureBuilder(builder); // Load appsettings.Testing.json
        
        // Add additional configuration sources
        builder.AddJsonFile("appsettings.TestOverrides.json", optional: true);
        builder.AddEnvironmentVariables(prefix: "MYAPP_");
        builder.AddInMemoryCollection(new Dictionary<String, String>
        {
            ["TestMode"] = "true",
            ["ApiKey"] = "test-key-12345"
        });
    }
}
```

**Why override `ConfigureBuilder`?**

Different test scenarios may need different configurations. For example, you might want to test with
various API endpoints, feature flags, or environment variables.

### Modifying Host Settings

Override `ConfigureSettings` to customize the host application builder:

```csharp
public class CustomHostFixture : IntegrationTestFixture
{
    protected override void ConfigureSettings(HostApplicationBuilderSettings settings)
    {
        settings.EnvironmentName = "IntegrationTest";
        settings.ApplicationName = "MyApp.Tests";
        settings.DisableDefaults = true; // Disable default configuration sources
    }
}
```

**Why override `ConfigureSettings`?**

This gives you control over the host's fundamental settings before it's created. You might want to
change the environment name to load different configuration files or disable defaults for complete
control over configuration sources.

### Using Configuration in Tests

Access configuration values through the `Configuration` property:

```csharp
[Fact]
public void Configuration_ShouldContainTestConnectionString()
{
    // Act
    var connectionString = Configuration.GetConnectionString("TestDatabase");

    // Assert
    Assert.NotNull(connectionString);
    Assert.Contains("MyApp_Tests", connectionString);
}
```

**Why expose `Configuration`?**

Tests sometimes need to verify that configuration is loaded correctly or use configuration values
to set up test data that matches expected formats.

## Working with Scoped Services

### Understanding Service Scopes

Each test gets its own service scope, which means:

```csharp
[Fact]
public void EachTest_GetsIsolatedScopedServices()
{
    // This scope is created by InitializeAsync() before the test runs
    var service1 = Services.GetRequiredService<CustomerApplicationService>();
    var service2 = Services.GetRequiredService<CustomerApplicationService>();
    
    // Same instance within the same test
    Assert.Same(service1, service2);
    
    // Different test = different scope = different instance
}
```

**Why use scopes?**

Scopes prevent tests from interfering with each other. If one test modifies a scoped service's state,
that change won't affect other tests. This is crucial for DbContext instances, which track entity
changes and shouldn't be shared across tests.

> NOTE: This does not protect against shared resources like databases. You may need to clean up data
> in your tests to avoid cross-test pollution.

### Testing Scoped vs Singleton Services

```csharp
public class ServiceLifetimeFixture : IntegrationTestFixture
{
    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<ICacheService, InMemoryCacheService>();
        services.AddScoped<IOrderService, OrderService>();
    }
}

public class ServiceLifetimeTests : IntegrationTestBase<IOrderService, ServiceLifetimeFixture>
{
    public ServiceLifetimeTests(ServiceLifetimeFixture fixture) : base(fixture) { }

    [Fact]
    public void SingletonServices_AreSameAcrossTests()
    {
        // Singleton services are shared across all tests in the class
        var cache = Services.GetRequiredService<ICacheService>();
        
        // This data would persist to the next test (be careful!)
        cache.Set("key", "value");
    }

    [Fact]
    public void ScopedServices_AreIsolatedPerTest()
    {
        // Each test gets a new instance
        var orderService = Services.GetRequiredService<IOrderService>();
        
        // Changes here won't affect other tests
    }
}
```

**Why distinguish between singleton and scoped?**

Singletons are shared across all tests in a class, which can cause test interdependence. Use singletons
only for truly stateless services. Scoped services are isolated per test, making them safer for most
testing scenarios.

## Best Practices

### 1. Keep Fixtures Lightweight

```csharp
// GOOD: Fixture only configures services
public class CustomersFixture : IntegrationTestFixture
{
    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CustomerDbContext>(options =>
            options.UseInMemoryDatabase("CustomerTests"));
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<CustomerApplicationService>();
    }
}

// AVOID: Don't put test logic in fixtures
public class BadFixture : IntegrationTestFixture
{
    public Customer TestCustomer { get; private set; }
    
    protected override void InitializeFixture(IServiceProvider services)
    {
        // Don't create test data here - it's shared across all tests!
        TestCustomer = new Customer(Guid.NewGuid(), "Test");
    }
}
```

**Why?**

Fixtures are shared across all tests in a class. Putting test-specific data in fixtures creates
coupling between tests and can lead to flaky tests.

### 2. Use Descriptive Test Names

```csharp
// GOOD: Describes what's being tested and expected outcome
[Fact]
public async Task CreateCustomer_WithValidData_ShouldReturn_Success()
{
}

// GOOD: Describes the scenario being tested
[Fact]
public async Task GetCustomer_WhenExists_ShouldReturnCustomer()
{
}
```

### 3. Arrange-Act-Assert Pattern

```csharp
[Fact]
public async Task UpdateCustomer_WhenExists_ShouldPersistChanges()
{
    // Arrange - Set up test data and dependencies
    var id = Guid.NewGuid();
    var repository = Services.GetRequiredService<ICustomerRepository>();
    var customer = new Customer(id, "Original Name");
    await repository.CreateAsync(customer, CancellationToken);

    // Act - Perform the operation being tested
    var command = new UpdateCustomerCommand(id, "Updated Name", "user@example.com");
    await SUT.UpdateCustomerAsync(command, CancellationToken);

    // Assert - Verify the expected outcome
    var updated = await repository.GetByIdAsync(id, CancellationToken);
    Assert.Equal("Updated Name", updated.Name);
    Assert.Equal("user@example.com", updated.Email);
}
```

**Why?**

This pattern makes tests readable and maintainable by clearly separating setup, execution, and
verification.

### 4. Clean Up Test Data

For test data cleanup, create a derived test base class that performs cleanup in `InitializeAsync`:

```csharp
public class CustomerTestBase<TSUT>(CustomersFixture fixture)
    : IntegrationTestBase<TSUT, CustomersFixture>(fixture)
    where TSUT : class
{
    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        
        // Clear data before each test for complete isolation
        var dbContext = Services.GetRequiredService<CustomerDbContext>();
        dbContext.Customers.RemoveRange(dbContext.Customers);
        await dbContext.SaveChangesAsync();
    }
}
```

**Why override `InitializeAsync` in the test base?**

While service scopes provide isolation for service instances, database data persists across tests.
Cleaning up data in `InitializeAsync` ensures each test starts with a known state. By placing this
in a test base class, all tests inheriting from it automatically get the cleanup behavior.

### 5. Use CancellationToken

```csharp
[Fact]
public async Task LongRunningOperation_ShouldRespectCancellation()
{
    // Use the CancellationToken provided by the test framework
    await SUT.ProcessLargeDatasetAsync(TestContext.Current.CancellationToken);
    
    // The framework can cancel long-running tests if needed
}
```

**Why use `CancellationToken`?**

The `TestContext.Current.CancellationToken` allows the test framework to cancel tests that run too
long or when the test run is aborted. This prevents hanging tests and improves developer experience.
It also gives you the opportunity to ensure that your code properly supports cancellation.

## Troubleshooting

### "Test scope has not been started"

**Problem:** Accessing `Services` before `InitializeAsync()` is called.

**Solution:** Don't access `Services` or `SUT` in the test constructor. Use test methods which run after
xUnit's `InitializeAsync()` lifecycle method completes.

### Tests Affecting Each Other

**Problem:** Changes in one test appear in another test.

**Cause:** Using singleton services or not cleaning up shared resources (like databases).

**Solution:**

- Use scoped services instead of singletons for stateful dependencies
- Override `InitializeAsync()` in your test base class to clean up shared resources before each test
- Ensure each test operates on unique data

### Cannot Resolve Service

**Problem:** `InvalidOperationException: Unable to resolve service for type 'X'`

**Solution:** Make sure the service is registered in `ConfigureServices`:

```csharp
protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddDbContext<CustomerDbContext>(options =>
        options.UseInMemoryDatabase("CustomerTests"));
    services.AddScoped<ICustomerRepository, CustomerRepository>();
    services.AddScoped<CustomerApplicationService>();
}
```

### Configuration File Not Found

**Problem:** `FileNotFoundException: appsettings.Testing.json`

**Solution:**

1. Create the file in your test project
2. Set "Copy to Output Directory" to "Copy if newer"
3. Verify the file is in the test output directory (`bin/Debug/net10.0/`)

## Complete Example

Here's the complete example from the `Aiel.Testing.Integration.Examples` project:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Aiel.Testing.Integration.Examples.Customers;

// Fixture
public class CustomersFixture : IntegrationTestFixture
{
    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register data access
        services.AddDbContext<CustomerDbContext>(options =>
            options.UseInMemoryDatabase("CustomerTests")
                   .EnableSensitiveDataLogging(true));

        // Register repositories
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        // Register services
        services.AddScoped<CustomerApplicationService>();
    }

    protected override async ValueTask InitializeFixtureAsync(IServiceProvider services)
    {
        // Ensure database schema exists
        var dbContext = services.GetRequiredService<CustomerDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
    }
}

// Test Base Class (optional, for better organization and cleanup)
public class CustomerTestBase<TSUT>(
    CustomersFixture fixture)
    : IntegrationTestBase<TSUT, CustomersFixture>(fixture)
    where TSUT : class
{
    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        
        // Clear data for test isolation
        var dbContext = Services.GetRequiredService<CustomerDbContext>();
        dbContext.Customers.RemoveRange(dbContext.Customers);
        await dbContext.SaveChangesAsync();
    }
}

// Tests
public class CustomerServiceTests(CustomersFixture fixture)
    : CustomerTestBase<CustomerApplicationService>(fixture)
{
    [Fact]
    public async Task CreateCustomer_WithValidData_ShouldReturn_Success()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var command = new CreateCustomerCommand(id, "Acme Corporation", "contact@acme.com");
        var result = await SUT.CreateCustomerAsync(command, CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(id, result.Value);

        var repository = Services.GetRequiredService<ICustomerRepository>();
        var created = await repository.GetByIdAsync(id, CancellationToken);
        Assert.Equal("Acme Corporation", created.Name);
        Assert.Equal("contact@acme.com", created.Email);
    }

    [Fact]
    public async Task GetCustomer_WhenExists_ShouldReturnCustomer()
    {
        // Arrange - Create test data using the repository directly
        var id = Guid.NewGuid();
        var repository = Services.GetRequiredService<ICustomerRepository>();
        var customer = new Customer(id, "Test Corp");
        await repository.CreateAsync(customer, CancellationToken);

        // Act - Use the SUT to retrieve it
        var query = new GetCustomerByIdQuery(id);
        var result = await SUT.GetCustomerByIdAsync(query, CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(id, result.Value.Id);
        Assert.Equal("Test Corp", result.Value.Name);
    }

    [Fact]
    public async Task UpdateCustomer_WhenExists_ShouldPersistChanges()
    {
        // Arrange
        var id = Guid.NewGuid();
        var repository = Services.GetRequiredService<ICustomerRepository>();
        var customer = new Customer(id, "Original Name");
        await repository.CreateAsync(customer, CancellationToken);

        // Act
        var command = new UpdateCustomerCommand(id, "Updated Name", "user@example.com");
        await SUT.UpdateCustomerAsync(command, CancellationToken);

        // Assert
        var updated = await repository.GetByIdAsync(id, CancellationToken);
        Assert.Equal("Updated Name", updated.Name);
        Assert.Equal("user@example.com", updated.Email);
    }
}
```

This example demonstrates:

- Setting up an in-memory database for testing
- Registering services and repositories
- Using the command/query pattern with DTOs
- Ensuring database initialization
- Cleaning up data between tests
- Using both the SUT and services directly
- Following the Arrange-Act-Assert pattern
- Using primary constructors for cleaner test class syntax

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.
