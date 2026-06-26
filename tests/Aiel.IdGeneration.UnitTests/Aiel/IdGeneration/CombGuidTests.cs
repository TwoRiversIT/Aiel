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

public class CombGuidFactoryTests
{
    [Fact]
    public void NewGuid_WithSqlServer_GeneratesValidGuid()
    {
        var guid = CombGuid.NewGuid(DatabaseType.SqlServer);

        guid.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void NewGuid_WithPostgreSql_GeneratesValidGuid()
    {
        var guid = CombGuid.NewGuid(DatabaseType.PostgreSql);

        guid.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void NewGuid_WithInvalidDatabaseType_ThrowsArgumentOutOfRangeException()
    {
        Invoking(() => CombGuid.NewGuid((DatabaseType)999)).Should().Throw<ArgumentOutOfRangeException>();
    }
}

public class SqlServerCombGuidTests
{
    [Fact]
    public void NewGuid_GeneratesValidGuid()
    {
        var guid = new SqlServerCombGuid().NewGuid();

        guid.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void NewGuid_GeneratesUniqueGuids()
    {
        var guids = new HashSet<Guid>();

        for (var i = 0; i < 1000; i++)
        {
            var guid = new SqlServerCombGuid().NewGuid();
            guids.Add(guid).Should().BeTrue($"Duplicate GUID generated: {guid}");
        }
    }

    [Fact]
    public void NewGuid_WithinSameMillisecond_GeneratesDifferentGuids()
    {
        var guid1 = new SqlServerCombGuid().NewGuid();
        var guid2 = new SqlServerCombGuid().NewGuid();

        guid1.Should().NotBe(guid2);
    }

    [Fact]
    public void NewGuid_GeneratedGuidsHaveTimestampInLastSixBytes()
    {
        var guid1 = new SqlServerCombGuid().NewGuid();
        Thread.Sleep(10);
        var guid2 = new SqlServerCombGuid().NewGuid();

        var bytes1 = guid1.ToByteArray();
        var bytes2 = guid2.ToByteArray();

        var lastSixBytes1 = bytes1.Skip(10).ToArray();
        var lastSixBytes2 = bytes2.Skip(10).ToArray();

        lastSixBytes1.SequenceEqual(lastSixBytes2).Should().BeFalse("The timestamp portion (last 6 bytes) should differ for SQL Server");
    }
}

public class PostgreSqlCombGuidTests
{
    [Fact]
    public void NewGuid_GeneratesValidGuid()
    {
        var guid = new PostgreSqlCombGuid().NewGuid();

        guid.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void NewGuid_GeneratesUniqueGuids()
    {
        var guids = new HashSet<Guid>();

        for (var i = 0; i < 1000; i++)
        {
            var guid = new PostgreSqlCombGuid().NewGuid();
            guids.Add(guid).Should().BeTrue($"Duplicate GUID generated: {guid}");
        }
    }

    [Fact]
    public void NewGuid_WithinSameMillisecond_GeneratesDifferentGuids()
    {
        var guid1 = new PostgreSqlCombGuid().NewGuid();
        var guid2 = new PostgreSqlCombGuid().NewGuid();

        guid1.Should().NotBe(guid2);
    }

    [Fact]
    public void NewGuid_GeneratedGuidsHaveTimestampInFirstSixBytes()
    {
        var guid1 = new PostgreSqlCombGuid().NewGuid();
        Thread.Sleep(50);
        var guid2 = new PostgreSqlCombGuid().NewGuid();

        var bytes1 = guid1.ToByteArray();
        var bytes2 = guid2.ToByteArray();

        var firstSixBytes1 = bytes1.Take(6).ToArray();
        var firstSixBytes2 = bytes2.Take(6).ToArray();

        firstSixBytes1.SequenceEqual(firstSixBytes2).Should().BeFalse("The timestamp portion (first 6 bytes) should differ for PostgreSQL");
    }
}
