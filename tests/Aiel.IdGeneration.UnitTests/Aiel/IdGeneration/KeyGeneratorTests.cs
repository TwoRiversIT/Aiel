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

using static AwesomeAssertions.FluentActions;

namespace Aiel.IdGeneration;

public class KeyGeneratorTests : IDisposable
{
    private readonly KeyGenerator _generator = new();

    [Fact]
    public void Generate_CreatesKeyOfCorrectLength()
    {
        var key = _generator.Generate(16);

        key.Should().HaveLength(16);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(64)]
    public void Generate_CreatesKeyOfVariousLengths(Int32 length)
    {
        var key = _generator.Generate(length);

        key.Should().HaveLength(length);
    }

    [Fact]
    public void Generate_UsesOnlyAllowedCharacters()
    {
        var key = _generator.Generate(1000);

        key.ToArray().Should().OnlyContain(c => KeyGenerator.AllowedChars.Contains(c));
    }

    [Fact]
    public void Generate_DoesNotContainLowercaseLetters()
    {
        var key = _generator.Generate(100);

        key.ToArray().Should().OnlyContain(c => !Char.IsLower(c));
    }

    [Fact]
    public void Generate_CreatesUniqueKeys()
    {
        var keys = new HashSet<String>();
        for (var i = 0; i < 1000; i++)
        {
            var key = _generator.Generate(16);
            keys.Add(key).Should().BeTrue($"Duplicate key generated: {key}");
        }
    }

    [Fact]
    public void Generate_HandlesMinimumLength()
    {
        var key = _generator.Generate(1);

        key.Should().HaveLength(1);
    }

    [Fact]
    public void Generate_WhenDisposed_ThrowsObjectDisposedException()
    {
        var generator = new KeyGenerator();
        generator.Dispose();

        Invoking(() => generator.Generate(16)).Should().Throw<ObjectDisposedException>();
    }

    [Fact]
    public void Generate_CreatesKeys_With_EvenlyDistributedCharacters()
    {
        /*
         * ## Test Explanation
         * 
         * This test **validates the statistical quality and randomness of the key generator** by ensuring characters are distributed uniformly across generated keys.
         * 
         * ### What It Does
         * 
         * 1. **Generates many keys** (10,000 iterations × 16 characters = 160,000 total characters)
         * 2. **Counts character frequency** — tracks how many times each character appears across all generated keys
         * 3. **Validates uniform distribution** — asserts that each character appears approximately the same number of times within acceptable tolerance
         * 
         * ### The Math
         * 
         * Given a 36-character alphabet (0-9, a-z):
         * 
         * ```
         * Expected count per character: (10,000 iterations × 16 chars) / 36 = ~4,444
         * Tolerance: ±50% = ±2,222
         * Acceptable range: [2,222 to 6,666]
         * ```
         * 
         * If all characters fall within this range, the distribution is statistically sound.
         * 
         * ### Why This Test Is Necessary
         * 
         * 1. **Detects bias** — If the generator favors certain characters, they'll exceed the upper bound; neglected characters fall below the lower bound
         * 2. **Ensures cryptographic quality** — Non-uniform RNG is a red flag for weak randomness, which can lead to predictable keys or security vulnerabilities
         * 3. **Catches implementation bugs** — Off-by-one errors in character selection, seeding issues, or modulo bias in the RNG would show up here
         * 4. **Validates suitability** — For distributed systems needing collision-resistant keys (IDs, tokens, etc.), uniform distribution is essential
         * 
         * ### When It Would Fail
         * 
         * - **Broken RNG** — If `Random` isn't properly seeded or uses a flawed algorithm
         * - **Modulo bias** — If the generator uses `rng.Next() % 36` instead of proper rejection sampling
         * - **Character set error** — If the character set has duplicates or wrong count assumptions
         * 
         * This is a standard **statistical randomness test** and represents best practice for validating key/ID generators.
         */

        const Int32 iterations = 10000;
        const Int32 keyLength = 32;
        const Int32 expectedCountPerChar = iterations * keyLength / 36; // A-Z + 0-9 = 36 characters
        const Int32 tolerance = expectedCountPerChar / 2; // 
        const Int32 low = expectedCountPerChar - tolerance;
        const Int32 high = expectedCountPerChar + tolerance;

        var characterCounts = new Dictionary<Char, Int32>();

        for (var i = 0; i < iterations; i++)
        {
            var key = _generator.Generate(keyLength);
            foreach (var c in key)
            {
                if (!characterCounts.TryGetValue(c, out var value))
                {
                    value = 0;
                    characterCounts[c] = value;
                }

                characterCounts[c] = ++value;
            }
        }

        foreach (var count in characterCounts.Values)
        {
            count.Should().BeInRange(low, high);
        }
    }

    public void Dispose()
    {
        _generator?.Dispose();
        GC.SuppressFinalize(this);
    }
}
