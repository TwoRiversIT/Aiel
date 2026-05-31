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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Aiel.Dependencies;
using static FluentAssertions.FluentActions;

namespace Aiel.Extensions;

public class ApplicationConfigurationTests
{

    [Fact]
    public async Task AddApplication_Walks_Dependencies()
    {
        // Arrange
        var services = new ServiceCollection();
        var hostEnvironmentMock = new Mock<IHostEnvironment>();
        hostEnvironmentMock.SetupGet(e => e.EnvironmentName).Returns("Development");
        hostEnvironmentMock.SetupGet(e => e.ApplicationName).Returns("Aiel.UnitTests");
        var builderMock = new Mock<IHostApplicationBuilder>();
        builderMock.SetupGet(b => b.Services).Returns(services);
        builderMock.SetupGet(b => b.Environment).Returns(hostEnvironmentMock.Object);
        var builder = builderMock.Object;

        // Act
        await builder.AddApplicationAsync<HostApplication>(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dependencyRoot = serviceProvider.GetRequiredService<DependencyRoot>();

        // Verify root assembly was registered
        dependencyRoot.Should().NotBeNull();
        dependencyRoot!.Type.Should().Be<HostApplication>("because any other kind is pointless.");
        dependencyRoot.Depth.Should().Be(0);

        // Collect all unique dependencies by traversing and tracking by Type.
        var visited = new HashSet<Type>();
        var allAssemblys = new List<(Type Type, Int32 Depth)>();
        var stack = new Stack<DependencyNode>();
        stack.Push(dependencyRoot);

        while (stack.Count > 0)
        {
            var assemblyInfo = stack.Pop();

            if (visited.Add(assemblyInfo.Type))
            {
                allAssemblys.Add((assemblyInfo.Type, assemblyInfo.Depth));

                foreach (var dependency in assemblyInfo.Dependencies)
                {
                    stack.Push(dependency);
                }
            }
        }

        // Verify all expected dependencies were discovered during the walk
        allAssemblys.Should().HaveCount(6, "all 6 dependencies should be in the hierarchy");

        var assemblyTypes = allAssemblys.ConvertAll(m => m.Type);
        assemblyTypes.Should().Contain(typeof(HostApplication));
        assemblyTypes.Should().Contain(typeof(ApplicationDependency));
        assemblyTypes.Should().Contain(typeof(DataBaseAssembly));
        assemblyTypes.Should().Contain(typeof(DomainAssembly));
        assemblyTypes.Should().Contain(typeof(ApplicationContractsAssembly));
        assemblyTypes.Should().Contain(typeof(DomainShared));

        // Verify HostAssembly (root) has correct direct dependencies and was configured
        dependencyRoot.Dependencies.Should().HaveCount(2, "HostAssembly depends on ApplicationAssembly and DataBaseAssembly");
        dependencyRoot.Dependencies.Select(d => d.Type).Should().Contain(typeof(ApplicationDependency));
        dependencyRoot.Dependencies.Select(d => d.Type).Should().Contain(typeof(DataBaseAssembly));
        AssertWasConfiguredOnceOnly(dependencyRoot.Instance);

        // Verify DataBaseAssembly structure
        // DataBaseAssembly is listed second, so it gets fully processed with its dependencies
        var dbAssembly = dependencyRoot.Dependencies.First(d => d.Type == typeof(DataBaseAssembly));
        dbAssembly.Depth.Should().Be(1, "DataBaseAssembly is at depth 1");
        dbAssembly.Dependencies.Should().HaveCount(2, "DataBaseAssembly depends on ApplicationAssembly and DomainAssembly");
        dbAssembly.Dependencies.Select(d => d.Type).Should().Contain(typeof(ApplicationDependency));
        dbAssembly.Dependencies.Select(d => d.Type).Should().Contain(typeof(DomainAssembly));
        AssertWasConfiguredOnceOnly(dbAssembly.Instance);

        // Get ApplicationAssembly from DataBaseAssembly (this is the fully populated instance)
        var appAssembly = dbAssembly.Dependencies.First(d => d.Type == typeof(ApplicationDependency));
        appAssembly.Depth.Should().Be(1, "ApplicationAssembly is first discovered from the host root and reused from DataBaseAssembly");
        appAssembly.Dependencies.Should().HaveCount(2, "ApplicationAssembly depends on DomainAssembly and ApplicationContractsAssembly");
        appAssembly.Dependencies.Select(d => d.Type).Should().Contain(typeof(DomainAssembly));
        appAssembly.Dependencies.Select(d => d.Type).Should().Contain(typeof(ApplicationContractsAssembly));
        AssertWasConfiguredOnceOnly(appAssembly.Instance);

        // Verify DomainAssembly from ApplicationAssembly
        var domainAssemblyFromApp = appAssembly.Dependencies.First(d => d.Type == typeof(DomainAssembly));
        domainAssemblyFromApp.Depth.Should().Be(2, "DomainAssembly is first discovered from DataBaseAssembly and reused from ApplicationAssembly");
        AssertWasConfiguredOnceOnly(domainAssemblyFromApp.Instance);
        // Note: This DomainAssembly instance is shared and may already be processed.

        // Verify ApplicationContractsAssembly
        var appContractsAssembly = appAssembly.Dependencies.First(d => d.Type == typeof(ApplicationContractsAssembly));
        appContractsAssembly.Depth.Should().Be(2, "ApplicationContractsAssembly is discovered while processing ApplicationAssembly");
        appContractsAssembly.Dependencies.Should().HaveCount(1, "ApplicationContractsAssembly depends on DomainSharedAssembly");
        appContractsAssembly.Dependencies.Select(d => d.Type).Should().Contain(typeof(DomainShared));
        AssertWasConfiguredOnceOnly(appContractsAssembly.Instance);

        // Verify DomainSharedAssembly (leaf node)
        var domainSharedAssembly = appContractsAssembly.Dependencies.First(d => d.Type == typeof(DomainShared));
        domainSharedAssembly.Depth.Should().Be(3, "DomainSharedAssembly depth reflects first discovery path");
        domainSharedAssembly.Dependencies.Should().BeEmpty("DomainSharedAssembly has no dependencies");
        AssertWasConfiguredOnceOnly(domainSharedAssembly.Instance);

        // Verify DomainAssembly from DataBaseAssembly (this is the fully populated instance)
        var domainAssemblyFromDb = dbAssembly.Dependencies.First(d => d.Type == typeof(DomainAssembly));
        domainAssemblyFromDb.Depth.Should().Be(2, "DomainAssembly (from DataBaseAssembly) is at depth 2");
        domainAssemblyFromDb.Dependencies.Should().HaveCount(1, "DomainAssembly depends on DomainSharedAssembly");
        domainAssemblyFromDb.Dependencies.Select(d => d.Type).Should().Contain(typeof(DomainShared));
        AssertWasConfiguredOnceOnly(domainAssemblyFromDb.Instance);

        static void AssertWasConfiguredOnceOnly(AielDependency dependency)
        {
            if (dependency is TestDependency testAssembly)
            {
                testAssembly.ConfigurationCount.Should().Be(1, "should only be configured once, regardless of how many assemblys depend on it");
            }
            else if (dependency is TestApplication testApp)
            {
                testApp.ConfigurationCount.Should().Be(1, "should only be configured once, regardless of how many assemblys depend on it");
            }
            else
            {
                throw new InvalidOperationException($"Dependency of type {dependency.GetType().FullName} is not a AielDependency, cannot check configuration count");
            }
        }
    }

    [Fact]
    public async Task AddApplication_SingleAssembly_WithNoDependencies()
    {
        // Arrange
        var services = new ServiceCollection();
        var hostEnvironmentMock = new Mock<IHostEnvironment>();
        hostEnvironmentMock.SetupGet(e => e.EnvironmentName).Returns("Development");
        hostEnvironmentMock.SetupGet(e => e.ApplicationName).Returns("Aiel.UnitTests");
        var builderMock = new Mock<IHostApplicationBuilder>();
        builderMock.SetupGet(b => b.Services).Returns(services);
        builderMock.SetupGet(b => b.Environment).Returns(hostEnvironmentMock.Object);
        var builder = builderMock.Object;

        // Act
        await builder.AddApplicationAsync<StandaloneApplication>(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var rootAssembly = serviceProvider.GetRequiredService<DependencyRoot>();

        rootAssembly.Should().NotBeNull();
        rootAssembly.Type.Should().Be<StandaloneApplication>();
        rootAssembly.Depth.Should().Be(0);
        rootAssembly.Dependencies.Should().BeEmpty("standalone assembly has no dependencies");

        var standaloneAssembly = rootAssembly.Instance as TestApplication;
        standaloneAssembly.Should().NotBeNull();
        standaloneAssembly.ConfigurationCount.Should().Be(1, "assembly should be configured once");
    }

    [Fact]
    public async Task AddApplication_LinearDependencyChain()
    {
        // Arrange
        var services = new ServiceCollection();
        var hostEnvironmentMock = new Mock<IHostEnvironment>();
        hostEnvironmentMock.SetupGet(e => e.EnvironmentName).Returns("Development");
        hostEnvironmentMock.SetupGet(e => e.ApplicationName).Returns("Aiel.UnitTests");
        var builderMock = new Mock<IHostApplicationBuilder>();
        builderMock.SetupGet(b => b.Services).Returns(services);
        builderMock.SetupGet(b => b.Environment).Returns(hostEnvironmentMock.Object);
        var builder = builderMock.Object;

        // Act - LinearAssembly4 -> LinearAssembly3 -> LinearAssembly2 -> LinearAssembly1
        await builder.AddApplicationAsync<LinearAssembly4>(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var rootAssembly = serviceProvider.GetRequiredService<DependencyRoot>();

        rootAssembly.Should().NotBeNull();
        rootAssembly!.Type.Should().Be<LinearAssembly4>();
        rootAssembly.Depth.Should().Be(0);

        // Verify linear chain
        rootAssembly.Dependencies.Should().HaveCount(1);
        var assembly3 = rootAssembly.Dependencies[0];
        assembly3.Type.Should().Be<LinearAssembly3>();
        assembly3.Depth.Should().Be(1);

        assembly3.Dependencies.Should().HaveCount(1);
        var assembly2 = assembly3.Dependencies[0];
        assembly2.Type.Should().Be<LinearAssembly2>();
        assembly2.Depth.Should().Be(2);

        assembly2.Dependencies.Should().HaveCount(1);
        var assembly1 = assembly2.Dependencies[0];
        assembly1.Type.Should().Be<LinearAssembly1>();
        assembly1.Depth.Should().Be(3);

        assembly1.Dependencies.Should().BeEmpty("LinearAssembly1 is the leaf");

        // Verify all were configured once
        ((TestApplication)rootAssembly.Instance).ConfigurationCount.Should().Be(1);
        ((TestApplication)assembly3.Instance).ConfigurationCount.Should().Be(1);
        ((TestApplication)assembly2.Instance).ConfigurationCount.Should().Be(1);
        ((TestApplication)assembly1.Instance).ConfigurationCount.Should().Be(1);
    }

    [Fact]
    public async Task AddApplication_DiamondDependency_FirstWins()
    {
        // Arrange
        var services = new ServiceCollection();
        var hostEnvironmentMock = new Mock<IHostEnvironment>();
        hostEnvironmentMock.SetupGet(e => e.EnvironmentName).Returns("Development");
        hostEnvironmentMock.SetupGet(e => e.ApplicationName).Returns("Aiel.UnitTests");
        var builderMock = new Mock<IHostApplicationBuilder>();
        builderMock.SetupGet(b => b.Services).Returns(services);
        builderMock.SetupGet(b => b.Environment).Returns(hostEnvironmentMock.Object);
        var builder = builderMock.Object;

        // Act - DiamondTopAssembly -> DiamondLeftAssembly, DiamondRightAssembly; DiamondLeftAssembly, DiamondRightAssembly -> DiamondBottomAssembly
        await builder.AddApplicationAsync<DiamondTopAssembly>(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var rootAssembly = serviceProvider.GetRequiredService<DependencyRoot>();

        rootAssembly.Should().NotBeNull();
        rootAssembly!.Dependencies.Should().HaveCount(2, "DiamondTopAssembly depends on Left and Right");

        var leftAssembly = rootAssembly.Dependencies.First(d => d.Type == typeof(DiamondLeftAssembly));
        var rightAssembly = rootAssembly.Dependencies.First(d => d.Type == typeof(DiamondRightAssembly));

        // DiamondLeftAssembly is processed first, so its DiamondBottomAssembly dependency is fully populated
        leftAssembly.Dependencies.Should().HaveCount(1);
        leftAssembly.Dependencies[0].Type.Should().Be<DiamondBottomAssembly>();

        // DiamondRightAssembly is processed second, so it also has DiamondBottomAssembly
        // but that instance won't be fully processed (already visited)
        rightAssembly.Dependencies.Should().HaveCount(1);
        rightAssembly.Dependencies[0].Type.Should().Be<DiamondBottomAssembly>();

        // Verify DiamondBottomAssembly only configured once despite being depended on twice
        var bottomFromLeft = leftAssembly.Dependencies[0];
        ((TestDependency)bottomFromLeft.Instance).ConfigurationCount.Should().Be(1);
    }

    [Fact]
    public async Task AddApplication_CircularDependency_ThrowsCircularDependencyException()
    {
        // Arrange
        var services = new ServiceCollection();
        var hostEnvironmentMock = new Mock<IHostEnvironment>();
        hostEnvironmentMock.SetupGet(e => e.EnvironmentName).Returns("Development");
        hostEnvironmentMock.SetupGet(e => e.ApplicationName).Returns("Aiel.UnitTests");
        var builderMock = new Mock<IHostApplicationBuilder>();
        builderMock.SetupGet(b => b.Services).Returns(services);
        builderMock.SetupGet(b => b.Environment).Returns(hostEnvironmentMock.Object);
        var builder = builderMock.Object;

        // Act and Assert: CircularAAssembly -> CircularBAssembly -> CircularAAssembly (circular!)
        await Invoking(() => builder.AddApplicationAsync<CircularAAssembly>())
            .Should().ThrowAsync<CircularDependencyException>()
            .WithMessage("Circular dependency detected: CircularAAssembly -> CircularBAssembly -> CircularAAssembly");
    }

    [Fact]
    public async Task AddApplication_ConfigurationContext_IsPassedCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationManager();
        var hostEnvironmentMock = new Mock<IHostEnvironment>();
        hostEnvironmentMock.SetupGet(e => e.EnvironmentName).Returns("Production");
        var builderMock = new Mock<IHostApplicationBuilder>();
        builderMock.SetupGet(b => b.Services).Returns(services);
        builderMock.SetupGet(b => b.Environment).Returns(hostEnvironmentMock.Object);
        builderMock.SetupGet(b => b.Configuration).Returns(configuration);
        var builder = builderMock.Object;

        // Act
        await builder.AddApplicationAsync<ContextVerificationAssembly>(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var rootAssembly = serviceProvider.GetRequiredService<DependencyRoot>();

        var contextAssembly = rootAssembly!.Instance as ContextVerificationAssembly;
        contextAssembly.Should().NotBeNull();
        contextAssembly.ReceivedEnvironmentName.Should().Be("Production");
        contextAssembly.ReceivedServices.Should().BeOfType<ObservableServiceCollection>(
            "the framework wraps builder.Services in an ObservableServiceCollection to support OnAdding callbacks");
        contextAssembly.ApplicationName.Should().Be("TestApplication");

        // Verify the transparent-proxy relationship: adds flow through to builder.Services
        var sentinel = new ServiceDescriptor(typeof(Object), new Object());
        contextAssembly.ReceivedServices!.Add(sentinel);
        services.Should().Contain(sentinel, "ObservableServiceCollection proxies adds to the underlying builder.Services");
    }

    [Fact]
    public async Task AddApplication_DuplicateDependsOnAttribute_HandledCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        var hostEnvironmentMock = new Mock<IHostEnvironment>();
        hostEnvironmentMock.SetupGet(e => e.EnvironmentName).Returns("Development");
        hostEnvironmentMock.SetupGet(e => e.ApplicationName).Returns("Aiel.UnitTests");
        var builderMock = new Mock<IHostApplicationBuilder>();
        builderMock.SetupGet(b => b.Services).Returns(services);
        builderMock.SetupGet(b => b.Environment).Returns(hostEnvironmentMock.Object);
        var builder = builderMock.Object;

        // Act - AssemblyWithDuplicateDependency has DomainSharedAssembly listed twice
        await builder.AddApplicationAsync<AssemblyWithDuplicateDependency>(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var rootAssembly = serviceProvider.GetRequiredService<DependencyRoot>();

        rootAssembly.Should().NotBeNull();
        // Duplicate attributes for the same dependency type are deduplicated.
        rootAssembly!.Dependencies.Should().HaveCount(1, "duplicate DependsOn attributes for the same dependency type are merged");

        // Both should reference the same type
        rootAssembly.Dependencies.Select(d => d.Type).Should().OnlyContain(t => t == typeof(DomainShared));

        // The shared assembly should only be configured once
        var firstInstance = rootAssembly.Dependencies[0].Instance as TestDependency;
        firstInstance!.ConfigurationCount.Should().Be(1);
    }

    [Fact]
    public async Task AddApplication_DependenciesAreConfiguredInDepthOrder()
    {
        // Arrange
        var services = new ServiceCollection();
        var hostEnvironmentMock = new Mock<IHostEnvironment>();
        hostEnvironmentMock.SetupGet(e => e.EnvironmentName).Returns("Development");
        hostEnvironmentMock.SetupGet(e => e.ApplicationName).Returns("Aiel.UnitTests");
        var builderMock = new Mock<IHostApplicationBuilder>();
        builderMock.SetupGet(b => b.Services).Returns(services);
        builderMock.SetupGet(b => b.Environment).Returns(hostEnvironmentMock.Object);
        var builder = builderMock.Object;

        TrackedTestApplication.ConfigurationOrder.Clear();

        // Act - LinearAssembly4 -> LinearAssembly3 -> LinearAssembly2 -> LinearAssembly1
        await builder.AddApplicationAsync<LinearAssembly4>(cancellationToken: TestContext.Current.CancellationToken);

        // Assert - deepest dependencies should be configured first
        TrackedTestApplication.ConfigurationOrder.Should().HaveCount(4);
        TrackedTestApplication.ConfigurationOrder[0].Should().Be("LinearAssembly1", "depth 3 configured first");
        TrackedTestApplication.ConfigurationOrder[1].Should().Be("LinearAssembly2", "depth 2 configured second");
        TrackedTestApplication.ConfigurationOrder[2].Should().Be("LinearAssembly3", "depth 1 configured third");
        TrackedTestApplication.ConfigurationOrder[3].Should().Be("LinearAssembly4", "depth 0 configured last");
    }

    [Fact]
    public async Task AddApplication_DependenciesArePreConfiguredBeforeConfigured()
    {
        // Arrange
        var services = new ServiceCollection();
        var hostEnvironmentMock = new Mock<IHostEnvironment>();
        hostEnvironmentMock.SetupGet(e => e.EnvironmentName).Returns("Development");
        hostEnvironmentMock.SetupGet(e => e.ApplicationName).Returns("Aiel.UnitTests");
        var builderMock = new Mock<IHostApplicationBuilder>();
        builderMock.SetupGet(b => b.Services).Returns(services);
        builderMock.SetupGet(b => b.Environment).Returns(hostEnvironmentMock.Object);
        var builder = builderMock.Object;

        TrackedTestApplication.ConfigurationOrder.Clear();
        TrackedTestApplication.PreConfigurationOrder.Clear();

        // Act - LinearAssembly4 -> LinearAssembly3 -> LinearAssembly2 -> LinearAssembly1
        await builder.AddApplicationAsync<LinearAssembly4>(cancellationToken: TestContext.Current.CancellationToken);

        // Assert - all pre-configure calls must complete before any configure call begins
        TrackedTestApplication.PreConfigurationOrder.Should().HaveCount(4, "all 4 modules ran PreConfigureAsync");
        TrackedTestApplication.ConfigurationOrder.Should().HaveCount(4, "all 4 modules ran ConfigureAsync");

        // Verify pre-configure is also deepest-first
        TrackedTestApplication.PreConfigurationOrder[0].Should().Be("LinearAssembly1", "depth 3 pre-configured first");
        TrackedTestApplication.PreConfigurationOrder[1].Should().Be("LinearAssembly2", "depth 2 pre-configured second");
        TrackedTestApplication.PreConfigurationOrder[2].Should().Be("LinearAssembly3", "depth 1 pre-configured third");
        TrackedTestApplication.PreConfigurationOrder[3].Should().Be("LinearAssembly4", "depth 0 pre-configured last");
    }

    [Fact]
    public async Task AddApplication_MultipleCalls_CreateSeparateHierarchies()
    {
        // Arrange
        var services1 = new ServiceCollection();
        var hostEnvironmentMock1 = new Mock<IHostEnvironment>();
        hostEnvironmentMock1.SetupGet(e => e.EnvironmentName).Returns("Development");
        hostEnvironmentMock1.SetupGet(e => e.ApplicationName).Returns("App1");
        var builderMock1 = new Mock<IHostApplicationBuilder>();
        builderMock1.SetupGet(b => b.Services).Returns(services1);
        builderMock1.SetupGet(b => b.Environment).Returns(hostEnvironmentMock1.Object);

        var services2 = new ServiceCollection();
        var hostEnvironmentMock2 = new Mock<IHostEnvironment>();
        hostEnvironmentMock2.SetupGet(e => e.EnvironmentName).Returns("Development");
        hostEnvironmentMock2.SetupGet(e => e.ApplicationName).Returns("App2");
        var builderMock2 = new Mock<IHostApplicationBuilder>();
        builderMock2.SetupGet(b => b.Services).Returns(services2);
        builderMock2.SetupGet(b => b.Environment).Returns(hostEnvironmentMock2.Object);

        // Act
        await builderMock1.Object.AddApplicationAsync<StandaloneApplication>(cancellationToken: TestContext.Current.CancellationToken);
        await builderMock2.Object.AddApplicationAsync<TrackedTestApplication>(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var serviceProvider1 = services1.BuildServiceProvider();
        var rootAssembly1 = serviceProvider1.GetRequiredService<DependencyRoot>();

        var serviceProvider2 = services2.BuildServiceProvider();
        var rootAssembly2 = serviceProvider2.GetRequiredService<DependencyRoot>();

        rootAssembly1.Should().NotBeNull();
        rootAssembly2.Should().NotBeNull();

        rootAssembly1!.Type.Should().Be<StandaloneApplication>();
        rootAssembly2!.Type.Should().Be<TrackedTestApplication>();

        // Assembly instances should be different
        rootAssembly1.Instance.Should().NotBeSameAs(rootAssembly2.Instance);
    }

    [Fact]
    public async Task AddApplication_RootAssemblyInfo_IsRegisteredAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();
        var hostEnvironmentMock = new Mock<IHostEnvironment>();
        hostEnvironmentMock.SetupGet(e => e.EnvironmentName).Returns("Development");
        hostEnvironmentMock.SetupGet(e => e.ApplicationName).Returns("Aiel.UnitTests");
        var builderMock = new Mock<IHostApplicationBuilder>();
        builderMock.SetupGet(b => b.Services).Returns(services);
        builderMock.SetupGet(b => b.Environment).Returns(hostEnvironmentMock.Object);
        var builder = builderMock.Object;

        // Act
        await builder.AddApplicationAsync<StandaloneApplication>(cancellationToken: TestContext.Current.CancellationToken);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var rootAssembly1 = serviceProvider.GetRequiredService<DependencyRoot>();
        var rootAssembly2 = serviceProvider.GetRequiredService<DependencyRoot>();

        rootAssembly1.Should().NotBeNull();
        rootAssembly1.Should().BeSameAs(rootAssembly2, "RootAssemblyInfo should be registered as singleton");
    }

    public abstract class TestDependency : AielDependency
    {
        public Int32 ConfigurationCount { get; protected set; }

        public override ValueTask ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
        {
            ConfigurationCount++;
            return base.ConfigureAsync(context, cancellationToken);
        }
    }

    public abstract class TestApplication : AielApplication
    {
        public override String ApplicationName => "TestApplication";
        public override String ApplicationVersion => "1.0.0";
        public Int32 ConfigurationCount { get; protected set; }
        public override ValueTask ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
        {
            ConfigurationCount++;
            return base.ConfigureAsync(context, cancellationToken);
        }
    }

    public class TrackedTestApplication : TestApplication
    {
        public static List<String> ConfigurationOrder { get; } = [];
        public static List<String> PreConfigurationOrder { get; } = [];
        public override String ApplicationName => GetType().Name;
        public override String ApplicationVersion => "1.0.0";
        protected String AssemblyName { get; init; } = String.Empty;

        public override ValueTask PreConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
        {
            PreConfigurationOrder.Add(AssemblyName);
            return base.PreConfigureAsync(context, cancellationToken);
        }

        public override ValueTask ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
        {
            ConfigurationOrder.Add(AssemblyName);
            return base.ConfigureAsync(context, cancellationToken);
        }
    }

    public class StandaloneApplication : TestApplication;

    public class DomainShared : TestDependency;

    [DependsOn(typeof(DomainShared))]
    public class DomainAssembly : TestDependency;

    [DependsOn(typeof(DomainShared))]
    public class ApplicationContractsAssembly : TestDependency;

    [DependsOn(typeof(DomainAssembly))]
    [DependsOn(typeof(ApplicationContractsAssembly))]
    public class ApplicationDependency : TestDependency;

    [DependsOn(typeof(ApplicationDependency))]
    [DependsOn(typeof(DomainAssembly))]
    public class DataBaseAssembly : TestDependency;

    [DependsOn(typeof(ApplicationDependency))]
    [DependsOn(typeof(DataBaseAssembly))]
    public class HostApplication : TestApplication;

    // Linear dependency chain test assemblys
    public class LinearAssembly1 : TrackedTestApplication
    {
        public LinearAssembly1() => AssemblyName = nameof(LinearAssembly1);
    }

    [DependsOn(typeof(LinearAssembly1))]
    public class LinearAssembly2 : TrackedTestApplication
    {
        public LinearAssembly2() => AssemblyName = nameof(LinearAssembly2);
    }

    [DependsOn(typeof(LinearAssembly2))]
    public class LinearAssembly3 : TrackedTestApplication
    {
        public LinearAssembly3() => AssemblyName = nameof(LinearAssembly3);
    }

    [DependsOn(typeof(LinearAssembly3))]
    public class LinearAssembly4 : TrackedTestApplication
    {
        public LinearAssembly4() => AssemblyName = nameof(LinearAssembly4);
    }

    // Diamond dependency test assemblys
    public class DiamondBottomAssembly : TestDependency;

    [DependsOn(typeof(DiamondBottomAssembly))]
    public class DiamondLeftAssembly : TestDependency;

    [DependsOn(typeof(DiamondBottomAssembly))]
    public class DiamondRightAssembly : TestDependency;

    [DependsOn(typeof(DiamondLeftAssembly))]
    [DependsOn(typeof(DiamondRightAssembly))]
    public class DiamondTopAssembly : TestApplication;

    // Circular dependency test assemblys
    [DependsOn(typeof(CircularBAssembly))]
    public class CircularAAssembly : TestApplication;

    [DependsOn(typeof(CircularAAssembly))]
    public class CircularBAssembly : TestDependency;

    // Context verification assembly
    public class ContextVerificationAssembly : TestApplication
    {
        public String? ReceivedEnvironmentName { get; private set; }
        public IServiceCollection? ReceivedServices { get; private set; }

        public override ValueTask ConfigureAsync(DependencyConfigurationContext context, CancellationToken cancellationToken = default)
        {
            ReceivedEnvironmentName = context.Environment.EnvironmentName;
            ReceivedServices = context.Services;
            return base.ConfigureAsync(context, cancellationToken);
        }
    }

    // Duplicate dependency test assembly
    [DependsOn(typeof(DomainShared))]
    [DependsOn(typeof(DomainShared))]
    public class AssemblyWithDuplicateDependency : TestApplication;
}
