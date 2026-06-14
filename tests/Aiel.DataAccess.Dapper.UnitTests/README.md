# Dapper Column Mapping Tests

This test project verifies that the `ColumnMapping` functionality works correctly with Dapper and SQLite in-memory databases.

## Test Coverage

### ColumnMappingTests

Tests the core column mapping functionality:

- **ApplyMap_Generic_ShouldMapColumnsToProperties**: Verifies generic `ApplyMap<T>()` method
- **ApplyMap_ByType_ShouldMapColumnsToProperties**: Verifies type-based `ApplyMap(Type)` extension method
- **ApplyMap_WithMultipleProperties_ShouldMapAllColumns**: Tests mapping multiple properties
- **Mapping_ShouldWorkWithInsertAndRetrieve**: Validates round-trip insert/query operations
- **Mapping_ShouldHandleNullableValues**: Ensures nullable properties are handled correctly

### ColumnMappingDependencyInjectionTests

Tests dependency injection integration:

- **MapColumns_WithServiceProvider_ShouldApplyAllRegisteredMappings**: Verifies DI container integration
- **MapColumns_WithMultipleMappings_ShouldApplyAllMappings**: Tests multiple type mappings via DI

## Running the Tests

```bash
dotnet test tests/Aiel.Dapper.UnitTests/Aiel.Dapper.UnitTests.csproj
```

## Test Database

All tests use SQLite in-memory databases (`Data Source=:memory:`) which are created fresh for each test via the `IAsyncLifetime` interface.

## Example Usage

The tests demonstrate how to use the column mapping:

```csharp
// Apply mapping
ColumnMapping.ApplyMap<Product>();

// Query with Dapper - columns automatically mapped
var product = await connection.QueryFirstOrDefaultAsync<Product>(
    "SELECT product_name, unit_price FROM products WHERE id = @id",
    new { id = 1 }
);
```

Where `Product` has properties decorated with `[ColumnMap]`:

```csharp
public class Product
{
    [ColumnMap("product_name")]
    public String Name { get; set; }
    
    [ColumnMap("unit_price")]
    public Decimal Price { get; set; }
}
```

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.
