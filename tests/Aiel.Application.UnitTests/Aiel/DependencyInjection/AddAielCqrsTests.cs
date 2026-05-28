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
using Aiel.Commands;
using Aiel.Domain;
using Aiel.Execution;
using Aiel.Queries;
using Aiel.Results;
using System.Reflection;

namespace Aiel.DependencyInjection;

public sealed class AddAielCqrsTests
{
    private static readonly Assembly ScanAssembly = typeof(AddAielCqrsTests).Assembly;

    // -----------------------------------------------------------------------
    // Dispatcher registration
    // -----------------------------------------------------------------------

    [Fact]
    public void AddAielCqrs_RegistersICommandDispatcher()
    {
        new ServiceCollection()
            .AddAielCqrs()
            .BuildServiceProvider()
            .GetService<ICommandDispatcher>()
            .Should().NotBeNull();
    }

    [Fact]
    public void AddAielCqrs_RegistersIQueryDispatcher()
    {
        new ServiceCollection()
            .AddAielCqrs()
            .BuildServiceProvider()
            .GetService<IQueryDispatcher>()
            .Should().NotBeNull();
    }

    [Fact]
    public void AddAielCqrs_RegistersIDomainEventDispatcher()
    {
        new ServiceCollection()
            .AddAielCqrs()
            .BuildServiceProvider()
            .GetService<IDomainEventDispatcher>()
            .Should().NotBeNull();
    }

    // -----------------------------------------------------------------------
    // Assembly scanning — handler discovery
    // -----------------------------------------------------------------------

    [Fact]
    public void AddAielCqrs_ScansAssemblyAndRegistersCommandHandler()
    {
        new ServiceCollection()
            .AddAielCqrs(ScanAssembly)
            .BuildServiceProvider()
            .GetService<ICommandHandler<ScanTestCommand>>()
            .Should().NotBeNull();
    }

    [Fact]
    public void AddAielCqrs_ScansAssemblyAndRegistersQueryHandler()
    {
        new ServiceCollection()
            .AddAielCqrs(ScanAssembly)
            .BuildServiceProvider()
            .GetService<IQueryHandler<ScanTestQuery, String>>()
            .Should().NotBeNull();
    }

    [Fact]
    public void AddAielCqrs_ScansAssemblyAndRegistersDomainEventHandler()
    {
        new ServiceCollection()
            .AddAielCqrs(ScanAssembly)
            .BuildServiceProvider()
            .GetServices<IDomainEventHandler<ScanTestDomainEvent>>()
            .Should().ContainSingle();
    }

    // -----------------------------------------------------------------------
    // Duplicate-registration safety
    // -----------------------------------------------------------------------

    [Fact]
    public void AddAielCqrs_CalledMultipleTimes_DoesNotThrow()
    {
        var act = () =>
        {
            var services = new ServiceCollection();
            services.AddAielCqrs(ScanAssembly);
            services.AddAielCqrs(ScanAssembly);
            services.BuildServiceProvider();
        };

        act.Should().NotThrow();
    }
}

// ----------------------------------------------------------------------------
// Scan-target stubs — internal at assembly scope so Assembly.GetTypes() finds them
// ----------------------------------------------------------------------------

internal record ScanTestCommand : ICommand;

internal sealed class ScanTestCommandHandler : ICommandHandler<ScanTestCommand>
{
    public Task<Result> HandleAsync(
        ScanTestCommand command, IExecutionContext context, CancellationToken ct = default)
        => Task.FromResult(Result.Success());
}

internal record ScanTestQuery : IQuery<String>;

internal sealed class ScanTestQueryHandler : IQueryHandler<ScanTestQuery, String>
{
    public Task<Result<String>> HandleAsync(
        ScanTestQuery query, IExecutionContext context, CancellationToken ct = default)
        => Task.FromResult(Result<String>.Success(String.Empty));
}

internal record ScanTestDomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
    public String EventType => nameof(ScanTestDomainEvent);
}

internal sealed class ScanTestDomainEventHandler : IDomainEventHandler<ScanTestDomainEvent>
{
    public Task HandleAsync(
        ScanTestDomainEvent domainEvent, IExecutionContext context, CancellationToken ct = default)
        => Task.CompletedTask;
}
