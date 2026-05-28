# Aiel.StrongIds.EntityFrameworkCore

Entity Framework Core integration helpers for `Aiel.StrongIds`.

## Usage

```csharp
using Aiel.StrongIds.EntityFrameworkCore;

modelBuilder.Entity<Order>(entity =>
{
	entity.HasKey(static order => order.Id);
	entity.Property(static order => order.Id).HasStrongIdConversion<OrderId, Guid>();
	entity.Property(static order => order.CustomerId).HasStrongIdConversion<CustomerId, Guid>();
	entity.Property(static order => order.OptionalCustomerId).HasStrongIdConversion<CustomerId, Guid>();
});
```

`HasStrongIdConversion<TStrongId, TValue>()` configures an EF Core value converter for generated Strong IDs and sets an immutable-friendly value comparer for change tracking. The nullable overload supports optional struct-backed Strong IDs and maps them through nullable provider values such as `Guid?`.
