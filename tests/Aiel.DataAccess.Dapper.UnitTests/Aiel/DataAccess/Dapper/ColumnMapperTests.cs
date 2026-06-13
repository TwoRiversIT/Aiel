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
using Microsoft.Data.Sqlite;

namespace Aiel.DataAccess.Dapper;

/// <summary>
/// Tests the <see cref="ColumnMapper"/> functionality using an in-memory SQLite database.
/// </summary>
public class ColumnMapperTests : IAsyncLifetime
{
    private SqliteConnection _connection = null!;

    /// <summary>
    /// Initializes the test by creating an in-memory SQLite database.
    /// </summary>
    public async ValueTask InitializeAsync()
    {
        // Cleanup any existing mappings
        SqlMapper.RemoveTypeMap(typeof(Product));
        SqlMapper.RemoveTypeMap(typeof(ProductWithOptionalFields));

        _connection = new SqliteConnection("Data Source=:memory:");
        await _connection.OpenAsync();

        // Create test table
        await _connection.ExecuteAsync("""
            CREATE TABLE products (
                id INTEGER PRIMARY KEY,
                product_name TEXT NOT NULL,
                unit_price REAL NOT NULL,
                quantity_in_stock INTEGER NOT NULL
            )
            """);
    }

    /// <summary>
    /// Disposes of the database connection.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            _connection.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Tests that the generic <see cref="ColumnMapper.ApplyMap{T}"/> method correctly maps columns to properties.
    /// </summary>
    [Fact]
    public async Task ApplyMap_Generic_ShouldMapColumnsToProperties()
    {
        // Arrange - Apply mapping for Product type
        ColumnMapper.Map<Product>();

        // Insert test data using database column names
        const Int32 productId = 1;
        const String productName = "Widget";
        const Decimal unitPrice = 19.99m;
        const Int32 quantityInStock = 100;

        await _connection.ExecuteAsync("""
            INSERT INTO products (id, product_name, unit_price, quantity_in_stock)
            VALUES (@id, @productName, @unitPrice, @quantityInStock)
            """, new { id = productId, productName, unitPrice, quantityInStock });

        // Act - Query data using Dapper with mapped type
        var products = (await _connection.QueryAsync<Product>(
            "SELECT id, product_name, unit_price, quantity_in_stock FROM products"
        )).ToList();

        // Assert
        products.Should().NotBeNull();
        products.Count.Should().Be(1);
        var product = products[0];
        product.Id.Should().Be(productId);
        product.Name.Should().Be(productName);
        product.Price.Should().Be(unitPrice);
        product.Stock.Should().Be(quantityInStock);
    }

    /// <summary>
    /// Tests that the type-based <see cref="ColumnMapper.MapColumns(Type)"/> extension method correctly maps columns to properties.
    /// </summary>
    [Fact]
    public async Task ApplyMap_ByType_ShouldMapColumnsToProperties()
    {
        // Arrange - Apply mapping for Product type using reflection-based method
        typeof(Product).MapColumns();

        // Insert test data
        const String productName = "Gadget";
        const Decimal unitPrice = 29.99m;
        const Int32 quantityInStock = 50;

        await _connection.ExecuteAsync("""
            INSERT INTO products (product_name, unit_price, quantity_in_stock)
            VALUES (@productName, @unitPrice, @quantityInStock)
            """, new { productName, unitPrice, quantityInStock });

        // Act
        var product = (await _connection.QueryFirstOrDefaultAsync<Product>(
            "SELECT id, product_name, unit_price, quantity_in_stock FROM products"
        ))!;

        // Assert
        product.Should().NotBeNull();
        product.Name.Should().Be(productName);
        product.Price.Should().Be(unitPrice);
        product.Stock.Should().Be(quantityInStock);
    }

    [Fact]
    public async Task MapTypesFromAssemblies_Should_MapTypes()
    {
        // Arrange
        const String productName = "Widget";
        const Decimal unitPrice = 19.99m;
        const Int32 quantityInStock = 100;

        await _connection.ExecuteAsync("""
            INSERT INTO products (product_name, unit_price, quantity_in_stock)
            VALUES (@productName, @unitPrice, @quantityInStock)
            """, new { productName, unitPrice, quantityInStock });

        // Act
        ColumnMapper.MapTypesFromAssemblies(typeof(ProductWithOptionalFields).Assembly);

        // Assert
        var product = await _connection.QueryFirstOrDefaultAsync<Product>(
            "SELECT id, product_name, unit_price, quantity_in_stock FROM products"
        );

        product.Should().NotBeNull();
        product.Name.Should().Be(productName);
        product.Price.Should().Be(unitPrice);
        product.Stock.Should().Be(quantityInStock);
    }

    [Fact]
    public async Task MapTypesFromCallingAssembly_Should_MapTypes()
    {
        // Arrange
        const String productName = "Widget";
        const Decimal unitPrice = 19.99m;
        const Int32 quantityInStock = 100;

        await _connection.ExecuteAsync("""
            INSERT INTO products (product_name, unit_price, quantity_in_stock)
            VALUES (@productName, @unitPrice, @quantityInStock)
            """, new { productName, unitPrice, quantityInStock });

        // Act
        ColumnMapper.MapTypesFromCallingAssembly();

        // Assert
        var product = await _connection.QueryFirstOrDefaultAsync<Product>(
            "SELECT id, product_name, unit_price, quantity_in_stock FROM products"
        );

        product.Should().NotBeNull();
        product.Name.Should().Be(productName);
        product.Price.Should().Be(unitPrice);
        product.Stock.Should().Be(quantityInStock);
    }

    [Fact]
    public async Task MapTypesFromAssemblyContaining_T_Should_MapTypes()
    {
        // Arrange
        const String productName = "Widget";
        const Decimal unitPrice = 19.99m;
        const Int32 quantityInStock = 100;

        await _connection.ExecuteAsync("""
            INSERT INTO products (product_name, unit_price, quantity_in_stock)
            VALUES (@productName, @unitPrice, @quantityInStock)
            """, new { productName, unitPrice, quantityInStock });

        // Act
        // This is a little redundant since it delegates to MapTypesFromAssemblies().
        ColumnMapper.MapTypesFromAssemblyContaining<ProductWithOptionalFields>();

        // Assert
        var product = await _connection.QueryFirstOrDefaultAsync<Product>(
            "SELECT id, product_name, unit_price, quantity_in_stock FROM products"
        );

        product.Should().NotBeNull();
        product.Name.Should().Be(productName);
        product.Price.Should().Be(unitPrice);
        product.Stock.Should().Be(quantityInStock);
    }

    /// <summary>
    /// Tests that column mappings work correctly with both insert and retrieve operations.
    /// </summary>
    [Fact]
    public async Task Mapping_ShouldWorkWithInsertAndRetrieve()
    {
        // Arrange
        ColumnMapper.Map<Product>();

        var originalProduct = new Product
        {
            Name = "Awesome Gadget",
            Price = 49.99m,
            Stock = 75
        };

        // Act - Insert using Dapper
        await _connection.ExecuteAsync("""
            INSERT INTO products (product_name, unit_price, quantity_in_stock)
            VALUES (@Name, @Price, @Stock)
            """, originalProduct);

        // Retrieve and verify
        var retrieved = await _connection.QueryFirstOrDefaultAsync<Product>(
            "SELECT id, product_name, unit_price, quantity_in_stock FROM products WHERE product_name = @Name",
            new { originalProduct.Name }
        );

        // Assert
        retrieved.Should().NotBeNull();
        retrieved.Name.Should().Be(originalProduct.Name);
        retrieved.Price.Should().Be(originalProduct.Price);
        retrieved.Stock.Should().Be(originalProduct.Stock);
    }

    /// <summary>
    /// Test entity with column mappings.
    /// </summary>
    [HasColumnMaps]
    private class Product
    {
        /// <summary>
        /// Gets or sets the product ID.
        /// </summary>
        [ColumnName("id")]
        public Int32 Id { get; set; }

        /// <summary>
        /// Gets or sets the product name, mapped from 'product_name' column.
        /// </summary>
        [ColumnName("product_name")]
        public String Name { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the product price, mapped from 'unit_price' column.
        /// </summary>
        [ColumnName("unit_price")]
        public Decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the quantity in stock, mapped from 'quantity_in_stock' column.
        /// </summary>
        [ColumnName("quantity_in_stock")]
        public Int32 Stock { get; set; }
    }

    /// <summary>
    /// Test entity with optional fields for null handling testing.
    /// </summary>
    private class ProductWithOptionalFields
    {
        /// <summary>
        /// Gets or sets the product ID.
        /// </summary>
        public Int32 Id { get; set; }

        /// <summary>
        /// Gets or sets the product name.
        /// </summary>
        [ColumnName("product_name")]
        public String Name { get; set; } = String.Empty;

        /// <summary>
        /// Gets or sets the product price.
        /// </summary>
        [ColumnName("unit_price")]
        public Decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the quantity in stock.
        /// </summary>
        [ColumnName("quantity_in_stock")]
        public Int32 Stock { get; set; }

        /// <summary>
        /// Gets or sets the optional product description.
        /// </summary>
        [ColumnName("description")]
        public String? Description { get; set; }
    }
}
