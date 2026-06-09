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

namespace Aiel.Mediator;

public sealed class DispatcherBuilderTests
{
    [Fact]
    public void AddDispatcher_returns_builder_that_registers_singleton_sender_and_publisher()
    {
        var services = new ServiceCollection();

        var builder = services.AddDispatcher(typeof(DispatcherBuilderTests).Assembly);
        var returnedServices = builder.Build();

        returnedServices.Should().BeSameAs(services);

        using var provider = services.BuildServiceProvider(validateScopes: true);
        var sender = provider.GetRequiredService<ISender>();
        var publisher = provider.GetRequiredService<IPublisher>();

        sender.Should().BeSameAs(provider.GetRequiredService<ISender>());
        publisher.Should().BeSameAs(provider.GetRequiredService<IPublisher>());
        sender.Should().BeSameAs((Object)publisher);
    }

    [Fact]
    public void AddDispatcher_registers_scanned_handlers_as_scoped_services()
    {
        var services = new ServiceCollection();
        services.AddDispatcher(typeof(DispatcherBuilderTests).Assembly).Build();

        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var firstScope = provider.CreateScope();
        using var secondScope = provider.CreateScope();

        var firstCommandHandler = firstScope.ServiceProvider.GetRequiredService<IActionHandler<ScopedResolutionCommand>>();
        var secondCommandHandler = secondScope.ServiceProvider.GetRequiredService<IActionHandler<ScopedResolutionCommand>>();
        var firstQueryHandler = firstScope.ServiceProvider.GetRequiredService<IActionHandler<ScopedResolutionQuery>>();
        var secondQueryHandler = secondScope.ServiceProvider.GetRequiredService<IActionHandler<ScopedResolutionQuery>>();
        var firstNotificationHandler = firstScope.ServiceProvider.GetRequiredService<INotificationHandler<ScopedResolutionNotification>>();
        var secondNotificationHandler = secondScope.ServiceProvider.GetRequiredService<INotificationHandler<ScopedResolutionNotification>>();

        firstCommandHandler.Should().NotBeSameAs(secondCommandHandler);
        firstQueryHandler.Should().NotBeSameAs(secondQueryHandler);
        firstNotificationHandler.Should().NotBeSameAs(secondNotificationHandler);
    }

    [Fact]
    public void WithBehavior_when_behavior_type_is_null_throws()
    {
        var services = new ServiceCollection();
        var builder = services.AddDispatcher(typeof(DispatcherBuilderTests).Assembly);

        Action act = () => builder.WithBehavior(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("openGenericBehaviorType");
    }

    [Fact]
    public void WithBehavior_when_type_is_not_open_generic_throws()
    {
        var services = new ServiceCollection();
        var builder = services.AddDispatcher(typeof(DispatcherBuilderTests).Assembly);

        Action act = () => builder.WithBehavior(typeof(String));

        act.Should().Throw<ArgumentException>()
            .WithParameterName("openGenericBehaviorType")
            .WithMessage("*open generic type definition*");
    }

    [Fact]
    public void WithBehavior_when_type_does_not_implement_IPipelineBehavior_throws()
    {
        var services = new ServiceCollection();
        var builder = services.AddDispatcher(typeof(DispatcherBuilderTests).Assembly);

        Action act = () => builder.WithBehavior(typeof(NotABehavior<>));

        act.Should().Throw<ArgumentException>()
            .WithParameterName("openGenericBehaviorType")
            .WithMessage("*IPipelineBehavior*");
    }
}
