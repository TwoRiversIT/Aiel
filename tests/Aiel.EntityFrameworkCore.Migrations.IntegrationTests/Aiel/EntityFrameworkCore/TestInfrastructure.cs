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

using Aiel.Domain.EntityFrameworkCore;
using Aiel.EntityFrameworkCore.Migrations;
using Aiel.Framework.DependencyInjection;
using Aiel.StrongIds;
using Aiel.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace Aiel.EntityFrameworkCore;

public class TestDbContext(DbContextOptions<TestDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers { get; init; } = default!;
    public DbSet<Person> People { get; set; } = default!;
    public DbSet<TypicalClass> TypicalClasses { get; init; } = default!;

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.ConfigureValueConverters();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Person>();
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasStrongIdConversion<PersonId, Guid>();
        builder.Property(x => x.FirstName).IsRequired();
        builder.Property(x => x.LastName).IsRequired();

        modelBuilder.ApplyConfiguration(new PersonConfiguration());
        modelBuilder.ApplyConfiguration(new TypicalClassConfiguration());
    }
}

public class MigrationsFixture
    : ConfiguratorTestFixture<AielEntityFrameworkCoreMigrationsIntegrationTests>
{
    private const String PostgreSqlImage = "postgres:15-alpine";
    private const String PostgreSqlUserName = "postgres";
    private const String PostgreSqlPassword = "postgres";
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder(PostgreSqlImage)
        //.WithCleanUp(false)
        .WithDatabase($"MigrationsTestDb_{Guid.NewGuid():N}")
        .WithUsername(PostgreSqlUserName)
        .WithPassword(PostgreSqlPassword)
        //.WithPortBinding(5432, 5432)
        .Build();

    public String ConnectionString { get; private set; } = String.Empty;

    public override ValueTask ConfigureAsync(ConfigurationContext context, CancellationToken cancellationToken = default)
    {
        context.Services.AddTransient<TestDbContextMigrator>();

        context.Services.AddDbContext<TestDbContext>(options =>
        {
            options.UseNpgsql(ConnectionString, config =>
            {
                config.MigrationsAssembly(typeof(TestDbContext).Assembly.FullName);
            });
        });

        return ValueTask.CompletedTask;
    }

    public override async ValueTask InitializeAsync(InitializationContext context, CancellationToken cancellationToken = default)
    {
        await _postgresContainer.StartAsync(cancellationToken);
        ConnectionString = _postgresContainer.GetConnectionString();
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        await _postgresContainer.DisposeAsync();
        await base.DisposeAsyncCore();
    }
}

public class MigrationsTestBase(MigrationsFixture fixture, ITestOutputHelper output)
    : ConfiguratorTestBase<AielEntityFrameworkCoreMigrationsIntegrationTests, MigrationsFixture>(fixture, output)
{
}

public class SutMigrationsFixture<TSut>
    : SystemUnderTestConfiguratorTestFixture<AielEntityFrameworkCoreMigrationsIntegrationTests, TSut>
    where TSut : class
{
    private const String PostgreSqlImage = "postgres:15-alpine";
    private const String PostgreSqlUserName = "postgres";
    private const String PostgreSqlPassword = "postgres";
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder(PostgreSqlImage)
        //.WithCleanUp(false)
        .WithDatabase($"MigrationsTestDb_{Guid.NewGuid():N}")
        .WithUsername(PostgreSqlUserName)
        .WithPassword(PostgreSqlPassword)
        //.WithPortBinding(5432, 5432)
        .Build();

    public String ConnectionString { get; private set; } = String.Empty;

    public override ValueTask ConfigureAsync(ConfigurationContext context, CancellationToken cancellationToken = default)
    {
        context.Services.AddTransient<TestDbContextMigrator>();

        context.Services.AddDbContext<TestDbContext>(options =>
        {
            options.UseNpgsql(ConnectionString, config =>
            {
                config.MigrationsAssembly(typeof(TestDbContext).Assembly.FullName);
            });
        });

        return ValueTask.CompletedTask;
    }

    public override async ValueTask InitializeAsync(InitializationContext context, CancellationToken cancellationToken = default)
    {
        await _postgresContainer.StartAsync(cancellationToken);
        ConnectionString = _postgresContainer.GetConnectionString();
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        await _postgresContainer.DisposeAsync();
        await base.DisposeAsyncCore();
    }
}

public class MigrationsSutTestBase<TSut>(SutMigrationsFixture<TSut> fixture, ITestOutputHelper output)
    : SystemUnderTestConfiguratorTestBase<AielEntityFrameworkCoreMigrationsIntegrationTests, SutMigrationsFixture<TSut>, TSut>(fixture, output)
    where TSut : class
{
}

public class TestDbContextMigrator(IServiceProvider serviceProvider)
    : DbContextMigrator<TestDbContext>(serviceProvider)
{
}

public class TestDbContextFactory : IDesignTimeDbContextFactory<TestDbContext>
{
    public TestDbContext CreateDbContext(String[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("MAILMULE_DB_CONNECTION")
            ?? "Host=localhost;Port=5432;Database=MailMule;Schema=public;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<TestDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new TestDbContext(optionsBuilder.Options);
    }
}
