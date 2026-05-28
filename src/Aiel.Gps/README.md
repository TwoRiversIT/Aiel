# Aiel.Gps

High‑performance, zero‑allocation NMEA 0183 sentence parser for .NET.  
Designed for real‑time GPS data processing, telemetry pipelines, embedded systems, and any workload where performance and memory efficiency matter.

Built on modern .NET primitives:

- `System.IO.Pipelines`
- `ReadOnlySequence<byte>`
- `Span<T>`
- Zero‑allocation lexing
- Pluggable lexer architecture

---

## **Features**

- **Zero‑Allocation Parsing Path**  
  Optimized lexer eliminates intermediate strings, arrays, and heap allocations.

- **High‑Throughput Performance**  
  Sub‑microsecond parsing for most NMEA sentences.

- **Async Stream Processing**  
  Efficient backpressure‑aware streaming using `System.IO.Pipelines`.

- **Extensible Message Model**  
  Add custom NMEA or proprietary sentences with minimal code.

- **Flexible API**  
  Consumers choose between:
  - raw UTF‑8 slices (`ReadOnlySequence<byte>`)
  - decoded strings
  - strongly‑typed numeric/time fields

- **Configurable Error Handling**  
  Control how unparsed or malformed lines are handled.

---

## Installation

Install the package via NuGet:

```bash
dotnet add package Aiel.Gps
```

Or via the Package Manager Console:

```powershell
Install-Package Aiel.Gps
```

---

## **Usage**

### **Parsing a Single Sentence**

```csharp
var bytes = Encoding.UTF8.GetBytes("$GPGGA,232608.000,5057.1975,N,...*62");
var seq = new ReadOnlySequence<byte>(bytes);

var gga = (GGA)new GGA().Parse(seq);
Console.WriteLine(gga.NumberOfSatellites);
```

### **Basic Stream Parsing**

```csharp
var streamReader = new NmeaStreamReader()
    .Register(new GGA(), new RMC(), new GSA(), new GSV());

using var stream = File.OpenRead("gps-data.log");
using var reader = new NmeaReader(streamReader, stream);

await foreach (var message in reader.ReadAsync(cancellationToken))
{
    if (message is GGA gga)
        Console.WriteLine($"Lat: {gga.Latitude}, Lon: {gga.Longitude}");
}
```

### Reading from Serial Port

Example reading from a GPS device connected via serial port:

```csharp
using System.IO.Ports;
using Aiel.Gps;

var port = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);
port.Open();

var streamReader = new NmeaStreamReader()
    .Register(new GGA(), new RMC(), new GSA());

using var reader = new NmeaReader(streamReader, port.BaseStream);

await foreach (var message in reader.ReadAsync(cancellationToken))
{
    ProcessGpsMessage(message);
}
```

### Error Handling

Control how many unparsed lines are allowed before aborting:

```csharp
var streamReader = new NmeaStreamReader()
{
    AbortAfterUnparsedLines = 10  // Stop after 10 consecutive unparsed lines
}
.Register(new GGA(), new RMC());
```

### Advanced Usage

```csharp
public sealed class GpsService : BackgroundService
{
    private readonly ILogger<GpsService> _logger;
    private readonly NmeaStreamReader _nsr;

    public GpsService(ILogger<GpsService> logger, NmeaStreamReader nmeaStreamReader)
    {
        _logger = logger;
        _nsr = nmeaStreamReader;
        _nsr.Register(new GFDTA(), new GGA(), new GLL(), new GSA(), new GSV(), new RMC(), new VTG());
        _nsr.AbortAfterUnparsedLines = 10;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken = default)
    {
        var portName = "COM6"; // Replace with your actual port name from configuration

        _logger.LogInformation("Opening {0}", portName);
        using (var port = new SerialPortStream(portName))
        {
            port.Open();
            port.Handshake = Handshake.None;
            port.NewLine = "\r\n";
            port.ReadTimeout = 5000;
            port.Write("\r\n");

            var exitReason = await _nsr
                .ParseStreamAsync(
                    port,
                    async (message) => await DispatchMessage(message),
                    stoppingToken);

            _logger.LogInformation("Exit Reason: {ExitReason}", exitReason);
        }
    }

    private Task DispatchMessage(NmeaMessage message)
    {
        _logger.LogInformation(message.ToString());
        return Task.CompletedTask;
    }
}
```

---

## **Extensibility**

The library is designed to be easily extensible. You can create custom parsers for proprietary or
non-standard NMEA sentences by inheriting from `NmeaMessage`. The included `GFDTA` message type is
an example of a custom proprietary sentence.

### Step-by-Step Guide

1. **Create a new class** that inherits from `NmeaMessage`
2. **Define the sentence key** (the identifier that starts each sentence)
3. **Add properties** for the parsed data fields
4. **Implement the Parse method** using the `Lexer` class

### Example: Custom Proprietary Sentence

Here is a complete example showing how to create a parser for a custom NMEA sentence:

```csharp
using System.Buffers;
using System.Text;
using Aiel.Gps;

public class CustomSentence : NmeaMessage
{
    // Define the sentence identifier (e.g., "$PCUST")
    private static readonly ReadOnlyMemory<Byte> KEY = Encoding.UTF8.GetBytes("$PCUST").AsMemory();

    protected override String SentenceIdentifier => "PCUST";

    // Define properties for your data fields
    public String DeviceId { get; private set; } = String.Empty;
    public Double Temperature { get; private set; }
    public Int32 SignalStrength { get; private set; }
    public Char Status { get; private set; }

    public override String ToString() => $"PCUST {DeviceId} {Temperature}°C {SignalStrength}dBm {Status}";

    public override NmeaMessage Parse(ReadOnlySequence<Byte> sentence)
    {
        var lexer = LexerFactory.Create(sentence);

        // Verify the sentence identifier matches
        if (lexer.NextString() != "PCUST")
        {
            throw lexer.Error();
        }

        // Parse each field in order using the appropriate Lexer method
        var custom = new CustomSentence
        {
            DeviceId = lexer.NextString(),
            Temperature = lexer.NextDouble(),
            SignalStrength = lexer.NextInteger(),
            Status = lexer.NextChar(),
            Checksum = lexer.NextChecksum()  // Always parse checksum last
        };

        return custom;
    }
}
```

### Available Lexer Methods

The `Lexer` class provides type-safe parsing methods for common NMEA field types:

| Method | Return Type | Description |
|--------|-------------|-------------|
| `NextString()` | String | Parses a text field |
| `NextChar()` | Char | Parses a single character field |
| `NextInteger()` | Int32 | Parses an integer number |
| `NextDouble()` | Double | Parses a floating-point number |
| `NextHexadecimal()` | Int32 | Parses a hexadecimal value |
| `NextTimeSpan()` | TimeSpan | Parses HHMMSS.sss time format |
| `NextDateTime()` | DateTime | Parses date and time fields |
| `NextLatitude()` | Double | Parses latitude in DDMM.mmmm format with N/S direction |
| `NextLongitude()` | Double | Parses longitude in DDDMM.mmmm format with E/W direction |
| `NextChecksum()` | Int32 | Parses the sentence checksum (always call last) |

### Handling Empty Fields

NMEA sentences can have empty fields (consecutive commas). The Lexer methods handle this automatically:

```csharp
// Empty string fields return String.Empty
var text = lexer.NextString();  // Returns "" for empty field

// Empty numeric fields return sensible defaults
var number = lexer.NextInteger();  // Returns 0 for empty field
var value = lexer.NextDouble();    // Returns Double.NaN for empty field
var time = lexer.NextTimeSpan();   // Returns TimeSpan.Zero for empty field
```

### Using Your Custom Message Type

Once you have created your custom message type, register it with the `NmeaStreamReader`:

```csharp
var streamReader = new NmeaStreamReader()
    .Register(new GGA(), new RMC(), new CustomSentence());

using var stream = GetDataStream();
using var reader = new NmeaReader(streamReader, stream);

await foreach (var message in reader.ReadAsync(cancellationToken))
{
    if (message is CustomSentence custom)
    {
        Console.WriteLine($"Device: {custom.DeviceId}");
        Console.WriteLine($"Temperature: {custom.Temperature}°C");
        Console.WriteLine($"Signal: {custom.SignalStrength}dBm");
    }
}
```

### Real-World Examples

#### Simple: GLL

```csharp
public class GLL : NmeaMessage
{
    public const String Identifier = "GPGLL";

    private static readonly ReadOnlyMemory<Byte> KEY = Encoding.UTF8.GetBytes($"${Identifier}").AsMemory();
    protected override ReadOnlyMemory<Byte> Key => KEY;

    public override NmeaMessage Parse(ReadOnlySequence<Byte> sentence)
    {
        var lexer = LexerFactory.Create(sentence);

        // Skip over GPGLL sentence identifier
        lexer.SkipString();

        return new GLL()
        {
            Latitude = lexer.NextLatitude(),
            Longitude = lexer.NextLongitude(),
            FixTime = lexer.NextTimeSpan(),
            DataActive = lexer.NextChar(),
            Checksum = lexer.NextChecksum()
        };
    }

    public Double Latitude { get; private set; }
    public Double Longitude { get; private set; }
    public TimeSpan FixTime { get; private set; }
    public Char DataActive { get; private set; }

    public override String ToString() => $"GPGLL {Latitude} {Longitude} {FixTime} {DataActive}";
}
```

#### Somewhat More Complex: GSV

```csharp
public class GSV : NmeaMessage
{
    public const String Identifier = "GPGSV";

    private static readonly ReadOnlyMemory<Byte> KEY = Encoding.UTF8.GetBytes($"${Identifier}").AsMemory();
    protected override ReadOnlyMemory<Byte> Key => KEY;

    public override NmeaMessage Parse(ReadOnlySequence<Byte> sentence)
    {
        var lexer = LexerFactory.Create(sentence);

        // Skip over GPGSV
        lexer.SkipString();

        var gsv = new GSV()
        {
            TotalMessages = lexer.NextInteger(),
            MessageNumber = lexer.NextInteger(),
            SatellitesInView = lexer.NextInteger(),
            SV1 = SV.Create(lexer.NextInteger(), lexer.NextInteger(), lexer.NextInteger(), lexer.NextInteger())
        };

        if (!lexer.EOL)
        {
            gsv.SV2 = SV.Create(lexer.NextInteger(), lexer.NextInteger(), lexer.NextInteger(), lexer.NextInteger());
        }

        if (!lexer.EOL)
        {
            gsv.SV3 = SV.Create(lexer.NextInteger(), lexer.NextInteger(), lexer.NextInteger(), lexer.NextInteger());
        }

        if (!lexer.EOL)
        {
            gsv.SV4 = SV.Create(lexer.NextInteger(), lexer.NextInteger(), lexer.NextInteger(), lexer.NextInteger());
        }

        gsv.Checksum = lexer.NextChecksum();
        return gsv;
    }

    public Int32 TotalMessages { get; private set; }
    public Int32 MessageNumber { get; private set; }
    public Int32 SatellitesInView { get; private set; }
    public SV SV1 { get; private set; } = new SV();
    public SV? SV2 { get; private set; }
    public SV? SV3 { get; private set; }
    public SV? SV4 { get; private set; }

    public override String ToString() => $"GPGSV {TotalMessages} {MessageNumber} {SatellitesInView} {SV1} {SV2} {SV3}";

    public class SV
    {
        public Int32 PRN { get; private set; }
        public Int32 Elevation { get; private set; }
        public Int32 Azimuth { get; private set; }
        public Int32 SNR { get; private set; }

        public override String ToString() => $"SV {PRN} {Elevation} {Azimuth} {SNR}";

        internal static SV Create(Int32 prn, Int32 elevation, Int32 azimuth, Int32 snr)
        {
            return new SV()
            {
                PRN = prn,
                Elevation = elevation,
                Azimuth = azimuth,
                SNR = snr
            };
        }
    }
}
```

#### Proprietary: GFDTA

The library includes the `GFDTA` message type as an example of a custom proprietary sentence. This
sentence format is used by proprietary gas detection equipment:

```csharp
public class GFDTA : NmeaMessage
{
    public const String Identifier = "GFDTA";

    private static readonly ReadOnlyMemory<Byte> KEY = Encoding.UTF8.GetBytes($"${Identifier}").AsMemory();
    protected override ReadOnlyMemory<Byte> Key => KEY;

    public override NmeaMessage Parse(ReadOnlySequence<Byte> sentence)
    {
        var lexer = LexerFactory.Create(sentence);

        // Skip over the $GFDTA
        lexer.SkipString();

        var dta = new GFDTA
        {
            Concentration = lexer.NextDouble(),
            R2 = lexer.NextInteger(),
            Distance = lexer.NextDouble(),
            Light = lexer.NextInteger(),
            DateTime = lexer.NextDateTime(),
            SerialNumber = lexer.NextStringSlice(),
            Status = lexer.NextStringSlice(),
            Checksum = lexer.NextChecksum()
        };

        return dta;
    }

    public Double Concentration { get; private set; }
    public Int32 R2 { get; private set; }
    public Double Distance { get; private set; }
    public Int32 Light { get; private set; }
    public DateTime DateTime { get; private set; }
    public ReadOnlySequence<Byte> SerialNumber { get; private set; }
    public ReadOnlySequence<Byte> Status { get; private set; }
    public override String ToString() => $"{Identifier} {Concentration} {R2} {Distance} {Light} {DateTime} {SerialNumber} {Status}";
}
```

---

## Message Types

### Currently Supported NMEA Sentences

| Sentence  | Description                                | Fields                                                                                               |
| --------- | ------------------------------------------ | ---------------------------------------------------------------------------------------------------- |
| **GGA**   | Global Positioning System Fix Data         | Time, Latitude, Longitude, Fix Quality, Satellites, HDOP, Altitude, Geoid Height                     |
| **RMC**   | Recommended Minimum Navigation Information | Time, Status, Latitude, Longitude, Speed, Track Angle, Date, Magnetic Variation                      |
| **GSA**   | GPS DOP and Active Satellites              | Fix Mode, Fix Type, Satellite PRNs, PDOP, HDOP, VDOP                                                 |
| **GSV**   | GPS Satellites in View                     | Total Messages, Message Number, Satellites in View, Satellite Details (PRN, Elevation, Azimuth, SNR) |
| **GLL**   | Geographic Position - Latitude/Longitude   | Latitude, Longitude, Time, Status, Mode                                                              |
| **VTG**   | Track Made Good and Ground Speed           | True Track, Magnetic Track, Ground Speed (knots and km/h)                                            |
| **GFDTA** | Custom Proprietary Sentence (Example)      | Concentration, R2, Distance, Light, DateTime, Serial Number, Status                                  |

### GGA - Global Positioning System Fix Data

Contains position fix, quality indicator, number of satellites, and altitude.

**Properties:**

- `FixTime` (TimeSpan) - UTC time of fix
- `Latitude` (Double) - Latitude in decimal degrees
- `Longitude` (Double) - Longitude in decimal degrees
- `Quality` (FixQuality) - GPS quality indicator (0 = Invalid, 1 = GPS fix, 2 = DGPS fix, etc.)
- `NumberOfSatellites` (Int32) - Number of satellites in use
- `Hdop` (Double) - Horizontal dilution of precision
- `Altitude` (Double) - Altitude above mean sea level
- `AltitudeUnits` (Char) - Units of altitude (typically 'M' for meters)
- `HeightOfGeoid` (Double) - Height of geoid above WGS84 ellipsoid
- `HeightOfGeoidUnits` (Char) - Units (typically 'M')
- `TimeSinceLastDgpsUpdate` (TimeSpan) - Time since last DGPS update
- `DgpsStationId` (Int32) - DGPS station ID

### RMC - Recommended Minimum Navigation Information

Essential GPS fix data including position, velocity, and date.

**Properties:**

- `FixTime` (TimeSpan) - UTC time of fix
- `Status` (Char) - Status (A = Active, V = Void)
- `Latitude` (Double) - Latitude in decimal degrees
- `Longitude` (Double) - Longitude in decimal degrees
- `SpeedOverGround` (Double) - Speed in knots
- `TrackAngle` (Double) - Track angle in degrees
- `Date` (DateTime) - UTC date
- `MagneticVariation` (Double) - Magnetic variation
- `Direction` (Char) - Direction of magnetic variation (E/W)
- `Mode` (Char) - Mode indicator (A = Autonomous, D = Differential, E = Estimated)

### GSA - GPS DOP and Active Satellites

Information about GPS fix type and dilution of precision.

**Properties:**

- `FixMode` (Char) - Mode (M = Manual, A = Automatic)
- `FixType` (FixType) - Fix type (1 = No fix, 2 = 2D, 3 = 3D)
- `SV` (Int32[]) - Array of satellite PRNs used (12 elements)
- `Pdop` (Double) - Position dilution of precision
- `Hdop` (Double) - Horizontal dilution of precision
- `Vdop` (Double) - Vertical dilution of precision

### GSV - GPS Satellites in View

Information about visible satellites.

**Properties:**

- `TotalMessages` (Int32) - Total number of GSV messages
- `MessageNumber` (Int32) - Current message number
- `SatellitesInView` (Int32) - Total satellites in view
- `SV1`, `SV2`, `SV3`, `SV4` (SV) - Satellite information objects

Each `SV` object contains:

- `PRN` (Int32) - Satellite PRN number
- `Elevation` (Int32) - Elevation in degrees
- `Azimuth` (Int32) - Azimuth in degrees
- `SNR` (Int32) - Signal-to-noise ratio in dB

---

## Architecture

The library uses a pipeline-based architecture for high-performance parsing:

1. **NmeaStreamReader** - Manages the parsing pipeline and registered message parsers
2. **NmeaReader** - Provides an async enumerable interface for reading messages from a stream
3. **NmeaMessage** - Abstract base class for all NMEA message types
4. **Lexer** - Low-level tokenizer for parsing NMEA sentence fields

## Performance

The library is designed for high performance:

- Uses `System.IO.Pipelines` for efficient stream processing with backpressure
- Zero-allocation parsing with `ReadOnlySequence<Byte>` and `Span<T>`
- Minimal memory allocations per message
- Efficient string parsing without intermediate allocations
- Suitable for real-time GPS data processing

## Target Frameworks

- .NET 8.0
- .NET 10.0

## Dependencies

- Microsoft.Extensions.Configuration.Json
- Microsoft.Extensions.Logging.Abstractions
- System.IO.Pipelines

## **Zero‑Allocation Lexer Architecture**

The parser uses a pluggable lexer system:

```csharp
LexerFactory.Create = seq => new LexerOptimized(seq);
```

Consumers can choose:

- **Optimized zero‑allocation lexer**  
- **Original string‑producing lexer**  
- **Custom lexers** for specialized workloads

This design keeps the API ergonomic while enabling high‑performance scenarios.

---

## **Performance**

- BenchmarkDotNet v0.14.0, Windows 11 (10.0.26200.7840)
- Unknown processor
- .NET SDK 10.0.103
  - [Host]     : .NET 10.0.3 (10.0.326.7603), X64 RyuJIT AVX2
  - DefaultJob : .NET 10.0.3 (10.0.326.7603), X64 RyuJIT AVX2

| Method                                  | Mean       | Error   | StdDev  | Ratio | Gen0   | Allocated | Alloc Ratio |
|---------------------------------------- |-----------:|--------:|--------:|------:|-------:|----------:|------------:|
| 'Parse GGA (Position Fix)'              | 1,187.9 ns | 1.42 ns | 1.11 ns |  1.00 | 0.0114 |     216 B |        1.00 |
| 'Parse RMC (Recommended Minimum)'       | 1,078.4 ns | 2.20 ns | 2.06 ns |  0.91 | 0.0095 |     208 B |        0.96 |
| 'Parse GSA (DOP and Active Satellites)' |   929.2 ns | 1.77 ns | 1.65 ns |  0.78 | 0.0134 |     256 B |        1.19 |
| 'Parse GSV (Satellites in View)'        |   946.2 ns | 1.78 ns | 1.58 ns |  0.80 | 0.0181 |     344 B |        1.59 |
| 'Parse GLL (Geographic Position)'       |   714.9 ns | 1.96 ns | 1.84 ns |  0.60 | 0.0086 |     168 B |        0.78 |
| 'Parse VTG (Track and Speed)'           |   560.7 ns | 1.81 ns | 1.69 ns |  0.47 | 0.0095 |     184 B |        0.85 |
| 'Parse GFDTA (GasFinder Custom)'        |   961.3 ns | 3.68 ns | 3.45 ns |  0.81 | 0.0114 |     224 B |        1.04 |

Outliers:

- IndividualParserBenchmarks.'Parse GGA (Position Fix)': Default        -> 3 outliers were removed (1.20 us..1.21 us)
- IndividualParserBenchmarks.'Parse GSV (Satellites in View)': Default  -> 1 outlier  was  removed (954.11 ns)
- IndividualParserBenchmarks.'Parse GLL (Geographic Position)': Default -> 1 outlier  was  detected (712.72 ns)

### **Throughput Benchmarks (Full Pipeline)**

- BenchmarkDotNet v0.14.0, Windows 11 (10.0.26200.7840)
- Unknown processor
- .NET SDK 10.0.103
  - [Host]     : .NET 10.0.3 (10.0.326.7603), X64 RyuJIT AVX2
  - DefaultJob : .NET 10.0.3 (10.0.326.7603), X64 RyuJIT AVX2

| Method                            | Mean        | Error     | StdDev    | Gen0     | Allocated  |
|---------------------------------- |------------:|----------:|----------:|---------:|-----------:|
| 'Small Dataset (343 messages)'    |    439.4 us |   1.37 us |   1.28 us |  11.7188 |  240.51 KB |
| 'Medium Dataset (4,483 messages)' |  6,335.0 us |  16.85 us |  15.76 us | 171.8750 |  3141.5 KB |
| 'Large Dataset (13,470 messages)' | 20,255.7 us | 135.69 us | 126.92 us | 333.3333 | 9006.98 KB |
| 'Manual Reading (ReadNextAsync)'  |    455.7 us |   1.91 us |   1.59 us |  11.7188 |  272.06 KB |

Outliers:

- ParsingThroughputBenchmarks.'Small Dataset (343 messages)': Default   -> 1 outlier  was  detected (436.30 us)
- ParsingThroughputBenchmarks.'Manual Reading (ReadNextAsync)': Default -> 2 outliers were removed (461.24 us, 461.82 us)

The medium dataset is the most reliable indicator:  

---

## **Contributing**

This is part of the [Aiel Application Framework](https://github.com/AielIT/AppFramework). Contributions are welcome via pull requests.

### ToDo

- Rewrite NextDateTime() to

---

## **License**

MIT License.

High-performance NMEA 0183 sentence parser for .NET applications. This library provides efficient parsing of GPS data streams using modern .NET features including `System.IO.Pipelines` and `ReadOnlySequence<T>` for zero-allocation parsing.
