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

using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Aiel.Dapper;
using Aiel.Testing;

namespace Aiel.Data.Dapper;

public class ColumnMappingTestFixture : IntegrationTestFixture
{
    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDbConnectionFactory, TestSqliteConnectionFactory>();
    }

    /// <summary>
    /// Initializes the test by creating an in-memory SQLite database.
    /// </summary>
    protected override async ValueTask InitializeFixtureAsync(IServiceProvider services)
    {
        var connection = await services.GetRequiredService<IDbConnectionFactory>().CreateConnectionAsync();

        ColumnMapper.MapTypesFromAssemblyContaining<Customer>();

        // Create test customers table
        await connection.ExecuteAsync("""
            CREATE TABLE customers (
                customer_id INTEGER PRIMARY KEY,
                full_name TEXT NOT NULL,
                email_address TEXT NOT NULL
            )
            """);

        // Create order table
        await connection.ExecuteAsync("""
            CREATE TABLE orders (
                order_id INTEGER PRIMARY KEY,
                customer_id INTEGER NOT NULL,
                order_total REAL NOT NULL
            )
            """);

        // Insert test data for both tables
        await connection.ExecuteAsync("""
            INSERT INTO customers (customer_id, full_name, email_address)
            VALUES (@Id, @Name, @Email)
            """, new { Id = 1, Name = "Test Customer", Email = "test@example.com" });

        await connection.ExecuteAsync("""
            INSERT INTO orders (order_id, customer_id, order_total)
            VALUES (@Id, @CustomerId, @Total)
            """, new { Id = 1, CustomerId = 1, Total = 99.99m });
    }
}
