# Aiel.Gps.HP.Generators

Source generators for the high-performance NMEA parsing library.  For more information, see the [Aiel.Gps.HP documentation](https://github.com/TwoRiversIT/Aiel/blob/main/src/Aiel.Gps.HP/README.md).

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

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.
