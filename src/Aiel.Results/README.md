# Result Pattern

> [Result Pattern in C#](https://adrianbailador.github.io/blog/44-result-pattern-)
> by [Adrian Bailador](https://adrianbailador.github.io/)

The `Result` class provides a way to represent the outcome of operations,
encapsulating success and failure states along with relevant data or error messages.

## Basic Usage

```csharp
public class UserService
{
    private readonly IUserRepository _repository;

    public Result<User> GetById(Int32 id)
    {
        var user = _repository.Find(id);
        
        if (user is null)
            return Error.NotFound($"User with ID {id} was not found");

        return user; // Implicit conversion to Result<User>.Success
    }

    public Result<User> Create(CreateUserRequest request)
    {
        // Validation happens in ASP.NET pipeline using FluentValidation
        // Domain only handles business rules
        
        if (_repository.ExistsByEmail(request.Email))
            return Error.Conflict("A user with this email already exists");

        var user = new User(request.Name, request.Email);
        _repository.Add(user);

        return user;
    }
}
```

## Error Codes

Error codes use singleton instances with reference equality. Each error type has a unique code that can be used programmatically:

```csharp
// Programmatic use - implicit String conversion
String errorName = error.Code;  // "NotFoundError"

// Debugging - ToString() for logging
Console.WriteLine(error.Code.ToString());  // "NotFoundError"

// Type checking
if (result.Error.IsErrorType<NotFoundError>())
{
    // Handle not found scenario
}
```

> ## :warning: Important Note
>
> Despite the inclusion of `ILLink.Descriptors.xml`, when this assembly is used in a Blazor WebAssembly application,
> required types are still trimmed, breaking the deserialization of `Result` and `Result<T>` and resulting in inexplicable,
> and incredibly hard to debug, runtime errors when deserializing JSON responses. This is a
> [known issue](https://github.com/dotnet/runtime/blob/main/docs/tools/illink/serialization.md) with the ILLinker.
>
> To avoid this, you must manually register the `ErrorJsonConverter` and `ErrorCodeJsonConverter` in your Blazor WebAssembly
> application to ensure they are included in the final build. We have provided a convenience extension method to do this:
>
> ```csharp
> builder.Services.AddResultPattern();
> ```

**Note**: The `.ToString()` method is primarily for debugging and logging. For programmatic use, rely on the implicit `String` operator or `IsErrorType<T>()` method.

## Chaining Operations

```csharp
public Result<OrderConfirmation> ProcessOrder(CreateOrderRequest request)
{
    return ValidateOrder(request)
        .Bind(order => CheckInventory(order))
        .Bind(order => ProcessPayment(order))
        .Bind(order => CreateShipment(order))
        .Map(shipment => new OrderConfirmation(shipment.TrackingNumber));
}

private Result<Order> ValidateOrder(CreateOrderRequest request)
{
    if (request.Items.Count == 0)
        return Error.Validation("Order must contain at least one item");
    
    return new Order(request.CustomerId, request.Items);
}

private Result<Order> CheckInventory(Order order)
{
    foreach (var item in order.Items)
    {
        if (!_inventory.IsAvailable(item.ProductId, item.Quantity))
            return Error.Conflict($"Product {item.ProductId} is out of stock");
    }
    return order;
}
```

## Async Operations

The Result pattern includes comprehensive async support for modern C# codebases:

```csharp
// Example 1: Async data access
public async Task<Result<User>> GetUserAsync(Int32 userId)
{
    return await _repository.FindAsync(userId)
        .MapAsync(user => user ?? Error.NotFound("User not found"));
}

// Example 2: Async pipeline with external services
public async Task<Result<OrderConfirmation>> ProcessOrderAsync(CreateOrderRequest request)
{
    return await ValidateOrderAsync(request)
        .BindAsync(async order => await CheckInventoryAsync(order))
        .BindAsync(async order => await ProcessPaymentAsync(order))
        .TapAsync(async order => await SendConfirmationEmailAsync(order))
        .MapAsync(async order => await CreateConfirmationAsync(order));
}

private async Task<Result<Order>> CheckInventoryAsync(Order order)
{
    foreach (var item in order.Items)
    {
        var isAvailable = await _inventoryService.CheckAvailabilityAsync(item.ProductId, item.Quantity);
        if (!isAvailable)
            return Error.Conflict($"Product {item.ProductId} is out of stock");
    }
    return order;
}

// Example 3: Async Match for handling results
var message = await userService.GetUserAsync(userId)
    .MatchAsync(
        onSuccess: async user => 
        {
            await _analytics.TrackUserAccessAsync(user.Id);
            return $"Welcome, {user.Name}!";
        },
        onFailure: async error => 
        {
            await _logger.LogErrorAsync(error.Description);
            return $"Error: {error.Description}";
        });
```

**When to use async methods:**

- `MapAsync` - When transforming the result value requires async operations (e.g., calling external APIs, database queries)
- `BindAsync` - When the next step in the pipeline is async and can fail (returns `Task<Result<T>>`)
- `MatchAsync` - When handling success/failure cases requires async operations (e.g., logging, analytics)
- `TapAsync` - When side effects are async but shouldn't affect the result (e.g., sending notifications, caching)

## Using Match

```csharp
var message = userService.GetById(userId).Match(
    onSuccess: user => $"Welcome, {user.Name}!",
    onFailure: error => $"Error: {error.Description}"
);
```

## Getting Values Safely

```csharp
// With explicit default
var user = result.GetValueOrDefault(User.Guest);

// With type default (null for reference types)
var userId = result.GetValueOrDefault();

// Using Match for custom logic
var user = result.Match(
    onSuccess: u => u,
    onFailure: error => 
    {
        _logger.LogError(error.Description);
        return User.Guest;
    });
```

## Domain Errors

```csharp
public static class DomainErrors
{
    public static class User
    {
        public static Error NotFound(Int32 id) =>
            Error.NotFound($"User with ID {id} was not found");

        public static Error EmailAlreadyExists(String email) =>
            Error.Conflict($"Email {email} is already registered");

        public static Error InvalidEmail =>
            Error.Validation("The email format is invalid");

        public static Error PasswordTooWeak =>
            Error.Validation(
                "Password must be at least 8 characters with uppercase, lowercase, and digits");
    }

    public static class Order
    {
        public static Error NotFound(Guid id) =>
            Error.NotFound($"Order {id} was not found");

        public static Error EmptyCart =>
            Error.Validation("Cannot create order with empty cart");

        public static Error InsufficientStock(String productId) =>
            Error.Conflict($"Insufficient stock for product {productId}");
    }
}
```

## Creating Custom Error Types

The `Error` class is fully extensible, allowing you to create domain-specific error types with additional properties while maintaining type safety and automatic JSON serialization.

### Basic Custom Error

Create a custom error by inheriting from `Error` and defining an internal singleton `ErrorCode`:

```csharp
public sealed class OrderNotFoundError : Error
{
    public String CustomerId { get; }

    public OrderNotFoundError(String description, String customerId)
        : base(CustomerNotFoundErrorCode.Instance, description)
    {
        CustomerId = customerId;
    }

    internal sealed class CustomerNotFoundErrorCode : ErrorCode
    {
        public static readonly CustomerNotFoundErrorCode Instance = new();
        protected override String Name => nameof(OrderNotFoundError);
    }
}
```

**Key requirements:**

- Inherit from `Error` as a `sealed class`
- Define an internal `ErrorCode` class with a singleton `Instance`
- Override `Name` property to return the error type name
- Call base constructor with `ErrorCode` and description
- Add any additional domain-specific properties with getters

### Custom Error with Additional Properties

Custom errors can include domain-specific data that will automatically serialize:

```csharp
public sealed class TransactionError : Error
{
    public String DeclineReason { get; }
    public String TransactionId { get; }

    public TransactionError(String description, String declineReason, String transactionId) 
        : base(PaymentDeclinedErrorCode.Instance, description)
    {
        DeclineReason = declineReason;
        TransactionId = transactionId;
    }

    internal sealed class PaymentDeclinedErrorCode : ErrorCode
    {
        public static readonly PaymentDeclinedErrorCode Instance = new();
        protected override String Name => nameof(TransactionError);
    }
}
```

### Using Custom Errors

Custom errors work seamlessly with the Result pattern:

```csharp
public class CustomerService
{
    public Result<Customer> GetCustomer(String customerId)
    {
        var customer = _repository.FindById(customerId);
        
        if (customer is null)
            return new OrderNotFoundError(
                $"Customer with ID '{customerId}' was not found",
                customerId);

        return customer;
    }

    public Result<PaymentConfirmation> ProcessPayment(PaymentRequest request)
    {
        var result = _paymentGateway.Charge(request);
        
        if (!result.Success)
            return new TransactionError(
                "Payment was declined by the payment processor",
                result.DeclineReason,
                result.TransactionId);

        return new PaymentConfirmation(result.TransactionId);
    }
}
```

### Type-Safe Error Handling

Use pattern matching or `IsErrorType<T>()` to handle custom errors:

```csharp
// Using pattern matching
var result = customerService.GetCustomer(customerId);
var message = result.Match(
    onSuccess: customer => $"Welcome, {customer.Name}!",
    onFailure: error => error switch
    {
        OrderNotFoundError notFound => 
            $"No customer found with ID: {notFound.CustomerId}",
        TransactionError declined => 
            $"Payment declined: {declined.DeclineReason} (Ref: {declined.TransactionId})",
        _ => $"Error: {error.Description}"
    });

// Using IsErrorType<T>()
if (result.IsFailure && result.Error.IsErrorType<OrderNotFoundError>())
{
    var notFoundError = (OrderNotFoundError)result.Error;
    _logger.LogWarning("Customer lookup failed for ID: {CustomerId}", notFoundError.CustomerId);
}
```

### JSON Serialization

Custom errors automatically serialize and deserialize without any configuration:

```csharp
// Serialization preserves custom properties
Result<Order> result = new TransactionError(
    "Card declined",
    "Insufficient funds",
    "TXN-12345");

var json = JsonSerializer.Serialize(result);
// {
//   "IsSuccess": false,
//   "Error": {
//     "$type": "MyApp.TransactionError, MyApp",
//     "Code": { "$type": "...", "Name": "TransactionError" },
//     "Description": "Card declined",
//     "DeclineReason": "Insufficient funds",
//     "TransactionId": "TXN-12345"
//   }
// }

// Deserialization restores exact type
var deserialized = JsonSerializer.Deserialize<Result<Order>>(json);
deserialized.Error.GetType(); // TransactionError
((TransactionError)deserialized.Error).TransactionId; // "TXN-12345"
```

**How it works:**

- The `ErrorJsonConverter` uses reflection to discover all properties on your custom error type
- During serialization, it writes the fully-qualified type name as `$type` discriminator
- During deserialization, it loads the type and invokes the constructor with matching parameter names
- All public properties (including custom ones) are automatically included

**Cross-assembly support:**
Custom errors defined in any assembly will serialize correctly, even if the consuming application has no knowledge of them at compile time. The type discriminator ensures the correct type is reconstructed during deserialization.

## Exception Handling

The `Error.Exception()` method is available to convert exceptions to errors, but its use should be **rare and discouraged**.

**Why it exists:**
In Blazor applications, unhandled exceptions can crash the entire app, forcing a page reload. Converting exceptions to errors provides a recovery path.

**Important limitations:**

```csharp
try
{
    await externalService.CallAsync();
}
catch (Exception ex)
{
    // ⚠️ This deliberately loses stack trace and inner exceptions
    return Error.Exception(ex);  
}
```

**The conversion is deliberately minimal:**

- ✅ Captures exception type name and message only
- ❌ Loses stack trace (security/privacy concern)
- ❌ Loses inner exceptions
- ❌ Loses custom exception properties

**Why these limitations:**

1. **Performance** - Error values must be lightweight for high-throughput scenarios
2. **Security** - Error descriptions may be visible to end users; stack traces can leak internal details
3. **Serialization** - Full exceptions are not serializable across API boundaries

**Best practice:**
Always log the full exception separately before converting:

```csharp
try
{
    await riskyOperation();
}
catch (Exception ex)
{
    _logger.LogError(ex, "Operation failed for user {UserId}", userId);
    return Error.Exception(ex);  // Only for user-facing message
}
```

## Input Validation vs Business Rules

The Result pattern is designed for **business rule violations**, not input validation.

**Use FluentValidation for input validation:**

```csharp
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).MinimumLength(8);
    }
}
```

ASP.NET pipeline validates input and returns 400 BadRequest with detailed validation errors **before** the domain logic executes.

**Use Result<T> for business logic:**

```csharp
public async Task<Result<User>> Handle(CreateUserCommand command)
{
    // Input is already validated by pipeline
    
    // Domain only checks business rules
    if (await _userRepository.EmailExistsAsync(command.Email))
        return Error.Conflict("Email already registered");
        
    return await _userRepository.CreateAsync(command);
}
```

This separation ensures:

- Multiple validation errors caught at API boundary
- Type-safe domain logic with single error per operation
- Clean architecture with clear responsibility boundaries

## Web API Integration

This is packaged separately in `Aiel.WebApi` so you need to install that package as well.

```pwsh
Install-Package Aiel.WebApi
```

Or via .NET CLI:

```pwsh
dotnet add package Aiel.WebApi
```

Once installed, you can use the extension methods to convert `Result` instances to appropriate HTTP responses.

```csharp
var app = builder.Build();

app.MapGet("/api/users/{id}", (Int32 id, UserService userService) =>
{
    return userService.GetById(id).ToApiResult();
});

app.MapPost("/api/users", (CreateUserRequest request, UserService userService) =>
{
    return userService.Create(request)
        .ToCreatedResult($"/api/users/{request.Email}");
});

app.MapPost("/api/orders", (CreateOrderRequest request, OrderService orderService) =>
{
    return orderService.ProcessOrder(request).Match(
        onSuccess: confirmation => Results.Ok(confirmation),
        onFailure: error => error switch
        {
            ValidationError => Results.BadRequest(new { error.Code, error.Description }),
            ConflictError => Results.Conflict(new { error.Code, error.Description }),
            _ => Results.Problem(error.Description)
        }
    );
});
```

## Async Operations

The Result pattern includes comprehensive async support for modern C# codebases:

```csharp
// Example 1: Async data access
public async Task<Result<User>> GetUserAsync(Int32 userId)
{
    return await _repository.FindAsync(userId)
        .MapAsync(user => user ?? Error.NotFound("User not found"));
}

// Example 2: Async pipeline with external services
public async Task<Result<OrderConfirmation>> ProcessOrderAsync(CreateOrderRequest request)
{
    return await ValidateOrderAsync(request)
        .BindAsync(async order => await CheckInventoryAsync(order))
        .BindAsync(async order => await ProcessPaymentAsync(order))
        .TapAsync(async order => await SendConfirmationEmailAsync(order))
        .MapAsync(async order => await CreateConfirmationAsync(order));
}

private async Task<Result<Order>> CheckInventoryAsync(Order order)
{
    foreach (var item in order.Items)
    {
        var isAvailable = await _inventoryService.CheckAvailabilityAsync(item.ProductId, item.Quantity);
        if (!isAvailable)
            return Error.Conflict($"Product {item.ProductId} is out of stock");
    }
    return order;
}

// Example 3: Async Match for handling results
var message = await userService.GetUserAsync(userId)
    .MatchAsync(
        onSuccess: async user => 
        {
            await _analytics.TrackUserAccessAsync(user.Id);
            return $"Welcome, {user.Name}!";
        },
        onFailure: async error => 
        {
            await _logger.LogErrorAsync(error.Description);
            return $"Error: {error.Description}";
        });
```

**When to use async methods:**

- `MapAsync` - When transforming the result value requires async operations (e.g., calling external APIs, database queries)
- `BindAsync` - When the next step in the pipeline is async and can fail (returns `Task<Result<T>>`)
- `MatchAsync` - When handling success/failure cases requires async operations (e.g., logging, analytics)
- `TapAsync` - When side effects are async but shouldn't affect the result (e.g., sending notifications, caching)

## HTTP Client Integration

The library provides specialized extension methods for working with `Result` and `Result<T>` types over HTTP:

```csharp
using Aiel.Results;

public class WeatherApiClient
{
    private readonly HttpClient _httpClient;

    public WeatherApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // GET request returning Result<T>
    public async Task<Result<WeatherForecast>> GetWeatherAsync(string location, CancellationToken cancellationToken = default)
    {
        return await _httpClient.GetResultAsync<WeatherForecast>($"weather/{location}", cancellationToken);
    }

    // POST request returning Result<T>
    public async Task<Result<OrderConfirmation>> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        return await _httpClient.PostAndReturnResultAsync<CreateOrderRequest, OrderConfirmation>(
            "orders",
            request,
            cancellationToken);
    }

    // PUT request returning Result<T>
    public async Task<Result<User>> UpdateUserAsync(Int32 id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        return await _httpClient.PutAndReturnResultAsync<UpdateUserRequest, User>(
            $"users/{id}",
            request,
            cancellationToken);
    }

    // PATCH request returning Result<T>
    public async Task<Result<User>> PartialUpdateAsync(Int32 id, PatchUserRequest request, CancellationToken cancellationToken = default)
    {
        return await _httpClient.PatchAndReturnResultAsync<PatchUserRequest, User>(
            $"users/{id}",
            request,
            cancellationToken);
    }

    // DELETE request returning Result<T>
    public async Task<Result<Unit>> DeleteUserAsync(Int32 id, CancellationToken cancellationToken = default)
    {
        return await _httpClient.DeleteAndReturnResultAsync<Unit>($"users/{id}", cancellationToken);
    }
}
```

**Important**: These extension methods automatically use the configured `Results.JSO` instance which includes all necessary converters for polymorphic error deserialization. This ensures that custom error types are properly deserialized when received from the server.

## Dependency Injection

The Result pattern requires proper configuration during application startup to enable JSON serialization of `Result`, `Result<T>`, and custom error types:

```csharp
// In Program.cs or Startup.cs
var builder = WebApplicationBuilder.CreateBuilder(args);

// Register Result pattern services
builder.Services.AddResultPattern();

var app = builder.Build();
app.Run();
```

This call:
- Configures the global `Results.JSO` instance with converters for polymorphic error deserialization
- Registers `JsonSerializerOptions` in the DI container for injection
- (ASP.NET Core only) Configures framework JSON options for API responses

### Static Access

For code that cannot use dependency injection, access the configured options via the static property:

```csharp
var json = JsonSerializer.Serialize(result, Results.JSO);
var deserialized = JsonSerializer.Deserialize<Result<T>>(json, Results.JSO);
```

### Custom Configuration

Pass a configuration action to customize serialization options before Results converters are registered:

```csharp
builder.Services.AddResultPattern(options =>
{
    options.PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase;
    options.WriteIndented = true;
});
