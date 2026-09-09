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

namespace Aiel.Framework;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void Find_ShouldReturnAllMatching_WhenServiceRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, FirstTestService>();
        services.AddSingleton<ITestService, SecondTestService>();

        // Act
        var serviceDescriptor = services.Find<ITestService>();

        // Assert
        serviceDescriptor.Should().HaveCount(2);
        serviceDescriptor[0].ImplementationType.Should().Be<FirstTestService>();
        serviceDescriptor[1].ImplementationType.Should().Be<SecondTestService>();
    }

    [Fact]
    public void Find_ShouldReturnEmpty_WhenServiceNotRegistered()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var serviceDescriptor = services.Find<ITestService>();

        // Assert
        serviceDescriptor.Should().BeEmpty();
    }

    [Fact]
    public void Replace_MustRequire_ImplementationType_ToImplement_ServiceType()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, FirstTestService>();

        // Does not compile because TestService does not implement ITestService
        // services.Replace<ITestService, TestService>(ServiceLifetime.Singleton);
    }

    [Fact]
    public void Replace_ShouldReplaceExistingRegistrations()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<ITestService, FirstTestService>();
        services.AddSingleton<ITestService, SecondTestService>();

        // Act
        services.Replace<ITestService, ThirdTestService>(ServiceLifetime.Singleton);

        // Assert
        var provider = services.BuildServiceProvider();
        provider.GetServices<ITestService>().Should().ContainSingle();
        var service = provider.GetService<ITestService>();
        service.Should().BeOfType<ThirdTestService>();
    }

    private class FirstTestService : ITestService
    {
    }

    private class SecondTestService : ITestService
    {
    }

    private class ThirdTestService : ITestService
    {
    }

    private interface ITestService
    {
    }

    private class TestService
    {
    }
}
