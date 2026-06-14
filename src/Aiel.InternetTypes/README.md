# Aiel.InternetTypes

This project contains types that are widely used on the Internet. 

- `DomainName` - A class that represents a domain name and provides validation and parsing functionality
- `Serial` - A class that represents a serial number and provides validation and parsing functionality
- `TTL` - A class that represents a time-to-live value and provides validation and parsing functionality

## Installation

You can install the Aiel.InternetTypes package via NuGet Package Manager Console:
```powershell
Install-Package Aiel.InternetTypes
```

Or via .NET CLI:

```powershell
dotnet add package Aiel.InternetTypes
```

## Usage

To use the Aiel.InternetTypes package, simply create an instance of the desired type and use
it's methods for validation and parsing. Here's an example of how to use the `Serial` class:

```csharp
[Fact]
public void Must_output_Human_Readable_strings()
{
    var s = Serial.NewSerial(DateTimeOffset.UnixEpoch);
    s.ToFormattedString().Should().Be("1970-01-01-00");
    s = s.Increment(DateTimeOffset.UnixEpoch);
    s.ToFormattedString().Should().Be("1970-01-01-01");
}
```

### Extension Methods

#### IPAddressExtensions

IP address utilities:

```csharp
using Aiel.Internet;
using System.Net;

var ip = IPAddress.Parse("192.168.1.100");
var network = IPAddress.Parse("192.168.1.0");

if (ip.IsInSameSubnet(network, 24))
{
    Console.WriteLine("Same subnet");
}
```

### Comparers

#### IPAddressComparer

Compares IP addresses for sorting:

```csharp
using Aiel.Net;

var addresses = new[]
{
    IPAddress.Parse("192.168.1.100"),
    IPAddress.Parse("10.0.0.1"),
    IPAddress.Parse("192.168.1.10")
};

Array.Sort(addresses, new IPAddressComparer());
```

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE.md) file for details.
