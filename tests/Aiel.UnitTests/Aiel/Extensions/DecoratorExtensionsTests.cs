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

namespace Aiel.Extensions;

public sealed class DecoratorExtensionsTests
{
    [Fact]
    public void DecorateCollection_Throws_When_Services_Is_Null()
    {
        IServiceCollection services = null!;

        Assert.Throws<ArgumentNullException>(services.DecorateCollection<String>);
    }

    [Fact]
    public void DecorateCollection_Resolves_CollectionDecorator()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICollection<String>, List<String>>();

        services.DecorateCollection<String>();

        var provider = services.BuildServiceProvider();
        var collection = provider.GetRequiredService<ICollection<String>>();

        Assert.IsType<CollectionDecorator<String>>(collection);
    }

    [Fact]
    public void DecorateCollection_Uses_CollectionDecorator_Behavior()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ICollection<String>, List<String>>();
        services.DecorateCollection<String>();

        var provider = services.BuildServiceProvider();
        var collection = provider.GetRequiredService<ICollection<String>>();
        var decorator = Assert.IsType<CollectionDecorator<String>>(collection);

        decorator.Changing += (_, eventArgs) =>
        {
            if (eventArgs.Action == CollectionChangeAction.Add)
            {
                eventArgs.Item = eventArgs.Item.ToUpperInvariant();
            }
        };

        collection.Add("decorated");

        Assert.Contains("DECORATED", collection);
    }
}
