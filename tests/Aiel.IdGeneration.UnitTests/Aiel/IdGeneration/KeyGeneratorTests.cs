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

namespace Aiel.IdGeneration;

public class KeyGeneratorTests : IDisposable
{
    private readonly KeyGenerator _generator = new();

    [Fact]
    public void Generate_CreatesKeyOfCorrectLength()
    {
        var key = _generator.Generate(16);

        Assert.Equal(16, key.Length);
    }

    [Theory]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(32)]
    [InlineData(64)]
    public void Generate_CreatesKeyOfVariousLengths(Int32 length)
    {
        var key = _generator.Generate(length);

        Assert.Equal(length, key.Length);
    }

    [Fact]
    public void Generate_UsesOnlyAllowedCharacters()
    {
        const String allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var key = _generator.Generate(100);

        Assert.All(key, c => Assert.Contains(c, allowedChars));
    }

    [Fact]
    public void Generate_DoesNotContainLowercaseLetters()
    {
        var key = _generator.Generate(100);

        Assert.DoesNotContain(key, c => Char.IsLower(c));
    }

    [Fact]
    public void Generate_CreatesUniqueKeys()
    {
        var keys = new HashSet<String>();
        for (var i = 0; i < 1000; i++)
        {
            var key = _generator.Generate(16);
            Assert.True(keys.Add(key), $"Duplicate key generated: {key}");
        }
    }

    [Fact]
    public void Generate_HandlesMinimumLength()
    {
        var key = _generator.Generate(1);

        Assert.Equal(1, key.Length);
    }

    [Fact]
    public void Generate_CreatesDistributedKeys()
    {
        var characterCounts = new Dictionary<Char, Int32>();
        const Int32 iterations = 10000;
        const Int32 keyLength = 16;

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

        const Double expectedCountPerChar = iterations * keyLength / 36.0;
        const Double tolerance = expectedCountPerChar * 0.5;

        foreach (var count in characterCounts.Values)
        {
            Assert.InRange(count, expectedCountPerChar - tolerance, expectedCountPerChar + tolerance);
        }
    }

    public void Dispose()
    {
        _generator?.Dispose();
        GC.SuppressFinalize(this);
    }
}
