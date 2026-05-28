# Aiel.Gps.HP.Generators

Source generators for the high-performance NMEA parsing library.

## Features

- **NmeaMessageUnionGenerator**: Generates a discriminated union (`NmeaMessage`) for all message types marked with `[NmeaMessage]` attribute.

## Generated Code

The generator produces:

- `NmeaMessageType` enum with all message types
- `NmeaMessage` struct (discriminated union) with:
  - Type checking properties (`IsGLL`, `IsGFDTA`, etc.)
  - TryGet methods (`TryGetGLL()`, `TryGetGFDTA()`, etc.)
  - Factory methods (`FromGLL()`, `FromGFDTA()`, etc.)
  - `TryParse()` for parsing sentences
  - `Match()` for exhaustive pattern matching

## Usage

Mark your message structs with `[NmeaMessage]`:

```csharp
[NmeaMessage("GPGLL")]
public struct GLL
{
    public Double Latitude;
    public Double Longitude;
    // ...
}
```

Mark your parser structs with `[NmeaParser]`:

```csharp
[NmeaParser(typeof(GLL))]
public readonly struct GllParser : INmeaParser<GLL>
{
    // ...
}
```

The generator will automatically create the discriminated union.
