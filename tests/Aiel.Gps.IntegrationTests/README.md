# GPS Integration Tests

This project contains integration tests for the Aiel.Gps library using real-world GPS data captured from actual devices.

## Test Data

All test data is stored as embedded resources in the `Aiel\Gps\TestData\` directory and loaded using the `RH` utility class.

### Data Files

| File | Size | Messages | Description |
|------|------|----------|-------------|
| **track1.nmea** | 920KB | 13,470 | Large dataset with GGA, RMC, and GSA sentences |
| **track2.nmea** | 295KB | 4,491 total | Mixed dataset including GGA, RMC, GSA, GSV, and GFDTA |
| **track3.nmea** | 18KB | 357 total | Small dataset with all standard GPS message types |
| **gf.nmea** | 12KB | 735 total | GasFinder custom message data (GFDTA, GFCMD, GFCER) |

### Message Type Distribution

#### track1.nmea
- GGA: 4,490
- RMC: 4,490
- GSA: 4,490
- GSV: 0
- GLL: 0
- VTG: 0
- GFDTA: 0

#### track2.nmea
- GGA: 600
- RMC: 600
- GSA: 600
- GSV: 2,313
- GLL: 0
- VTG: 0
- GFDTA: 370

#### track3.nmea
- GGA: 39
- RMC: 39
- GSA: 39
- GSV: 147
- GLL: 40
- VTG: 39
- GFDTA: 0

#### gf.nmea
- GGA: 0
- RMC: 0
- GSA: 0
- GSV: 0
- GLL: 0
- VTG: 0
- GFDTA: 121

## Test Coverage

### Data Processing Tests

- **ProcessesTrack1Data**: Validates parsing of 13,470 messages from a large GPS stream
- **ProcessesTrack2Data**: Validates parsing of mixed message types including custom GFDTA
- **ProcessesTrack3Data**: Validates parsing of all standard GPS message types
- **ProcessesGasFinderData**: Validates parsing of GasFinder custom messages

### Integration Tests

- **Track1DataContainsValidGpsPositions**: Ensures real-world GPS coordinates are correctly parsed
- **Track2DataContainsMixedMessageTypes**: Validates multiple message type handling
- **Track3DataContainsAllStandardMessageTypes**: Ensures all standard NMEA types are recognized

### Stress Tests

- **LargeDataSetDoesNotExceedBufferCapacity**: Tests BufferBlock capacity with 13,470 messages
- **ReadNextAsyncWorksWithLargeDataSet**: Validates manual reading with large datasets
- **SlowConsumerDoesNotLoseMessages**: Ensures backpressure handling works correctly

## Running the Tests

```powershell
# Run all GPS integration tests
dotnet test tests\Aiel.Gps.IntegrationTests\Aiel.Gps.IntegrationTests.csproj

# Run all GPS tests (unit + integration)
dotnet test --filter "FullyQualifiedName~Aiel.Gps"
```

## Performance Characteristics

Integration tests validate that the library can:
- Process large streams (920KB+ of NMEA data)
- Handle mixed message types efficiently
- Maintain message ordering and accuracy
- Support slow consumers without data loss
- Respect bounded buffer capacity (1000 messages)

## Adding New Test Data

To add new test data:

1. Place the file in `Aiel\Gps\TestData\`
2. Ensure the file uses `*.nmea` or `*.log` extension (automatically embedded)
3. Load using: `RH.GetStream<RealWorldDataTests>("TestData.filename.nmea")`
4. Create test validating expected message counts and types

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.
