// MIT License
//
// Copyright 2026 Two Rivers Information Technology Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sub-license,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System.Collections.Concurrent;
using System.Text;

namespace Aiel.Gps.HP;

/// <summary>
/// Registry for custom NMEA parsers that can be registered at runtime.
/// </summary>
/// <remarks>
/// <para>
/// This class allows you to register custom parsers for NMEA sentence types that are
/// not included in the source-generated discriminated union. Custom parsers return
/// boxed objects, which means each parse operation allocates.
/// </para>
/// <para>
/// For high-throughput scenarios, consider adding your message types to the source-generated
/// union using <see cref="NmeaMessageAttribute"/> instead.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var registry = new NmeaParserRegistry();
/// registry.Register(new MyCustomParser());
///
/// var reader = new NmeaBatchReader(stream, registry: registry);
/// await foreach (var message in reader.ReadAsync())
/// {
///     // Built-in messages
/// }
/// await foreach (var custom in reader.ReadCustomMessagesAsync())
/// {
///     // Custom messages (boxed)
/// }
/// </code>
/// </example>
public sealed class NmeaParserRegistry
{
    private readonly ConcurrentDictionary<String, ICustomNmeaParser> _parsers = new();

    /// <summary>
    /// Registers a custom parser with the registry.
    /// </summary>
    /// <param name="parser">The parser to register.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="parser"/> is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if a parser for the same identifier is already registered.</exception>
    public void Register(ICustomNmeaParser parser)
    {
        ArgumentNullException.ThrowIfNull(parser);

        var identifier = Encoding.UTF8.GetString(parser.Identifier);

        if (!_parsers.TryAdd(identifier, parser))
        {
            throw new InvalidOperationException(
                $"A parser for identifier '{identifier}' is already registered. " +
                "Call Unregister() first if you want to replace it.");
        }
    }

    /// <summary>
    /// Unregisters a parser for the specified identifier.
    /// </summary>
    /// <param name="identifier">The sentence identifier to unregister.</param>
    /// <returns>True if a parser was removed; otherwise, false.</returns>
    public Boolean Unregister(ReadOnlySpan<Byte> identifier)
    {
        var key = Encoding.UTF8.GetString(identifier);
        return _parsers.TryRemove(key, out _);
    }

    /// <summary>
    /// Unregisters a parser for the specified identifier.
    /// </summary>
    /// <param name="identifier">The sentence identifier to unregister.</param>
    /// <returns>True if a parser was removed; otherwise, false.</returns>
    public Boolean Unregister(String identifier)
    {
        return _parsers.TryRemove(identifier, out _);
    }

    /// <summary>
    /// Tries to get a parser for the specified identifier.
    /// </summary>
    /// <param name="identifier">The sentence identifier to look up.</param>
    /// <param name="parser">When this method returns true, contains the parser.</param>
    /// <returns>True if a parser was found; otherwise, false.</returns>
    public Boolean TryGetParser(ReadOnlySpan<Byte> identifier, out ICustomNmeaParser? parser)
    {
        var key = Encoding.UTF8.GetString(identifier);
        return _parsers.TryGetValue(key, out parser);
    }

    /// <summary>
    /// Tries to get a parser for the specified identifier.
    /// </summary>
    /// <param name="identifier">The sentence identifier to look up.</param>
    /// <param name="parser">When this method returns true, contains the parser.</param>
    /// <returns>True if a parser was found; otherwise, false.</returns>
    public Boolean TryGetParser(String identifier, out ICustomNmeaParser? parser)
    {
        return _parsers.TryGetValue(identifier, out parser);
    }

    /// <summary>
    /// Gets an enumerable of all registered identifiers.
    /// </summary>
    public IEnumerable<String> RegisteredIdentifiers => _parsers.Keys;

    /// <summary>
    /// Gets the number of registered parsers.
    /// </summary>
    public Int32 Count => _parsers.Count;

    /// <summary>
    /// Removes all registered parsers.
    /// </summary>
    public void Clear() => _parsers.Clear();
}
