# Result Pattern

> Inspired by the [Result Pattern in C#](https://adrianbailador.github.io/blog/44-result-pattern-)
> article by [Adrian Bailador](https://adrianbailador.github.io/)

The `Result` class provides a way to represent the outcome of operations, encapsulating success and failure states along with relevant data or error messages.

> ## :warning: Important Note regarding Blazor WebAssembly applications :warning:
>
> Even with the inclusion of `ILLink.Descriptors.xml` in a consuming proejct, when this assembly is used in a Blazor WebAssembly
> application, required types are still trimmed, breaking the deserialization of `Result` and `Result<T>` and resulting in
> inexplicable, and incredibly hard to debug, runtime errors when deserializing JSON responses. This is a
> [known issue](https://github.com/dotnet/runtime/blob/main/docs/tools/illink/serialization.md) with the ILLinker.
>
> To avoid this, you must manually register the `ErrorJsonConverter` and `ErrorCodeJsonConverter` in your Blazor WebAssembly
> application to ensure they are included in the final build. We have provided a convenience extension method to do this:
>
> ```csharp
> builder.Services.AddResultPattern();
> ```

## Results

`Result<T>` is constrained to `where T : notnull`, and `Result<T>.Success(value)` rejects `null` at runtime. A successful result will never carry a `null` value.

### Creating Results

```csharp
// Returns a Result indicating a successful operation: `Result.IsSuccess == true`
Result.Success();

// Returns a Result<String> indicating a successful operation: `Result.Value == "User was added."`
Result.Success("User was added.");

// Returns a Result indicating a failed operation: `Result.Error == ConcurrencyViolation("...")`
Result.Failure(new ConcurrencyViolation("..."));

// Throws ArgumentNullException — a successful Result<T> cannot carry null.
Result.Success<Customer>(null!);

// If the method signature is `public Result<Customer> FindCustomer(customerName)`...

return customer; // Implicit conversion to Result<Customer> with `Result.IsSuccess == true` and `Result.Value == customer`.

return new NotFoundError("Customer not found"); // Implicit conversion to Result<Customer> with `Result.IsSuccess == false`. Reading `Result.Value` throws.
```

### Accessing the Value

A value only exists when `Result.IsSuccess == true`. There are two ways to get the value and the right one depends on whether failure is an expected outcome at that call site.

```csharp
var result = userService.GetById(userId);

// Checked access — use this when failure is expected and you intend to handle it.
if (result.TryGetValue(out var user))
{
    Console.WriteLine($"Welcome, {user.Name}!");
}
else
{
    _logger.LogError("{Error}", result.Error.Description);
}

// Direct access — .Value is safe to use once you have established IsSuccess == true.
if (result.IsSuccess)
{
    Console.WriteLine($"Welcome, {result.Value.Name}.");
}
```

The `Result<T>.Value` property is symmetrical: it never returns `null`. When `IsSuccess == false`, reading `Value` throws a `ResultException` carrying the `Error` rather than handing back a `null` you were not expecting. This is deliberate: the alternative is a silent `null` that surfaces as a `NullReferenceException` at runtime somewhere further away from the actual mistake.

This embraces the **Fail Fast** principle, while following the principle of **Least Surprise**: if you are reading `Value`, you are expecting a valid value, and if there is no value, you want to know about it immediately.

> NOTE: `Value` is annotated `[JsonIgnore]`. Because the getter can throw, it must never sit in the path of a
> serializer, logger, or object mapper that walks public properties. `Result<T>` is serialized by its
> dedicated `ErrorJsonConverter` converter. To make use of this converter, call `ConfigureForResults()` on 
> your `JsonSerializerOptions`, or if you are using ASP.NET Core Blazor WebAssembly, call 
> `services.AddResultPattern()` at startup.

### Full Example

```csharp
// We will explain these errors in the next section.
public sealed partial class NotFoundError : Error;

public sealed partial class ConflictError : Error;

public class UserService
{
    private readonly IUserRepository _repository;

    public Result<User> GetById(Int32 id)
    {
        var user = _repository.Find(id);
        
        if (user is null)
            return new NotFoundError($"User with ID {id} was not found");

        return user; // Implicit conversion to Result<User> with `Result.IsSuccess == true` and `Result.Value == user`.
    }

    public Result<User> Create(CreateUserRequest request)
    {
        // Validation happens in ASP.NET pipeline, possibly using FluentValidation
        // Domain only handles business rules
        
        if (_repository.ExistsByEmail(request.Email))
            return new ConflictError("A user with this email already exists");

        var user = new User(request.Name, request.Email);

        _repository.Add(user); // Unit of Work calls SaveChangesAsync()

        return user; // Implicit conversion to Result<User> with `Result.IsSuccess == true` and `Result.Value == user`.
    }
}
```

## Modelling Absence with `Maybe<T>`

`Result<T>` has exactly two states: it worked, or it did not. Some operations have a third outcome — the
operation worked and the answer is legitimately nothing. A lookup that finds no match did not *fail*.

Encoding that as a failure forces every caller to inspect error *types* to find out whether anything actually
went wrong, which is the "exceptions for control flow" problem rebuilt inside `Result`. Encoding it as a
`null` value reintroduces the very thing the pattern exists to remove. `Maybe<T>` is the third option:

```csharp
public async Task<Result<Maybe<Customer>>> FindCustomerByEmailAsync(Email email, CancellationToken ct)
{
    try
    {
        // FirstCustomerByEmailAsync is a IQueryable<Customer> extension method.
        var customer = await _repository.FirstCustomerByEmailAsync(email, ct);

        // FromNullable is the adapter for boundaries that still produce null.
        return Result.Success(Maybe<Customer>.FromNullable(customer));
    }
    catch (DbException ex)
    {
        // This one really is a failure.
        return InfrastructureError.FromException(ex);
    }
}
```

The three outcomes are now distinct, and the compiler makes the caller deal with all of them:

```csharp
var result = await FindCustomerAsync(id, ct);

if (result.IsFailure)
{
    return Problem(result.Error);          // a real error occured during the search
}

if (!result.Value.TryGetValue(out var customer))
{
    return NotFound();                     // the lookup worked; there is no such customer
}

return Ok(customer);                       // the lookup worked; here it is
```

### `Maybe<T>` API

| Member | Behaviour |
|---|---|
| `Maybe<T>.Some(value)` | Holds `value`. Throws `ArgumentNullException` if it is `null`. |
| `Maybe<T>.None` | Holds nothing. This is also `default(Maybe<T>)`. |
| `Maybe<T>.FromNullable(value)` | `None` when `value` is `null`, otherwise `Some(value)`. |
| `HasValue` / `IsNone` | Whether a value is present. |
| `TryGetValue(out value)` | Checked access. Prefer this. |
| `Value` | Direct access. Throws `InvalidOperationException` when `None`. |
| `GetValueOrDefault(fallback)` | The value, or `fallback` when `None`. |

Two properties are worth calling out:

- **`default(Maybe<T>)` is `None`.** An uninitialized or default-constructed value fails closed rather than
  presenting `default(T)` as though it were a real answer. This matters most for enums and other value types,
  where `default` is otherwise indistinguishable from a legitimate zero.
- **`Some(default(T))` is still `Some`.** `Maybe<Int32>.Some(0)` has a value. A count of zero, an empty string,
  and `Guid.Empty` are all answers, and `Maybe<T>` keeps them distinct from absence.

### On the wire

`Some` serializes as the bare underlying value and `None` serializes as `null`. The wrapper never appears in
the JSON, so API contracts stay clean for consumers that have no notion of `Maybe<T>`:

```jsonc
{ "isSuccess": true, "value": { "id": 42, "name": "Ada" } }  // Success(Some(customer))
{ "isSuccess": true, "value": null }                         // Success(None)
{ "isSuccess": false, "error": { /* ... */ } }               // Failure
```

The absence of a wrapper on the wire does not weaken the guarantee. It is the type system, not the JSON, that
forces callers to handle the empty case.

## Failed Results and Errors

Results always carry an `Error` object. When `IsSuccess == false`, the `Error` property contains the error that describes the failure. When `IsSuccess == true`, the `Error` property contains a sentinel `NoError` instance that indicates there was no error. This design ensures that the `Error` property is never null, simplifies error handling, reduces the risk of null reference exceptions, and avoids unnecessary allocations.

In the previous example, `NotFoundError` and `ConflictError` are domain-specific errors that inherit from the base `Error` class. Each error type has a unique `ErrorCode` that can be used programmatically to identify the error type without relying on string comparisons or reflection. A source generator in `Aiel.Results.Generator` automatically generates the necessary boilerplate code for these domain specific errors, including error codes, constructors, properties, and JSON serialization support.

Error codes use singleton instances with reference equality to avoid unnecessary allocations and ensure efficient comparisons.

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

**Note**: `ErrorCode.ToString()` is primarily for debugging and logging. For programmatic use, rely on the implicit `String` operator or `IsErrorType<T>()` method.

## Convenience Methods for Domain Errors

`Aiel.Results` works hard to make returning results easy but it can only go so far. You can create your own convenience methods for your specific domain.

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

## Customized Error Types

There are times when a basic error does not carry enough information in the description and error code. Fortunately, the `Error` class is fully extensible, allowing you to create error types with additional properties while still allowing the source generator to generate the necessary constructors and boilerplate code. This allows carrying more error details while maintaining type safety and automatic JSON serialization.

### Rules

A Failure Result must have an error. A Success Result must not have an error. The `Error` base class handles this for you. What you end up with is a `Result<T>` that is either a success with a value, or a failure with an error. The following rules apply:

**When `Result.IsSuccess == true`**

- `Result.Value` can be safely accessed
- `Result.Error` returns the sentinel `NoError` type instead of `null`

**When `Result.IsSuccess == false`**

- `Result.Value` throws an exception if accessed
- `Result.Error` contains the error that caused the failure

### Basic Custom Error

**Key requirements:**

- Must inherit from `Error` as a `public sealed class` or `internal sealed class`.
- May have additional domain-specific properties with getters, and optionally setters.
- Optionally define a constructor that takes a description string and any additional properties, and call the base constructor with the appropriate `ErrorCode` instance and description.

> Before the source generator runs you may see `CS1729: Error' does not contain a constructor that takes 0 arguments.`. This occurs because the source generator has not yet generated the constructor(s) that will call the correct base constructor. This error is resolved after the source generator runs.

Start by creating an error that inherits from `Error`. Then add any additional properties as needed.

```csharp
public sealed partial class OrderNotFoundError : Error
{
    public String CustomerId { get; }

    // Optional custom constructor that takes a description and any additional properties.
    [JsonConstructor]
    public OrderNotFoundError(String description, String customerId)
        : base(CustomerNotFoundErrorCode.Instance, description)
    {
        CustomerId = customerId;
    }
}
```

That is pretty much it. The signature of the class declaration will attract the source generator and generate the remaining boilerplate. Use the error like any other.

**Important:** If you implement a custom constructor to initialize your domain properties you must also decorate it with the `[JsonConstructor]` attribute so that JSON deserialization works correctly.

### Advanced Custom Error

Custom errors do not have to use the source generator. However, if they do not use the source generator, they must implement all the necessary boilerplate normally emitted by the source generator. 

The following `CustomerNotFoundError` example demonstrates a fully custom error type that can still be used with the Result Pattern. Note that it does not include the `partial` keyword effectively hiding it from the source generator.

- It has a custom property `CustomerId`.
- It has a custom constructor that initializes the `CustomerId` property and calls the base constructor with the `CustomerNotFoundErrorCode` instance.
- The custom constructor is decorated with the `[JsonConstructor]` attribute to ensure that JSON deserialization works correctly.
- It defines a nested `CustomerNotFoundErrorCode`, overrides the `Name` property, and has a static `Instance` property.
- It overrides the `GenerateDescription()` method to provide a meaningful error message. This allows an error to be created without requiring a description string to be formatted and passed to the constructor, simplifying error creation and eliminating an allocation if the error description is never accessed.
- It registers itself with the `Aiel.Results.ErrorRegistry` in a static constructor. This is required for it to work with the `ErrorJsonConverter`.

```csharp
using Aiel.Results;                    // Required for Error, ErrorCode, and ErrorRegistry types.
using System.Text.Json.Serialization;  // For the [JsonConstrucor] attribute.

namespace Aiel.Results.TestErrors;

public sealed class CustomerNotFoundError : Error
{
    static CustomerNotFoundError()
        => ErrorRegistry.Register<CustomerNotFoundError>();

    public String CustomerId { get; }

    [JsonConstructor]
    public CustomerNotFoundError(String customerId)
        : base(CustomerNotFoundErrorCode.Instance)
    {
        CustomerId = customerId;
    }

    override protected String GenerateDescription()
        => $"Customer was not found: {CustomerId}";

    internal sealed class CustomerNotFoundErrorCode : ErrorCode
    {
        public static readonly CustomerNotFoundErrorCode Instance = new();

        protected override String Name => "Customer Not Found"
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
            return new CustomerNotFoundError(customerId);

        return customer;
    }
}
```

### Type-Safe Error Handling

Use pattern matching or `IsErrorType<T>()` to handle custom errors:

```csharp
// Using pattern matching
var result = customerService.GetCustomer(customerId);
var message = result.TryGetValue(out var customer)
    ? $"Welcome, {customer.Name}!"
    : result.Error switch
    {
        OrderNotFoundError notFound =>
            $"No customer found with ID: {notFound.CustomerId}",
        TransactionError declined =>
            $"Payment declined: {declined.DeclineReason} (Ref: {declined.TransactionId})",
        var error => $"Error: {error.Description}"
    };

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

// Deserialization restores exact type
var deserialized = JsonSerializer.Deserialize<Result<Order>>(json);
deserialized.Error.GetType(); // TransactionError
((TransactionError)deserialized.Error).TransactionId; // "TXN-12345"
```

This is roughly what the serialized result looks like:

```json
{
  "IsSuccess": false,
  "Error": {
    "$type": "MyApp.TransactionError, MyApp",
    "Code": { "$type": "...", "Name": "TransactionError" },
    "Description": "Card declined",
    "DeclineReason": "Insufficient funds",
    "TransactionId": "TXN-12345"
  }
}
```

**How it works:**

- The `ErrorJsonConverter` uses reflection to discover all properties on your custom error type
- During serialization, it writes the fully-qualified type name as `$type` discriminator
- During deserialization, it loads the type and invokes the constructor with matching parameter names
- All public properties (including custom ones) are automatically included

**Cross-assembly support:**
Custom errors defined in any assembly will serialize correctly, even if the consuming application has no knowledge of them at compile time. The type discriminator ensures the correct type is reconstructed during deserialization.

## Exception Handling

In Blazor applications, unhandled exceptions can crash the entire app, forcing a page reload. Converting exceptions to errors provides a recovery path. However, converting an exception to an error is deliberately minimal:

- Captures exception type name and message only
- Loses stack trace (security/privacy concern)
- Loses custom exception properties

**Why these limitations:**

1. **Performance** - Error values must be lightweight for high-throughput scenarios
2. **Security** - Error descriptions may be visible to end users; stack traces can leak internal details
3. **Serialization** - Full exceptions are not serializable across API boundaries

**Best practice:**

Always log the full exception separately before converting it to an error:

```csharp
try
{
    await riskyOperation();
}
catch (Exception ex)
{
    _logger.LogError(ex, "Operation failed for user {UserId}", userId);
    return PlaceholderError.FromException(ex);
}
```

### PlaceholderError

The `PlaceholderError` exists for easing development when a more specific error type does not currently exist.

```csharp
public sealed partial class PlaceholderError : Error
{
    public static PlaceholderError FromException(Exception ex, String? message = null)
    {
        var sb = new StringBuilder();
        if (!String.IsNullOrWhiteSpace(message))
        {
            sb.AppendLine(message);
            sb.AppendLine();
        }

        ex.Visit((iex) => sb.AppendLine($"{iex.GetType().Name}: {iex.Message}"));

        return new PlaceholderError(sb.ToString());
    }
}
```

Use it like this:

```csharp
try
{
    await externalService.CallAsync();
}
catch (Exception ex)
{
    // This deliberately loses stack trace and inner exceptions
    return PlaceholderError.FromException(ex);  
}
```

> While we provide the `PlaceholderError` as part of **Aiel.Results**, using it in production is **strongly discouraged**. We have worked hard to make it simple to create a rich set of domain-specific errors and encourage you to do so. The `PlaceholderError` is intended for development and prototyping only. An analyzer is forthcoming that will complain when it sees it in code.

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

## AspNetCore WebAPI Integration

> TODO: Add example of using `Result<T>` in ASP.NET Core WebAPI controller actions, including returning appropriate HTTP status codes based on success/failure.

## Async Operations

`Result<T>` composes with `async`/`await` using ordinary control flow. There are no combinators to learn —
each step checks the previous one and returns early:

```csharp
// Example 1: Async data access, where "not found" is an expected outcome
public async Task<Result<Maybe<User>>> FindUserAsync(Int32 userId, CancellationToken ct)
{
    var user = await _repository.FindAsync(userId, ct);
    return Result.Success(Maybe<User>.FromNullable(user));
}

// Example 2: Async pipeline with external services
public async Task<Result<OrderConfirmation>> ProcessOrderAsync(CreateOrderRequest request, CancellationToken ct)
{
    var validated = await ValidateOrderAsync(request, ct);
    if (!validated.TryGetValue(out var order))
    {
        return validated.Error;
    }

    var stocked = await CheckInventoryAsync(order, ct);
    if (stocked.IsFailure)
    {
        return stocked.Error;
    }

    var paid = await ProcessPaymentAsync(order, ct);
    if (paid.IsFailure)
    {
        return paid.Error;
    }

    await SendConfirmationEmailAsync(order, ct);

    return await CreateConfirmationAsync(order, ct);
}

private async Task<Result> CheckInventoryAsync(Order order, CancellationToken ct)
{
    foreach (var item in order.Items)
    {
        var isAvailable = await _inventoryService.CheckAvailabilityAsync(item.ProductId, item.Quantity, ct);
        if (!isAvailable)
        {
            return new ConflictError($"Product {item.ProductId} is out of stock");
        }
    }

    return Result.Success();
}

// Example 3: Handling both outcomes
var result = await userService.GetUserAsync(userId, ct);

if (result.TryGetValue(out var user))
{
    await _analytics.TrackUserAccessAsync(user.Id, ct);
    message = $"Welcome, {user.Name}!";
}
else
{
    _logger.LogError("{Error}", result.Error.Description);
    message = $"Error: {result.Error.Description}";
}
```

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
```

## Combinators

> Earlier versions of this package shipped `Map`, `Bind`, `Match`, and `Tap` combinators along with their
> async variants. They have been removed. Explicit early-return reads better against the rest of the
> framework, keeps stack traces intact, and avoids the awkward double-unwrapping that combinators require
> once `Result<Maybe<T>>` enters the picture.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.
