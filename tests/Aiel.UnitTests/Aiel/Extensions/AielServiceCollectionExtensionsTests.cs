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

using Aiel.Dependencies;
using Microsoft.Extensions.DependencyInjection;
using static FluentAssertions.FluentActions;

namespace Aiel.Extensions;

public class AielServiceCollectionExtensionsTests
{
    [Fact]
    public void GetInstance_ReturnsNull_WhenServiceCollectionIsEmpty()
    {
        var services = new ServiceCollection();

        var result = services.GetInstance<ITestService>();

        Assert.Null(result);
    }

    [Fact]
    public void GetInstance_ReturnsNull_WhenServiceTypeNotRegistered()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IOtherService>(new OtherService());

        var result = services.GetInstance<ITestService>();

        Assert.Null(result);
    }

    [Fact]
    public void GetInstance_ReturnsInstance_WhenSingletonRegisteredWithInstance()
    {
        var services = new ServiceCollection();
        var expectedInstance = new TestService();
        services.AddSingleton<ITestService>(expectedInstance);

        var result = services.GetInstance<ITestService>();

        Assert.Same(expectedInstance, result);
    }

    [Fact]
    public void GetInstance_ReturnsNull_WhenServiceRegisteredWithFactory()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService>(_ => new TestService());

        var result = services.GetInstance<ITestService>();

        Assert.Null(result);
    }

    [Fact]
    public void GetInstance_ReturnsNull_WhenServiceRegisteredWithType()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();

        var result = services.GetInstance<ITestService>();

        Assert.Null(result);
    }

    [Fact]
    public void GetInstance_ReturnsLastInstance_WhenMultipleInstancesRegistered()
    {
        var services = new ServiceCollection();
        var firstInstance = new TestService();
        var secondInstance = new TestService();
        var thirdInstance = new TestService();
        services.AddSingleton<ITestService>(firstInstance);
        services.AddSingleton<ITestService>(secondInstance);
        services.AddSingleton<ITestService>(thirdInstance);

        var result = services.GetInstance<ITestService>();

        Assert.Same(thirdInstance, result);
    }

    [Fact]
    public void GetInstance_ReturnsLastInstance_WhenMixedRegistrationTypes()
    {
        var services = new ServiceCollection();
        var instanceOne = new TestService();
        services.AddSingleton<ITestService>(instanceOne);
        services.AddSingleton<ITestService, TestService>();
        services.AddSingleton<ITestService>(_ => new TestService());
        var instanceTwo = new TestService();
        services.AddSingleton<ITestService>(instanceTwo);

        var result = services.GetInstance<ITestService>();

        Assert.Same(instanceTwo, result);
    }

    [Fact]
    public void GetInstance_ReturnsConcreteType_WhenRegisteredAsConcreteType()
    {
        var services = new ServiceCollection();
        var instance = new TestService();
        services.AddSingleton(instance);

        var result = services.GetInstance<TestService>();

        Assert.Same(instance, result);
    }

    [Fact]
    public void GetInstance_ReturnsNull_WhenTypeRegisteredButNoInstanceAvailable()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        services.AddSingleton<IOtherService>(new OtherService());

        var result = services.GetInstance<ITestService>();

        Assert.Null(result);
    }

    [Fact]
    public void GetInstance_ReturnsValueType_WhenRegisteredWithValueTypeInstance()
    {
        var services = new ServiceCollection();
        const Int32 expectedValue = 42;
        services.AddSingleton<Object>(expectedValue);

        var result = services.GetInstance<Object>();

        Assert.Equal(expectedValue, result);
    }

    [Fact]
    public void GetInstance_ReturnsDefaultValueType_WhenValueTypeNotRegistered()
    {
        var services = new ServiceCollection();

        var result = services.GetInstance<String>();

        Assert.Null(result);
    }

    [Fact]
    public void GetInstance_ThrowNullArgumentException_WhenServicesIsNull()
    {
        ServiceCollection services = default!;
        Invoking(() => services!.GetInstance<String>()).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void OnAdding_ThrowsArgumentNullException_WhenServicesIsNull()
    {
        ServiceCollection services = default!;
        Invoking(() => services!.OnAdding(_ => { })).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void OnAdding_ThrowsArgumentNullException_WhenCallbackIsNull()
    {
        var services = new ServiceCollection();
        Invoking(() => services.OnAdding(null!)).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void OnAdding_ReturnsSameCollection_ForChaining()
    {
        var services = new ObservableServiceCollection(new ServiceCollection());

        var result = services.OnAdding(_ => { });

        Assert.Same(services, result);
    }

    [Fact]
    public void OnAdding_CallbackNotInvoked_WhenNoServicesAdded()
    {
        var services = new ObservableServiceCollection(new ServiceCollection());
        var invoked = false;

        services.OnAdding(_ => invoked = true);

        Assert.False(invoked);
    }

    [Fact]
    public void OnAdding_CallbackInvoked_WhenServiceAddedAfterRegistration()
    {
        var services = new ObservableServiceCollection(new ServiceCollection());
        var invoked = false;
        services.OnAdding(_ => invoked = true);

        services.AddSingleton<ITestService, TestService>();

        Assert.True(invoked);
    }

    [Fact]
    public void OnAdding_CallbackReceivesCorrectDescriptor_WhenServiceAdded()
    {
        var services = new ObservableServiceCollection(new ServiceCollection());
        ServiceDescriptor? captured = null;
        services.OnAdding(d => captured = d);

        services.AddSingleton<ITestService, TestService>();

        Assert.NotNull(captured);
        Assert.Equal(typeof(ITestService), captured.ServiceType);
        Assert.Equal(typeof(TestService), captured.ImplementationType);
        Assert.Equal(ServiceLifetime.Singleton, captured.Lifetime);
    }

    [Fact]
    public void OnAdding_CallbackInvokedOncePerServiceAdded()
    {
        var services = new ObservableServiceCollection(new ServiceCollection());
        var callCount = 0;
        services.OnAdding(_ => callCount++);

        services.AddSingleton<ITestService, TestService>();
        services.AddTransient<IOtherService, OtherService>();

        Assert.Equal(2, callCount);
    }

    [Fact]
    public void OnAdding_CallbackNotInvoked_ForServicesAddedBeforeRegistration()
    {
        var services = new ObservableServiceCollection(new ServiceCollection());
        services.AddSingleton<ITestService, TestService>();

        var invoked = false;
        services.OnAdding(_ => invoked = true);

        Assert.False(invoked);
    }

    [Fact]
    public void OnAdding_AllCallbacksInvoked_WhenMultipleRegistered()
    {
        var services = new ObservableServiceCollection(new ServiceCollection());
        var firstCount = 0;
        var secondCount = 0;
        services.OnAdding(_ => firstCount++);
        services.OnAdding(_ => secondCount++);

        services.AddSingleton<ITestService, TestService>();

        Assert.Equal(1, firstCount);
        Assert.Equal(1, secondCount);
    }

    private interface ITestService;

    private sealed class TestService : ITestService;

    private interface IOtherService;

    private sealed class OtherService : IOtherService;
}
