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

using Microsoft.Extensions.DependencyInjection;
using static AwesomeAssertions.FluentActions;

namespace Aiel.Collections;

public class AielObservableServiceCollectionTests
{
    [Fact]
    public void GetInstance_ReturnsNull_WhenServiceCollectionIsEmpty()
    {
        var services = new ObservableServiceCollection();

        var result = services.GetInstance<ITestService>();

        result.Should().BeNull();
    }

    [Fact]
    public void GetInstance_ReturnsNull_WhenServiceTypeNotRegistered()
    {
        var services = new ObservableServiceCollection();
        services.AddSingleton<IOtherService>(new OtherService());

        var result = services.GetInstance<ITestService>();

        result.Should().BeNull();
    }

    [Fact]
    public void GetInstance_ReturnsInstance_WhenSingletonRegisteredWithInstance()
    {
        var services = new ObservableServiceCollection();
        var expectedInstance = new TestService();
        services.AddSingleton<ITestService>(expectedInstance);

        var result = services.GetInstance<ITestService>();

        result.Should().BeSameAs(expectedInstance);
    }

    [Fact]
    public void GetInstance_ReturnsNull_WhenServiceRegisteredWithFactory()
    {
        var services = new ObservableServiceCollection();
        services.AddSingleton<ITestService>(_ => new TestService());

        var result = services.GetInstance<ITestService>();

        result.Should().BeNull();
    }

    [Fact]
    public void GetInstance_ReturnsNull_WhenServiceRegisteredWithType()
    {
        var services = new ObservableServiceCollection();
        services.AddSingleton<ITestService, TestService>();

        var result = services.GetInstance<ITestService>();

        result.Should().BeNull();
    }

    [Fact]
    public void GetInstance_ReturnsLastInstance_WhenMultipleInstancesRegistered()
    {
        var services = new ObservableServiceCollection();
        var firstInstance = new TestService();
        var secondInstance = new TestService();
        var thirdInstance = new TestService();
        services.AddSingleton<ITestService>(firstInstance);
        services.AddSingleton<ITestService>(secondInstance);
        services.AddSingleton<ITestService>(thirdInstance);

        var result = services.GetInstance<ITestService>();

        result.Should().BeSameAs(thirdInstance);
    }

    [Fact]
    public void GetInstance_ReturnsLastInstance_WhenMixedRegistrationTypes()
    {
        var services = new ObservableServiceCollection();
        var instanceOne = new TestService();
        services.AddSingleton<ITestService>(instanceOne);
        services.AddSingleton<ITestService, TestService>();
        services.AddSingleton<ITestService>(_ => new TestService());
        var instanceTwo = new TestService();
        services.AddSingleton<ITestService>(instanceTwo);

        var result = services.GetInstance<ITestService>();

        result.Should().BeSameAs(instanceTwo);
    }

    [Fact]
    public void GetInstance_ReturnsConcreteType_WhenRegisteredAsConcreteType()
    {
        var services = new ObservableServiceCollection();
        var instance = new TestService();
        services.AddSingleton(instance);

        var result = services.GetInstance<TestService>();

        result.Should().BeSameAs(instance);
    }

    [Fact]
    public void GetInstance_ReturnsNull_WhenTypeRegisteredButNoInstanceAvailable()
    {
        var services = new ObservableServiceCollection();
        services.AddSingleton<ITestService, TestService>();
        services.AddSingleton<IOtherService>(new OtherService());

        var result = services.GetInstance<ITestService>();

        result.Should().BeNull();
    }

    [Fact]
    public void GetInstance_ReturnsValueType_WhenRegisteredWithValueTypeInstance()
    {
        var services = new ObservableServiceCollection();
        const Int32 expectedValue = 42;
        services.AddSingleton<Object>(expectedValue);

        var result = services.GetInstance<Object>();

        result.Should().Be(expectedValue);
    }

    [Fact]
    public void GetInstance_ReturnsDefaultValueType_WhenValueTypeNotRegistered()
    {
        var services = new ObservableServiceCollection();

        var result = services.GetInstance<String>();

        result.Should().BeNull();
    }

    [Fact]
    public void GetInstance_ThrowNullArgumentException_WhenServicesIsNull()
    {
        ObservableServiceCollection services = default!;
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
        var services = new ObservableServiceCollection();
        Invoking(() => services.OnAdding(null!)).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void OnAdding_ReturnsSameCollection_ForChaining()
    {
        var services = new ObservableServiceCollection();

        var result = services.OnAdding(_ => { });

        result.Should().BeSameAs(services);
    }

    [Fact]
    public void OnAdding_CallbackNotInvoked_WhenNoServicesAdded()
    {
        var services = new ObservableServiceCollection();
        var invoked = false;

        services.OnAdding(_ => invoked = true);

        invoked.Should().BeFalse();
    }

    [Fact]
    public void OnAdding_CallbackInvoked_WhenServiceAddedAfterRegistration()
    {
        var services = new ObservableServiceCollection();
        var invoked = false;
        services.OnAdding(_ => invoked = true);

        services.AddSingleton<ITestService, TestService>();

        invoked.Should().BeTrue();
    }

    [Fact]
    public void OnAdding_CallbackReceivesCorrectDescriptor_WhenServiceAdded()
    {
        var services = new ObservableServiceCollection();
        ServiceDescriptor? captured = null;
        services.OnAdding(d => captured = d);

        services.AddSingleton<ITestService, TestService>();

        captured.Should().NotBeNull();
        captured.Should().NotBeNull();
        captured.ServiceType.Should().Be<ITestService>();
        captured.ImplementationType.Should().Be<TestService>();
        captured.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void OnAdding_CallbackInvokedOncePerServiceAdded()
    {
        var services = new ObservableServiceCollection();
        var callCount = 0;
        services.OnAdding(_ => callCount++);

        services.AddSingleton<ITestService, TestService>();
        services.AddTransient<IOtherService, OtherService>();

        callCount.Should().Be(2);
    }

    [Fact]
    public void OnAdding_CallbackNotInvoked_ForServicesAddedBeforeRegistration()
    {
        var services = new ObservableServiceCollection();
        services.AddSingleton<ITestService, TestService>();

        var invoked = false;
        services.OnAdding(_ => invoked = true);

        invoked.Should().BeFalse();
    }

    [Fact]
    public void OnAdding_AllCallbacksInvoked_WhenMultipleRegistered()
    {
        var services = new ObservableServiceCollection();
        var firstCount = 0;
        var secondCount = 0;
        services.OnAdding(_ => firstCount++);
        services.OnAdding(_ => secondCount++);

        services.AddSingleton<ITestService, TestService>();

        firstCount.Should().Be(1);
        secondCount.Should().Be(1);
    }

    private interface ITestService;

    private sealed class TestService : ITestService;

    private interface IOtherService;

    private sealed class OtherService : IOtherService;
}
