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

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Aiel.EntityFrameworkCore.Migrations;

public class MigrationTests(SutMigrationsFixture<IMigrationManager> fixture, ITestOutputHelper output)
    : MigrationsSutTestBase<IMigrationManager>(fixture, output)
{
    const String Query = """
        SELECT * FROM public."__EFMigrationsHistory"
        ORDER BY "MigrationId" ASC 
        """;

    record MigrationHistory(String MigrationId, String ProductVersion);

    [Fact]
    public async Task MigrateAsync_Succeeds()
    {
        // Act
        await SUT.MigrateAsync(CancellationToken);

        var dbContext = Services.GetRequiredService<TestDbContext>();
        var result = await dbContext.Database
            .SqlQueryRaw<MigrationHistory>(Query)
            .SingleOrDefaultAsync(CancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.MigrationId.Should().Be("00000000000000_CreateDatabase");
    }
}
