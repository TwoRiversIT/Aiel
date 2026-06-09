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

using Aiel.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Reflection.Emit;

namespace Aiel.EntityFrameworkCore;

public sealed class AielDbContextIssue17ContractTests
{
    private const BindingFlags InstanceMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    [Fact]
    public void Issue17_requires_the_renamed_base_context_without_a_legacy_shim()
    {
        var assembly = typeof(ModelBuilderExtensions).Assembly;
        var renamedType = assembly.GetType("Aiel.EntityFrameworkCore.AielDbContext");
        var legacyType = assembly.GetType("Aiel.EntityFrameworkCore.TrDbContext");

        renamedType.Should().NotBeNull("issue #17 replaces TrDbContext with AielDbContext.");
        legacyType.Should().BeNull("issue #17 called for a clean rename without compatibility shims.");
    }

    [Fact]
    public void Issue17_requires_explicit_tenant_resolution_binding_on_the_base_context()
    {
        var baseContextType = GetRequiredAielDbContextType();
        var constructors = baseContextType.GetConstructors(InstanceMembers);
        var resolutionProperties = baseContextType
            .GetProperties(InstanceMembers | BindingFlags.DeclaredOnly)
            .Where(static property => property.PropertyType == typeof(TenantResolution))
            .ToArray();

        constructors.Should().Contain(
            static constructor => HasConstructorSignature(constructor, typeof(DbContextOptions), typeof(ITenantResolver)),
            "issue #17 needs an explicit TenantResolution-producing dependency.");
        constructors.Should().NotContain(
            static constructor => HasConstructorSignature(constructor, typeof(DbContextOptions), typeof(ITenantAccessor)),
            "issue #17 moves the EF contract away from the legacy accessor-only shape.");
        baseContextType.GetProperty("HasTenantContext", InstanceMembers).Should().BeNull();
        baseContextType.GetProperty("CurrentTenantIdOrDefault", InstanceMembers).Should().BeNull();
        resolutionProperties.Should().ContainSingle("issue #17 needs one explicit resolution state instead of nullable tenant escape hatches.");
    }

    [Fact]
    public async Task Issue17_stamps_new_entities_when_the_tenant_resolution_is_resolved()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var tenantId = new TenantId(Guid.NewGuid());

        await using var dbContext = CreateResolverBackedDbContext(
            Guid.NewGuid().ToString("N"),
            new TenantResolution.Resolved(new TenantIdentity(tenantId)));

        var entity = new Issue17TenantScopedNote { Id = Guid.NewGuid(), Name = "alpha" };

        dbContext.Set<Issue17TenantScopedNote>().Add(entity);

        await dbContext.SaveChangesAsync(cancellationToken);

        entity.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task Issue17_keeps_query_results_isolated_between_resolved_tenants_without_cross_tenant_leakage()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var databaseName = Guid.NewGuid().ToString("N");
        var firstTenantId = new TenantId(Guid.NewGuid());
        var secondTenantId = new TenantId(Guid.NewGuid());

        await SeedAsync(
            databaseName,
            new Issue17TenantScopedNote { Id = Guid.NewGuid(), TenantId = firstTenantId, Name = "first" },
            new Issue17TenantScopedNote { Id = Guid.NewGuid(), TenantId = secondTenantId, Name = "second" });

        await using var firstTenantContext = CreateResolverBackedDbContext(
            databaseName,
            new TenantResolution.Resolved(new TenantIdentity(firstTenantId)));
        await using var secondTenantContext = CreateResolverBackedDbContext(
            databaseName,
            new TenantResolution.Resolved(new TenantIdentity(secondTenantId)));

        var firstTenantResults = await firstTenantContext
            .Set<Issue17TenantScopedNote>()
            .OrderBy(static note => note.Name)
            .Select(static note => note.Name)
            .ToListAsync(cancellationToken);
        var secondTenantResults = await secondTenantContext
            .Set<Issue17TenantScopedNote>()
            .OrderBy(static note => note.Name)
            .Select(static note => note.Name)
            .ToListAsync(cancellationToken);

        firstTenantResults.Should().Equal("first");
        secondTenantResults.Should().Equal("second");
    }

    [Fact]
    public async Task Issue17_fails_closed_for_missing_tenant_resolution()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var databaseName = Guid.NewGuid().ToString("N");

        await SeedAsync(
            databaseName,
            new Issue17TenantScopedNote
            {
                Id = Guid.NewGuid(),
                TenantId = new TenantId(Guid.NewGuid()),
                Name = "seed"
            });

        await using var dbContext = CreateResolverBackedDbContext(databaseName, new TenantResolution.Missing());

        var results = await dbContext.Set<Issue17TenantScopedNote>().ToListAsync(cancellationToken);
        var tenantResolution = GetCurrentTenantResolution(dbContext);

        results.Should().BeEmpty("tenant-scoped queries must fail closed when no tenant is resolved.");
        tenantResolution.Should().BeOfType<TenantResolution.Missing>();
    }

    [Fact]
    public async Task Issue17_fails_closed_for_error_tenant_resolution()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var databaseName = Guid.NewGuid().ToString("N");

        await SeedAsync(
            databaseName,
            new Issue17TenantScopedNote
            {
                Id = Guid.NewGuid(),
                TenantId = new TenantId(Guid.NewGuid()),
                Name = "seed"
            });

        await using var dbContext = CreateResolverBackedDbContext(
            databaseName,
            new TenantResolution.Error(TenantResolutionErrorReason.MembershipLookupFailed));

        var results = await dbContext.Set<Issue17TenantScopedNote>().ToListAsync(cancellationToken);
        var tenantResolution = GetCurrentTenantResolution(dbContext);

        results.Should().BeEmpty("tenant-scoped queries must fail closed when tenant resolution reports an error.");
        tenantResolution.Should().BeOfType<TenantResolution.Error>();
        tenantResolution.As<TenantResolution.Error>().Reason.Should().Be(TenantResolutionErrorReason.MembershipLookupFailed);
    }

    private static async Task SeedAsync(String databaseName, params Issue17TenantScopedNote[] entities)
    {
        await using var seedContext = new Issue17SeedDbContext(CreateSeedOptions(databaseName));

        await seedContext.Database.EnsureCreatedAsync();
        await seedContext.Notes.AddRangeAsync(entities);
        await seedContext.SaveChangesAsync();
    }

    private static DbContext CreateResolverBackedDbContext(String databaseName, TenantResolution tenantResolution)
    {
        var baseContextType = GetRequiredAielDbContextType();
        var constructor = baseContextType
            .GetConstructors(InstanceMembers)
            .SingleOrDefault(static value => HasConstructorSignature(value, typeof(DbContextOptions), typeof(ITenantResolver)));

        constructor.Should().NotBeNull("the renamed base context must accept ITenantResolver so EF can consume explicit outcomes.");

        var derivedType = BuildRuntimeDerivedContextType(baseContextType, constructor!);
        var options = new DbContextOptionsBuilder()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return (DbContext)Activator.CreateInstance(
            derivedType,
            options,
            new StubTenantResolver(tenantResolution))!;
    }

    private static DbContextOptions<Issue17SeedDbContext> CreateSeedOptions(String databaseName)
        => new DbContextOptionsBuilder<Issue17SeedDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

    private static TenantResolution GetCurrentTenantResolution(DbContext dbContext)
    {
        var resolutionProperty = dbContext.GetType()
            .BaseType!
            .GetProperties(InstanceMembers | BindingFlags.DeclaredOnly)
            .SingleOrDefault(static property => property.PropertyType == typeof(TenantResolution));

        resolutionProperty.Should().NotBeNull("issue #17 needs the current resolution outcome to remain inspectable.");

        return (TenantResolution)resolutionProperty!.GetValue(dbContext)!;
    }

    private static Type BuildRuntimeDerivedContextType(Type baseContextType, ConstructorInfo baseConstructor)
    {
        var assemblyName = new AssemblyName("Aiel.EntityFrameworkCore.Issue17.Generated." + Guid.NewGuid().ToString("N"));
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name!);
        var typeBuilder = moduleBuilder.DefineType(
            "Issue17GeneratedContext_" + Guid.NewGuid().ToString("N"),
            TypeAttributes.Public | TypeAttributes.Sealed,
            baseContextType);

        BuildConstructor(typeBuilder, baseConstructor);
        BuildOnModelCreatingOverride(typeBuilder, baseContextType);

        return typeBuilder.CreateType()!;
    }

    private static void BuildConstructor(TypeBuilder typeBuilder, ConstructorInfo baseConstructor)
    {
        var constructorBuilder = typeBuilder.DefineConstructor(
            MethodAttributes.Public,
            CallingConventions.Standard,
            [typeof(DbContextOptions), typeof(ITenantResolver)]);
        var il = constructorBuilder.GetILGenerator();

        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Call, baseConstructor);
        il.Emit(OpCodes.Ret);
    }

    private static void BuildOnModelCreatingOverride(TypeBuilder typeBuilder, Type baseContextType)
    {
        var methodBuilder = typeBuilder.DefineMethod(
            "OnModelCreating",
            MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Virtual,
            typeof(void),
            [typeof(ModelBuilder)]);
        var entityMethod = typeof(ModelBuilder)
            .GetMethods()
            .Single(static method =>
                method.Name == nameof(ModelBuilder.Entity)
                && method.IsGenericMethodDefinition
                && method.GetGenericArguments().Length == 1
                && method.GetParameters().Length == 0)
            .MakeGenericMethod(typeof(Issue17TenantScopedNote));
        var baseOnModelCreating = baseContextType.GetMethod(
            "OnModelCreating",
            InstanceMembers,
            binder: null,
            types: [typeof(ModelBuilder)],
            modifiers: null);
        var il = methodBuilder.GetILGenerator();

        baseOnModelCreating.Should().NotBeNull();

        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Callvirt, entityMethod);
        il.Emit(OpCodes.Pop);
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Call, baseOnModelCreating!);
        il.Emit(OpCodes.Ret);

        typeBuilder.DefineMethodOverride(methodBuilder, baseOnModelCreating!);
    }

    private static Type GetRequiredAielDbContextType()
    {
        var type = typeof(ModelBuilderExtensions).Assembly.GetType("Aiel.EntityFrameworkCore.AielDbContext");

        type.Should().NotBeNull("issue #17 is specifically about renaming the base context to AielDbContext.");

        return type!;
    }

    private static Boolean HasConstructorSignature(ConstructorInfo constructor, params Type[] parameterTypes)
        => constructor.GetParameters().Select(static parameter => parameter.ParameterType).SequenceEqual(parameterTypes);

    private sealed class Issue17SeedDbContext(DbContextOptions<Issue17SeedDbContext> options) : DbContext(options)
    {
        public DbSet<Issue17TenantScopedNote> Notes => Set<Issue17TenantScopedNote>();
    }

    public sealed class Issue17TenantScopedNote : IMultiTenant
    {
        public Guid Id { get; set; }

        public TenantId TenantId { get; set; }

        public String Name { get; set; } = String.Empty;
    }

    private sealed class StubTenantResolver(TenantResolution tenantResolution) : ITenantResolver
    {
        public ValueTask<TenantResolution> ResolveAsync(CancellationToken cancellationToken = default)
            => ValueTask.FromResult(tenantResolution);
    }
}
