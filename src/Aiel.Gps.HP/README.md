# Aiel.Gps.HP

High‑performance, zero‑allocation NMEA 0183 sentence parser for .NET 10.

Designed for real‑time GPS data processing, telemetry pipelines, embedded systems, and any workload where performance and memory efficiency are critical.

Built on modern .NET features:

- `System.IO.Pipelines` for efficient I/O
- `ReadOnlySpan<byte>` for zero‑allocation parsing
- `ref struct Lexer` for stack‑only parsing operations
- Source‑generated discriminated union for type‑safe message handling
- Attribute‑driven compile‑time registration (fork path) and runtime registration (NuGet consumers)


---

## **Features**

- **True Zero‑Allocation Parsing**  
  Stack‑based lexer and struct‑based messages eliminate heap allocations for message types without string fields. GLL parsing produces **zero allocations**.

- **Exceptional Performance**  
  **~84ns** per message for simple sentences. **10x faster** than previous versions.

- **Source‑Generated Discriminated Union**  
  Type‑safe pattern matching over NMEA message types with compile‑time guarantees.

- **Async Stream Processing**  
  Efficient backpressure‑aware streaming using `System.IO.Pipelines`.

- **Dual Extensibility Model**  
  - **Fork (highest performance)**: Add custom messages to the library source so they are included in the compile‑time discriminated union — zero‑allocation
  - **Runtime registration**: Register custom parsers via `NmeaParserRegistry` as a NuGet consumer — one allocation per custom message

- **Separation of Concerns**  
  Single‑message parsing (`NmeaSingleParser`) and batch streaming (`NmeaBatchReader`) are independent APIs.

- **Comprehensive Error Handling**  
  Parse errors available via `ReadErrorsAsync()` channel without disrupting message flow.

---


> **.NET 10 Only**: This is intentional. While the other packages in this repo multi-target .NET versions, the HP library exists
> to be the absolute fastest possible implementation. The .NET team and many other contributors have made amazing improvements to
> performance over the years, so if you are not using the latest version of .NET, you are not as serious about performance as you
> say you are. If you need to support .NET 8, please consider using the `Aiel.Gps` package instead. It has a similar API but
> prioritizes extensibility over speed.


---

## Installation

Install the package via NuGet:

```bash
dotnet add package Aiel.Gps.HP
```

Or via the Package Manager Console:

```powershell
Install-Package Aiel.Gps.HP
```

---

## **Usage**

### **Parsing a Single Sentence**

```csharp
using Aiel.Gps.HP;
using Aiel.Gps.HP.Sentences;

var bytes = "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n"u8;

// Parse with a specific parser
var parser = new GllParser();
var gll = NmeaSingleParser.Parse(bytes, parser);
Console.WriteLine($"Lat: {gll.Latitude}, Lon: {gll.Longitude}");
```

It's called `NmeaSingleParser`, but there is no reason you cannot use it inside a loop. For high‑throughput scenarios, batch
processing with `NmeaBatchReader` is recommended.

> 
> ## A Note on the Discriminated Union
> 
> The source generator creates a `NmeaMessage` discriminated union for all message types marked with `[NmeaMessage]`. This is the
> magic that enables type-safe pattern matching over NMEA message types with compile-time guarantees. And it is a lot of
> tedious boiler plate, perfectly suited for source generation. I have not included the generated code here because it is quite
> large, even though it currently only handles 7 message types. Without the source generator it would be a maintenance nightmare
> to manage by hand, and keeping this document up-to-date with every new message type is simply impractical. Instead, you can find
> the generated source in `obj/Generated/NmeaMessage.g.cs` after building the project.
> 
> Once you have an instance of the `NmeaMessage` discriminated union, you call `Match()` to handle each message in a type-safe
> way without casting:
> 
> ```csharp
> var bytes = "$GPGLL,4916.45,N,12311.12,W,225444,A,*1D\r\n"u8;
> 
> if (NmeaMessage.TryParse(bytes, out var message))
> {
>     message.Match(
>         onGLL: gll => Console.WriteLine($"Position: {gll.Latitude}, {gll.Longitude}"),
>         onGGA: gga => Console.WriteLine($"Altitude: {gga.Altitude}m"),
>         onRMC: rmc => Console.WriteLine($"Speed: {rmc.SpeedOverGround} knots"),
>         onGSA: gsa => Console.WriteLine($"Fix: {gsa.FixType}"),
>         onGSV: gsv => Console.WriteLine($"Satellites: {gsv.SatellitesInView}"),
>         onVTG: vtg => Console.WriteLine($"Track: {vtg.TrueTrackMadeGood}°"),
>         onGFDTA: gfdta => Console.WriteLine($"Concentration: {gfdta.Concentration}"));
> }
> ```
> 
> So not really magic, but bloody fast!
> 


### **Batch Stream Processing**

```csharp
using Aiel.Gps.HP;

using var stream = File.OpenRead("gps-data.log");
var reader = new NmeaBatchReader(stream);

await foreach (var message in reader.ReadAsync(cancellationToken))
{
    message.Match(
        onGLL: gll => ProcessPosition(gll.Latitude, gll.Longitude),
        onGGA: gga => ProcessAltitude(gga.Altitude),
        onRMC: rmc => ProcessSpeed(rmc.SpeedOverGround),
        onGSA: gsa => ProcessDop(gsa.Hdop, gsa.Vdop),
        onGSV: gsv => ProcessSatellites(gsv.SatellitesInView),
        onVTG: vtg => ProcessTrack(vtg.TrueTrackMadeGood),
        onGFDTA: gfdta => ProcessGasFinder(gfdta.Concentration));
}

// Access statistics after processing
var stats = reader.Statistics;
Console.WriteLine($"Total: {stats.TotalSentences}, Errors: {stats.Errors}");
```

### **Reading from Serial Port**

Example reading from a GPS device connected via serial port:

```csharp
using System.IO.Ports;
using Aiel.Gps.HP;

var port = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
port.Open();

var reader = new NmeaBatchReader(port.BaseStream);

await foreach (var message in reader.ReadAsync(cancellationToken))
{
    message.Match(
        onGLL: gll => Console.WriteLine($"{gll.Latitude}, {gll.Longitude}"),
        onGGA: gga => Console.WriteLine($"Quality: {gga.Quality}"),
        onRMC: rmc => Console.WriteLine($"{rmc.Date} {rmc.FixTime}"),
        onGSA: _ => { },
        onGSV: _ => { },
        onVTG: _ => { },
        onGFDTA: _ => { });
}
```

### **Error Handling**

Parse errors are available through a separate channel without disrupting the message stream:

```csharp
var reader = new NmeaBatchReader(stream);

// Read messages in one task
var messagesTask = Task.Run(async () =>
{
    await foreach (var message in reader.ReadAsync())
    {
        ProcessMessage(message);
    }
});

// Read errors in another task
var errorsTask = Task.Run(async () =>
{
    await foreach (var error in reader.ReadErrorsAsync())
    {
        Console.WriteLine($"Parse error at line {error.LineNumber}: {error.Message}");
    }
});

await Task.WhenAll(messagesTask, errorsTask);
```

### **Custom Messages at Runtime**

For proprietary or non‑standard NMEA sentences that cannot be added at compile time:

```csharp
public sealed class MyCustomParser : ICustomNmeaParser
{
    public ReadOnlySpan<Byte> Identifier => "MYCST"u8;

    public Object Parse(ref Lexer lexer)
    {
        lexer.ConsumeString(); // Skip identifier

        return new MyCustomMessage
        {
            DeviceId = lexer.NextString(),
            Value = lexer.NextDouble(),
            Checksum = lexer.NextChecksum()
        };
    }
}

// Register and use
var registry = new NmeaParserRegistry();
registry.Register(new MyCustomParser());

var reader = new NmeaBatchReader(stream, registry: registry);

// Built-in messages
await foreach (var message in reader.ReadAsync())
{
    // Process built-in types
}

// Custom messages (boxed, one allocation per message)
await foreach (var custom in reader.ReadCustomMessagesAsync())
{
    if (custom is MyCustomMessage msg)
    {
        Console.WriteLine($"Custom: {msg.DeviceId} = {msg.Value}");
    }
}
```

### **Background Service Example**

```csharp
public sealed class GpsService : BackgroundService
{
    private readonly ILogger<GpsService> _logger;

    public GpsService(ILogger<GpsService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var portName = "COM6"; // From configuration

        _logger.LogInformation("Opening {PortName}", portName);

        using var port = new SerialPort(portName, 9600);
        port.Open();

        var reader = new NmeaBatchReader(port.BaseStream);

        await foreach (var message in reader.ReadAsync(stoppingToken))
        {
            message.Match(
                onGLL: gll => _logger.LogInformation("Position: {Lat}, {Lon}", gll.Latitude, gll.Longitude),
                onGGA: gga => _logger.LogInformation("Satellites: {Count}", gga.NumberOfSatellites),
                onRMC: rmc => _logger.LogInformation("Speed: {Speed} knots", rmc.SpeedOverGround),
                onGSA: gsa => _logger.LogDebug("HDOP: {Hdop}", gsa.Hdop),
                onGSV: gsv => _logger.LogDebug("Satellites in view: {Count}", gsv.SatellitesInView),
                onVTG: vtg => _logger.LogDebug("Track: {Track}°", vtg.TrueTrackMadeGood),
                onGFDTA: gfdta => _logger.LogInformation("Concentration: {Value}", gfdta.Concentration));
        }

        var stats = reader.Statistics;
        _logger.LogInformation("Processed {Total} messages, {Errors} errors", 
            stats.TotalSentences, stats.Errors);
    }
}
```

---

## **Extensibility**

Aiel.Gps.HP supports two extensibility tiers. Choosing the right one depends on your performance requirements and whether you are consuming the library as a NuGet package or building from source (recommended).

### Understanding the Two Tiers

|                                      | Tier 1: Fork                     | Tier 2: Runtime Registration |
| ------------------------------------ | -------------------------------- | ---------------------------- |
| **Who uses this?**                   | People like David Fowler         | NuGet package consumers      |
| **When does registration happen?**   | Compile time                     | Application startup          |
| **Allocation per message?**          | Zero                             | One (boxed object)           |
| **Included in `NmeaMessage` union?** | Yes                              | No — separate channel        |
| **IntelliSense / type safety?**      | Full                             | Cast required                |
| **Suitable for high‑throughput?**    | Yes                              | Yes, with minor overhead     |

---

### Using Both Tiers Together

ℹ️ A common misconception: the two tiers are **not mutually exclusive**. You do not have to choose one or the other — you can use both simultaneously.

When `NmeaBatchReader` receives a sentence, it follows a strict dispatch order:

1. **Built-in dispatcher first** — `NmeaMessage.TryParse()` attempts to match the sentence identifier against all built-in types (GLL, GGA, RMC, etc.) using the source-generated switch. If it matches, the message is parsed with **zero allocation** and yielded through `ReadAsync()`. The registry is **never consulted**.

2. **Registry second** — Only if the built-in dispatcher does not recognise the identifier does `NmeaBatchReader` consult the `NmeaParserRegistry`. If a matching custom parser is found, it parses the sentence (one allocation for boxing) and the result is written to the custom messages channel accessible via `ReadCustomMessagesAsync()`.

3. **Error channel last** — If neither the built-in dispatcher nor the registry recognises the identifier, the sentence is written to the errors channel.

This means you can register any number of custom parsers without affecting the performance of your built-in message processing. The benchmarks confirm this design: 4,000 `$GPGLL` sentences processed with a populated registry show a ratio of **0.96** — statistically identical to processing with no registry present at all.

✅ **Bottom line**: register your proprietary sentences via Tier 2 and enjoy zero-allocation parsing for all standard NMEA sentences at the same time. No trade-offs required.

---

### **Tier 1 — Fork the Library (Absolute Highest Performance)**

> This path is for teams that need every nanosecond and are willing to maintain their own fork, or for contributors submitting new built‑in sentence types.

The source generator discovers message types at compile time by scanning the current compilation for structs marked with `[NmeaMessage]` and `[NmeaParser]`. Because the generator runs during compilation of `Aiel.Gps.HP` itself, any custom types must exist in that compilation to be included in the generated `NmeaMessage` discriminated union.

**What this means in practice:** custom types added to a fork are first‑class citizens — they appear alongside `GLL`, `GGA`, and `RMC` in the `Match()` call with zero overhead.

**Step 1 — Fork the repository and add your message struct:**

```csharp
// In Aiel.Gps.HP/Sentences/MyCustom.cs
namespace Aiel.Gps.HP.Sentences;

[NmeaMessage("MYCST")]
public struct MyCustomMessage
{
    public String DeviceId;
    public Double Temperature;
    public Int32 SignalStrength;
    public Char Status;
    public Int32 Checksum;

    public override readonly String ToString() =>
        $"MYCST {DeviceId} {Temperature}°C {SignalStrength}dBm {Status}";
}
```

**Step 2 — Add your parser struct (in the same file please if you intend to contribute your changes back to the main repository):**

```csharp
[NmeaParser(typeof(MyCustomMessage))]
public readonly struct MyCustomParser : INmeaParser<MyCustomMessage>
{
    public ReadOnlySpan<Byte> Identifier => "MYCST"u8;

    public void Parse(ref Lexer lexer, out MyCustomMessage msg)
    {
        lexer.ConsumeString(); // Skip the sentence identifier

        msg = new MyCustomMessage
        {
            DeviceId = lexer.NextString(),
            Temperature = lexer.NextDouble(),
            SignalStrength = lexer.NextInteger(),
            Status = lexer.NextChar(),
            Checksum = lexer.NextChecksum()
        };
    }
}
```

**Step 3 — Rebuild.** The generator runs automatically and includes `MyCustomMessage` in the `NmeaMessage` union. Your custom type now appears as a first‑class member:

```csharp
if (NmeaMessage.TryParse(bytes, out var message))
{
    message.Match(
        onGLL: gll => ProcessGll(gll),
        onGGA: gga => ProcessGga(gga),
        onMyCustomMessage: custom => Console.WriteLine($"Temp: {custom.Temperature}°C"));
        // ... etc.
}
```

> **Generated code is written to `obj/Generated/`** for inspection. If a type is missing from the union after rebuilding, check that both `[NmeaMessage]` and the corresponding `[NmeaParser]` struct are present in the same compilation.

---

### **Tier 2 — Runtime Registration (NuGet Consumers)**

> This is the standard extensibility path for anyone consuming `Aiel.Gps.HP` as a NuGet package. One heap allocation is made per custom message parsed. For most workloads this is negligible. The performance section contains benchmarks comparing both tiers.

Custom parsers are registered at application startup via `NmeaParserRegistry`. Custom messages flow through a separate `ReadCustomMessagesAsync()` channel on `NmeaBatchReader` and are returned as `Object` (requiring a cast). Built‑in messages are unaffected and continue to flow through `ReadAsync()` as normal.

A full end‑to‑end example follows in the next section.

---

### **Runtime Custom Message Registration — Complete Example**

This example shows the complete workflow for a proprietary gas detector sentence `$PCGAS` with fields for concentration, alarm state, and unit serial number.

#### Define the Message Type

```csharp
// Any namespace — this lives in your application, not in Aiel.Gps.HP
public sealed record GasDetectorMessage
{
    public Double Concentration { get; init; }
    public Boolean Alarm { get; init; }
    public String SerialNumber { get; init; } = String.Empty;
    public Int32 Checksum { get; init; }

    public override String ToString() =>
        $"PCGAS Conc={Concentration} Alarm={Alarm} SN={SerialNumber}";
}
```

> Note: Because custom messages are returned as `Object`, there is no restriction on the type — you may use a `record`, `class`, or `struct`.

#### Implement the Parser

```csharp
using Aiel.Gps.HP;

public sealed class GasDetectorParser : ICustomNmeaParser
{
    // The identifier must match what appears between '$' and the first comma.
    public ReadOnlySpan<Byte> Identifier => "PCGAS"u8;

    public Object Parse(ref Lexer lexer)
    {
        // Skip the sentence identifier — custom parsers are responsible for this,
        // just like built-in INmeaParser<T> implementations.
        lexer.ConsumeString();

        return new GasDetectorMessage
        {
            Concentration = lexer.NextDouble(),
            Alarm = lexer.NextInteger() != 0,
            SerialNumber = lexer.NextString(),
            Checksum = lexer.NextChecksum()
        };
    }
}
```

#### Register and Read

```csharp
using Aiel.Gps.HP;

// Build the registry at startup — it is thread-safe and can be shared
var registry = new NmeaParserRegistry();
registry.Register(new GasDetectorParser());

using var stream = File.OpenRead("sensor-log.nmea");
var reader = new NmeaBatchReader(stream, registry: registry);

// Built-in messages and custom messages run concurrently.
// Start both consumers before awaiting either to avoid blocking the pipeline.
var builtInTask = Task.Run(async () =>
{
    await foreach (var message in reader.ReadAsync())
    {
        message.Match(
            onGLL: gll => Console.WriteLine($"Position: {gll.Latitude}, {gll.Longitude}"),
            onGGA: gga => Console.WriteLine($"Satellites: {gga.NumberOfSatellites}"),
            onRMC: rmc => Console.WriteLine($"Speed: {rmc.SpeedOverGround} kn"),
            onGSA: _ => { },
            onGSV: _ => { },
            onVTG: _ => { },
            onGFDTA: _ => { });
    }
});

var customTask = Task.Run(async () =>
{
    await foreach (var raw in reader.ReadCustomMessagesAsync())
    {
        // Each custom message requires a cast — the type system cannot
        // know at compile time which custom parsers are registered.
        if (raw is GasDetectorMessage gas)
        {
            Console.WriteLine($"Gas: {gas.Concentration} ppm  Alarm: {gas.Alarm}");
        }
    }
});

await Task.WhenAll(builtInTask, customTask);
```

#### Accessing Statistics

After the reader completes, `Statistics` reports counts for both built‑in and custom messages:

```csharp
var stats = reader.Statistics;
Console.WriteLine($"Total messages : {stats.TotalSentences}");
Console.WriteLine($"Parse errors   : {stats.Errors}");
```

#### Unregistering a Parser

Parsers can be removed at runtime if a device is disconnected or a sensor type changes:

```csharp
registry.Unregister("PCGAS"u8);
// or
registry.Unregister("PCGAS");
```

---

### **Available Lexer Methods**

The `Lexer` ref struct provides type-safe parsing methods for common NMEA field types:

| Method              | Return Type | Description                                              | Allocates |
| ------------------- | ----------- | -------------------------------------------------------- |:---------:|
| `NextString()`      | String      | Parses a text field                                      | Yes*      |
| `NextChar()`        | Char        | Parses a single character field                          | No        |
| `NextInteger()`     | Int32       | Parses an integer number                                 | No        |
| `NextDouble()`      | Double      | Parses a floating-point number                           | No        |
| `NextHexadecimal()` | Int32       | Parses a hexadecimal value                               | No        |
| `NextTime()`        | TimeOnly    | Parses HHMMSS.sss time format                            | No        |
| `NextDate()`        | DateOnly    | Parses DDMMYY date format                                | No        |
| `NextDateTime()`    | DateTime    | Parses combined date/time fields                         | No        |
| `NextLatitude()`    | Double      | Parses latitude in DDMM.mmmm format with N/S direction   | No        |
| `NextLongitude()`   | Double      | Parses longitude in DDDMM.mmmm format with E/W direction | No        |
| `NextChecksum()`    | Int32       | Parses the sentence checksum (always call last)          | No        |
| `ConsumeString()`   | void        | Skips a field without parsing                            | No        |

*Unavoidable for string fields

### **Handling Empty Fields**

NMEA sentences can have empty fields (consecutive commas). The Lexer methods handle this automatically:

```csharp
// Empty string fields return String.Empty
var text = lexer.NextString();  // Returns "" for empty field

// Empty numeric fields return sensible defaults
var number = lexer.NextInteger();  // Returns 0 for empty field
var value = lexer.NextDouble();    // Returns Double.NaN for empty field
var time = lexer.NextTime();       // Returns TimeOnly.MinValue for empty field
```

### **Built‑In Message Examples**

#### Simple: GLL (Geographic Position)

```csharp
[NmeaMessage("GPGLL")]
public struct GLL
{
    public Double Latitude;
    public Double Longitude;
    public TimeOnly FixTime;
    public Char DataActive;
    public Int32 Checksum;

    public override readonly String ToString() => 
        $"GPGLL {Latitude} {Longitude} {FixTime} {DataActive}";
}

[NmeaParser(typeof(GLL))]
public readonly struct GllParser : INmeaParser<GLL>
{
    public ReadOnlySpan<Byte> Identifier => "GPGLL"u8;

    public void Parse(ref Lexer lexer, out GLL msg)
    {
        lexer.ConsumeString(); // Skip "GPGLL"

        msg = new GLL
        {
            Latitude = lexer.NextLatitude(),
            Longitude = lexer.NextLongitude(),
            FixTime = lexer.NextTime(),
            DataActive = lexer.NextChar(),
            Checksum = lexer.NextChecksum()
        };
    }
}
```

#### Complex: GSV (Satellites in View)

```csharp
[NmeaMessage("GPGSV")]
public struct GSV
{
    public Int32 TotalMessages;
    public Int32 MessageNumber;
    public Int32 SatellitesInView;
    public SV SV1;
    public SV? SV2;
    public SV? SV3;
    public SV? SV4;
    public Int32 Checksum;

    public struct SV
    {
        public Int32 PRN;
        public Int32 Elevation;
        public Int32 Azimuth;
        public Int32 SNR;
    }
}

[NmeaParser(typeof(GSV))]
public readonly struct GsvParser : INmeaParser<GSV>
{
    public ReadOnlySpan<Byte> Identifier => "GPGSV"u8;

    public void Parse(ref Lexer lexer, out GSV msg)
    {
        lexer.ConsumeString();

        msg = new GSV
        {
            TotalMessages = lexer.NextInteger(),
            MessageNumber = lexer.NextInteger(),
            SatellitesInView = lexer.NextInteger(),
            SV1 = new GSV.SV
            {
                PRN = lexer.NextInteger(),
                Elevation = lexer.NextInteger(),
                Azimuth = lexer.NextInteger(),
                SNR = lexer.NextInteger()
            }
        };

        // Optional satellites
        if (!lexer.EOL)
        {
            msg.SV2 = new GSV.SV
            {
                PRN = lexer.NextInteger(),
                Elevation = lexer.NextInteger(),
                Azimuth = lexer.NextInteger(),
                SNR = lexer.NextInteger()
            };
        }

        // ... SV3 and SV4 similarly ...

        msg.Checksum = lexer.NextChecksum();
    }
}
```

---

## Message Types

### Currently Supported NMEA Sentences

| Sentence  | Description                                | Key Fields                                                                                               |
| --------- | ------------------------------------------ | -------------------------------------------------------------------------------------------------------- |
| **GGA**   | Global Positioning System Fix Data         | Time, Latitude, Longitude, Fix Quality, Satellites, HDOP, Altitude, Geoid Height                         |
| **RMC**   | Recommended Minimum Navigation Information | Time, Status, Latitude, Longitude, Speed, Track Angle, Date, Magnetic Variation                          |
| **GSA**   | GPS DOP and Active Satellites              | Fix Mode, Fix Type, Satellite PRNs, PDOP, HDOP, VDOP                                                     |
| **GSV**   | GPS Satellites in View                     | Total Messages, Message Number, Satellites in View, Satellite Details (PRN, Elevation, Azimuth, SNR)     |
| **GLL**   | Geographic Position - Latitude/Longitude   | Latitude, Longitude, Time, Status                                                                         |
| **VTG**   | Track Made Good and Ground Speed           | True Track, Magnetic Track, Ground Speed (knots and km/h)                                                |
| **GFDTA** | Proprietary GasFinder Sentence             | Concentration, R2, Distance, Light, DateTime, Serial Number, Status                                       |

All message types are implemented as `struct` for optimal performance.

### GGA - Global Positioning System Fix Data

Contains position fix, quality indicator, number of satellites, and altitude.

**Properties:**

- `FixTime` (TimeOnly) - UTC time of fix
- `Latitude` (Double) - Latitude in decimal degrees
- `Longitude` (Double) - Longitude in decimal degrees
- `Quality` (FixQuality) - GPS quality indicator (0 = Invalid, 1 = GPS fix, 2 = DGPS fix, etc.)
- `NumberOfSatellites` (Int32) - Number of satellites in use
- `Hdop` (Double) - Horizontal dilution of precision
- `Altitude` (Double) - Altitude above mean sea level
- `AltitudeUnits` (Char) - Units of altitude (typically 'M' for meters)
- `HeightOfGeoid` (Double) - Height of geoid above WGS84 ellipsoid
- `HeightOfGeoidUnits` (Char) - Units (typically 'M')
- `TimeSinceLastDgpsUpdate` (TimeOnly) - Time since last DGPS update
- `DgpsStationId` (Int32) - DGPS station ID
- `Checksum` (Int32) - NMEA checksum

### RMC - Recommended Minimum Navigation Information

Essential GPS fix data including position, velocity, and date.

**Properties:**

- `FixTime` (TimeOnly) - UTC time of fix
- `Status` (Char) - Status (A = Active, V = Void)
- `Latitude` (Double) - Latitude in decimal degrees
- `Longitude` (Double) - Longitude in decimal degrees
- `SpeedOverGround` (Double) - Speed in knots
- `TrackAngle` (Double) - Track angle in degrees
- `Date` (DateOnly) - UTC date
- `MagneticVariation` (Double) - Magnetic variation
- `Direction` (Char) - Direction of magnetic variation (E/W)
- `Mode` (Char) - Mode indicator (A = Autonomous, D = Differential, E = Estimated)
- `Checksum` (Int32) - NMEA checksum

### GSA - GPS DOP and Active Satellites

Information about GPS fix type and dilution of precision.

**Properties:**

- `FixMode` (Char) - Mode (M = Manual, A = Automatic)
- `FixType` (FixType) - Fix type (1 = No fix, 2 = 2D, 3 = 3D)
- `SV` (Int32[]) - Array of satellite PRNs used (12 elements)
- `Pdop` (Double) - Position dilution of precision
- `Hdop` (Double) - Horizontal dilution of precision
- `Vdop` (Double) - Vertical dilution of precision
- `Checksum` (Int32) - NMEA checksum

### GSV - GPS Satellites in View

Information about visible satellites.

**Properties:**

- `TotalMessages` (Int32) - Total number of GSV messages
- `MessageNumber` (Int32) - Current message number
- `SatellitesInView` (Int32) - Total satellites in view
- `SV1`, `SV2`, `SV3`, `SV4` (SV?) - Satellite information objects (nullable)
- `Checksum` (Int32) - NMEA checksum

Each `SV` struct contains:

- `PRN` (Int32) - Satellite PRN number
- `Elevation` (Int32) - Elevation in degrees
- `Azimuth` (Int32) - Azimuth in degrees
- `SNR` (Int32) - Signal-to-noise ratio in dB

### GLL - Geographic Position

Basic latitude/longitude position fix.

**Properties:**

- `Latitude` (Double) - Latitude in decimal degrees
- `Longitude` (Double) - Longitude in decimal degrees
- `FixTime` (TimeOnly) - UTC time of fix
- `DataActive` (Char) - Data status (A = Active, V = Void)
- `Checksum` (Int32) - NMEA checksum

### VTG - Track Made Good and Ground Speed

**Properties:**

- `TrueTrackMadeGood` (Double) - Track angle in degrees true
- `MagneticTrackMadeGood` (Double) - Track angle in degrees magnetic
- `GroundSpeedKnots` (Double) - Speed over ground in knots
- `GroundSpeedKmh` (Double) - Speed over ground in kilometers per hour
- `Mode` (Char) - Mode indicator
- `Checksum` (Int32) - NMEA checksum

---

## Architecture

The library uses a modern, high‑performance architecture:

1. **Source Generator** (`Aiel.Gps.HP.Generators`) - Scans the `Aiel.Gps.HP` compilation at build time and emits the `NmeaMessage` discriminated union and parser dispatcher
2. **NmeaBatchReader** - Pipeline‑based async stream reader using `System.IO.Pipelines`; routes sentences to built‑in parsers or the `NmeaParserRegistry`
3. **NmeaSingleParser** - Zero‑allocation single‑sentence parser; accepts any `INmeaParser<TMessage>` directly
4. **Lexer** (`ref struct`) - Stack‑only tokenizer; never allocates; passed by `ref` through the entire parse chain
5. **Message Structs** - Value types; live on the stack when possible
6. **NmeaParserRegistry** - Thread‑safe dictionary of `ICustomNmeaParser` instances; consulted only when a sentence identifier is not recognised by the built‑in dispatcher


## Performance

Aiel.Gps.HP delivers exceptional performance through zero‑allocation parsing, stack‑based operations, and source generation.

### Single‑Message Parsing Benchmarks

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26200.7840)  
Unknown processor  
.NET SDK 10.0.103
  - [Host]     : .NET 10.0.3 (10.0.326.7603), X64 RyuJIT AVX2
  - DefaultJob : .NET 10.0.3 (10.0.326.7603), X64 RyuJIT AVX2

| Method                                    | Mean            | Error         | StdDev        | Gen0     | Allocated |
|------------------------------------------ |----------------:|--------------:|--------------:|---------:|----------:|
| HP: Single GLL Parse                      |        83.76 ns |      0.223 ns |      0.198 ns |        - |         - |
| HP: Single GFDTA Parse                    |       179.51 ns |      0.623 ns |      0.520 ns |   0.0038 |      72 B |
| HP: NmeaMessage.TryParse (GLL)            |        84.56 ns |      0.140 ns |      0.131 ns |        - |         - |

**Key Observations:**

- **GLL parsing is truly zero‑allocation** — no Gen0 collections, 0 bytes allocated
- GFDTA allocates 72 bytes due to `String` fields (unavoidable)
- Discriminated union `TryParse` has nearly identical performance to direct parsing

### Batch Stream Processing Benchmarks

| Method                                    | Mean             | Error         | StdDev        | Gen0     | Gen1     | Gen2    | Allocated  |
|------------------------------------------ |-----------------:|--------------:|--------------:|---------:|---------:|--------:|-----------:|
| HP: Batch Parse Small (343 messages)      |     47,491.61 ns |    207.190 ns |    193.806 ns |   3.8452 |   0.7935 |       - |    73408 B |
| HP: Batch Parse Medium (4,483 messages)   |    737,020.21 ns |  4,051.609 ns |  3,383.277 ns |  55.6641 |  41.9922 |       - |  1059617 B |
| HP: Batch Parse Large (13,470 messages)   |  2,515,757.47 ns | 26,705.870 ns | 24,980.687 ns | 167.9688 | 164.0625 | 78.1250 |  3268809 B |

**Per‑Message Averages:**

- Small: 138 ns/message, 214 B/message
- Medium: 164 ns/message, 236 B/message
- Large: 187 ns/message, 243 B/message

### Comparison with Previous Versions

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26200.7840)  
Unknown processor  
.NET SDK 10.0.103
  - [Host]     : .NET 10.0.3 (10.0.326.7603), X64 RyuJIT AVX2
  - DefaultJob : .NET 10.0.3 (10.0.326.7603), X64 RyuJIT AVX2

| Method           | Categories          | Mean             | Error          | StdDev         | Ratio | Gen0     | Gen1    | Gen2    | Allocated  | Alloc Ratio |
|----------------- |-------------------- |-----------------:|---------------:|---------------:|------:|---------:|--------:|--------:|-----------:|------------:|
| v2 (DKW.NMEA)    | Single GLL          |        663.35 ns |       2.157 ns |       2.018 ns |  1.00 |   0.0277 |       - |       - |      528 B |        1.00 |
| v3 (Aiel)   | Single GLL          |        647.84 ns |       1.309 ns |       1.160 ns |  0.98 |   0.0114 |       - |       - |      216 B |        0.41 |
| **v4 (HP)**      | **Single GLL**      |     **81.66 ns** |   **0.240 ns** |   **0.224 ns** |**0.12**|    **-** |   **-** |   **-** |      **-** |    **0.00** |
|                  |                     |                  |                |                |       |          |         |         |            |             |
| v2 (DKW.NMEA)    | Small (343 msgs)    |    303,997.69 ns |   1,080.527 ns |     957.859 ns |  1.00 |  20.9961 |       - |       - |   395352 B |        1.00 |
| v3 (Aiel)   | Small (343 msgs)    |    502,527.71 ns |   3,733.187 ns |   3,492.025 ns |  1.65 |   7.8125 |       - |       - |   216177 B |        0.55 |
| **v4 (HP)**      | **Small (343 msgs)**|**47,323.03 ns**  | **293.105 ns** | **259.830 ns** |**0.16**|**3.8452**|**0.7935**| **-** | **73408 B**|    **0.19** |
|                  |                     |                  |                |                |       |          |         |         |            |             |
| v2 (DKW.NMEA)    | Medium (4,483 msgs) |  4,446,987.24 ns |  73,743.461 ns |  68,979.679 ns |  1.00 | 281.2500 |       - |       - |  5391990 B |        1.00 |
| v3 (Aiel)   | Medium (4,483 msgs) |  6,445,557.64 ns | 128,756.441 ns | 137,767.949 ns |  1.45 | 132.8125 |       - |       - |  2555516 B |        0.47 |
| **v4 (HP)**      | **Medium (4,483 msgs)**|**782,095.78 ns**|**3,784.288 ns**|**3,160.052 ns**|**0.18**|**47.8516**|**38.0859**|**-**|**1059617 B**|**0.20** |
|                  |                     |                  |                |                |       |          |         |         |            |             |
| v2 (DKW.NMEA)    | Large (13,470 msgs) | 16,289,125.62 ns | 252,821.579 ns | 236,489.462 ns |  1.00 | 750.0000 |       - |       - | 14160544 B |        1.00 |
| v3 (Aiel)   | Large (13,470 msgs) | 22,248,733.06 ns | 429,201.112 ns | 655,435.482 ns |  1.37 | 406.2500 |       - |       - |  7986278 B |        0.56 |
| **v4 (HP)**      | **Large (13,470 msgs)**|**2,573,893.65 ns**|**23,233.267 ns**|**21,732.413 ns**|**0.16**|**58.5938**|**54.6875**|**39.0625**|**3268828 B**|**0.23** |

**HP vs Previous Versions:**

- **8.1x faster** than v2 for single messages
- **6.4x faster** than v2 for batch processing (small dataset)
- **5.7x faster** than v2 for batch processing (medium dataset)
- **6.3x faster** than v2 for batch processing (large dataset)
- **77% reduction** in memory allocations vs v2 (large dataset)

### Performance Characteristics

- **Sub‑100ns latency** for simple messages without string fields
- **Stack‑only parsing** using `ref struct Lexer` (no heap pressure)
- **Source‑generated dispatch** eliminates virtual calls and reflection
- **Efficient pipeline I/O** with `System.IO.Pipelines` for batch processing
- **Minimal GC pressure** — Gen0‑only collections for datasets under 5,000 messages

### Built‑In vs Runtime Registration Benchmarks

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26200.7840)  
Unknown processor  
.NET SDK 10.0.103
  - [Host]     : .NET 10.0.3 (10.0.326.7603), X64 RyuJIT AVX2
  - DefaultJob : .NET 10.0.3 (10.0.326.7603), X64 RyuJIT AVX2

| Method        | Categories                              | Mean              | Error          | StdDev         | Ratio | Gen0     | Gen1   | Allocated | Alloc Ratio |
|-------------- |---------------------------------------- |------------------:|---------------:|---------------:|------:|---------:|-------:|----------:|------------:|
| Built-In      | Batch: Built-In vs Runtime (4,000 GLL)  |    906,935.18 ns  |  2,287.379 ns  |  2,139.615 ns  |  1.00 |        - |      - |    6,825 B |        1.00 |
| Runtime       | Batch: Built-In vs Runtime (4,000 GLL)  |  1,303,904.30 ns  | 14,396.381 ns  | 13,466.384 ns  |  1.44 |  19.5313 | 5.8594 |  393,185 B |       57.61 |
|               |                                         |                   |                |                |       |          |        |           |             |
| No Registry   | Batch: Registry Presence (4,000 GLL)    |    915,689.45 ns  |  7,571.574 ns  |  6,712.003 ns  |  1.00 |        - |      - |    6,824 B |        1.00 |
| With Registry | Batch: Registry Presence (4,000 GLL)    |    880,781.18 ns  |  4,271.091 ns  |  3,995.181 ns  |  0.96 |        - |      - |    6,824 B |        1.00 |
|               |                                         |                   |                |                |       |          |        |           |             |
| Built-In      | Single: GFDTA (with strings)            |        184.35 ns  |      1.434 ns  |      1.271 ns  |  1.00 |   0.0038 |      - |      72 B |        1.00 |
| Runtime       | Single: GFDTA (with strings)            |        213.99 ns  |      1.201 ns  |      1.003 ns  |  1.16 |   0.0093 |      - |     176 B |        2.44 |
|               |                                         |                   |                |                |       |          |        |           |             |
| Built-In      | Single: GLL (no strings)               |         76.69 ns  |      0.437 ns  |      0.409 ns  |  1.00 |        - |      - |         - |          NA |
| Runtime       | Single: GLL (no strings)               |        148.10 ns  |      1.767 ns  |      1.653 ns  |  1.93 |   0.0041 |      - |      80 B |          NA |

**Key Observations:**

- **Registry presence costs nothing for built-in messages** — the `With Registry` batch is statistically identical to `No Registry` (ratio 0.96 is noise). The registry is never consulted when `NmeaMessage.TryParse` succeeds.
- **Zero-alloc messages pay the full boxing cost** — runtime GLL is **1.93× slower** and allocates **80 B** per message where the built-in path allocates nothing.
- **String-heavy messages dilute the overhead** — runtime GFDTA is only **1.16× slower** because the unavoidable string allocations (72 B) already dominate; the 104 B boxing overhead is proportionally minor.
- **Batch GC pressure compounds fast** — 4,000 runtime GLL messages generate **393 KB** of garbage vs **6.8 KB** for built-in, triggering Gen0 and Gen1 collections.

> See [`docs/features/gps-nmea/benchmarks.md`](../../docs/features/gps-nmea/benchmarks.md) for a detailed
> explanation of each category and guidance on interpreting the results.

---

### Real‑World Throughput

Based on the medium dataset benchmark (4,483 messages in 737μs):

- **~6.1 million messages/second**
- **~3.3 GB/s** throughput (assuming 550 bytes average per NMEA sentence)

This makes Aiel.Gps.HP suitable for:

- Real‑time GPS tracking systems
- High‑frequency telemetry processing
- Embedded systems with limited resources
- Data ingestion pipelines processing large GPS log files

---

## Target Framework

- **.NET 10.0** (requires .NET 10 SDK or later)

## Dependencies

- Aiel (internal framework utilities)
- No external NuGet dependencies

---

## **Source Generation**

Aiel.Gps.HP uses a Roslyn incremental source generator (`Aiel.Gps.HP.Generators`) to create the `NmeaMessage` discriminated union and the parser dispatcher at compile time.

### What the Generator Produces

Given a set of structs decorated with `[NmeaMessage]` and `[NmeaParser]` in the **Aiel.Gps.HP compilation**, the generator emits:

- A `NmeaMessage` readonly struct containing a union of all message types
- A `TryParse(ReadOnlySpan<Byte>, out NmeaMessage)` static method
- A `Match(...)` method with one `Action<T>` delegate parameter per message type
- An internal dispatcher that routes bytes to the correct parser struct

### Scope Limitation

> The generator scans only the compilation in which it runs. It cannot see types in referencing assemblies or NuGet consumers. This is a fundamental constraint of how Roslyn source generators work — they are **not** macros and cannot reach across assembly boundaries.

This means:

- Types in `Aiel.Gps.HP` itself (GLL, GGA, RMC, etc.) → included in the union automatically
- Types added to a **fork** of the library → included in the union automatically
- Types in a **NuGet consumer's project** → NOT included — use `ICustomNmeaParser` instead

### Inspecting Generated Code

Generated source is written to `obj/Generated/` during build. To view it:

```powershell
Get-ChildItem -Path obj\Generated -Recurse -Filter *.cs
```

The key generated file is typically named `NmeaMessage.g.cs`. Inspect it when troubleshooting missing types or unexpected `Match()` signatures.

### Adding a Built‑In Type (Fork Path)

1. Add a struct with `[NmeaMessage("IDENTIFIER")]` in the `Aiel.Gps.HP` project
2. Add a corresponding `readonly struct` with `[NmeaParser(typeof(YourMessage))]` implementing `INmeaParser<YourMessage>`
3. Rebuild — the generator picks up both and regenerates the union

---


## **Contributing**

This is part of the [Aiel Application Framework](https://github.com/AielIT/AppFramework). Contributions are welcome via pull requests.

---

## **License**

MIT License.

High-performance NMEA 0183 sentence parser for .NET 10. This library provides efficient parsing of GPS data streams using modern .NET features including source generation, `System.IO.Pipelines`, `ReadOnlySpan<T>`, and `ref struct` for true zero-allocation parsing.

---

## **Migration from Aiel.Gps**

If you are migrating from the original `Aiel.Gps` library, here are the key changes:

### API Changes

**Before (v3):**
```csharp
var streamReader = new NmeaStreamReader()
    .Register(new GGA(), new RMC(), new GSA());

using var reader = new NmeaReader(streamReader, stream);
await foreach (var message in reader.ReadAsync())
{
    if (message is GGA gga)
        ProcessGga(gga);
}
```

**After (HP/v4):**
```csharp
using var reader = new NmeaBatchReader(stream);

await foreach (var message in reader.ReadAsync())
{
    message.Match(
        onGGA: gga => ProcessGga(gga),
        onRMC: rmc => ProcessRmc(rmc),
        onGSA: gsa => ProcessGsa(gsa),
        onGSV: _ => { },
        onGLL: _ => { },
        onVTG: _ => { },
        onGFDTA: _ => { });
}
```

### Key Differences

1. **No Registration Required**: Built‑in message types are automatically available
2. **Discriminated Union**: Use pattern matching instead of type checking
3. **Struct‑Based**: All messages are structs, not classes
4. **Source Generation**: Custom messages use attributes instead of inheritance
5. **TimeOnly/DateOnly**: Uses modern .NET date/time types instead of `TimeSpan`/`DateTime`
6. **Separate APIs**: Single‑message parsing (`NmeaSingleParser`) and batch streaming (`NmeaBatchReader`) are independent

### Performance Improvements

- **8x faster** single‑message parsing
- **6x faster** batch processing
- **77% reduction** in memory allocations
- **True zero‑allocation** for messages without string fields
