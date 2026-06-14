# Aiel.DataAccess.Dapper

Provides column mapping support for Dapper. This allows you to map column names to properties on your classes without having to jump through hoops. Well, not too many hoops anyway. This is especially useful when you have a database that uses snake_case for table and column names and you want to use PascalCase in your code.

## Installation

You can install the Aiel.DataAccess.Dapper package via Package Manager Console:

```pwsh
Install-Package Aiel.DataAccess.Dapper
```

Or via .NET CLI:

```pwsh
dotnet add package Aiel.DataAccess.Dapper
```

## Usage

Decorate your class with the `[HasColumnMaps]` attribute and then decorate the properties with
the `[ColumnName]` attribute to map the column name to the property.

```csharp
[HasColumnMaps]
public class Invoice
{
    [ColumnName("id")]
    public Int32 InvoiceNumber { get; set; }

    public required InvoiceLineItem[] LineItems { get; set; } = [];

    [ColumnName("customer_fk")]
    public Int32 CustomerId { get; set; }

    [ColumnName("site_number")]
    public Int32 SiteNumber { get; set; }
}
```

Before your first query, use one or more of the following to apply the column maps:

```csharp
ColumnMapper.MapTypesFromCallingAssembly();

ColumnMapper.MapTypesFromExecutingAssembly();

ColumnMapper.MapTypesFromAssemblyContaining<Invoice>();

ColumnMapper.MapTypesFromAssemblies(typeof(Invoice).Assembly);
```

Or map specific types:

```csharp
ColumnMapper.Map<Invoice>();

typeof(Invoice).MapColumns();
```

> NOTE: If you are mapping specific types, you do not need to apply the `[HasColumnMaps]` attribute to the class.

That's it! You are done! Dapper will now use the column mappings when a query returns
a type that has been mapped.


## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.
