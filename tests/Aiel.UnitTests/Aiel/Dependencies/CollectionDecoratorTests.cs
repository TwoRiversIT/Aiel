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

namespace Aiel.Dependencies;

public sealed class CollectionDecoratorTests
{
    [Fact]
    public void Constructor_Throws_When_InnerCollection_IsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new CollectionDecorator<String>(inner: null!));
    }

    [Fact]
    public void Add_Allows_ChangingSubscriber_To_Modify_Item()
    {
        var inner = new List<String>();
        var sut = new CollectionDecorator<String>(inner);
        CollectionChangedEventArgs<String>? changedEventArgs = null;

        sut.Changing += (_, eventArgs) =>
        {
            if (eventArgs.Action == CollectionChangeAction.Add)
            {
                eventArgs.Item = eventArgs.Item.ToUpperInvariant();
            }
        };
        sut.Changed += (_, eventArgs) => changedEventArgs = eventArgs;

        sut.Add("two-rivers");

        Assert.Contains("TWO-RIVERS", inner);
        Assert.NotNull(changedEventArgs);
        Assert.Equal("TWO-RIVERS", changedEventArgs!.Item);
    }

    [Fact]
    public void Add_DoesNot_Modify_Collection_When_Canceled()
    {
        var inner = new List<String>();
        var sut = new CollectionDecorator<String>(inner);
        var changedRaised = false;

        sut.Changing += (_, eventArgs) => eventArgs.Cancel = true;
        sut.Changed += (_, _) => changedRaised = true;

        sut.Add("blocked");

        Assert.Empty(inner);
        Assert.False(changedRaised);
    }

    [Fact]
    public void Remove_Allows_ChangingSubscriber_To_Change_Target_Item()
    {
        var inner = new List<String> { "one", "two" };
        var sut = new CollectionDecorator<String>(inner);

        sut.Changing += (_, eventArgs) =>
        {
            if (eventArgs.Action == CollectionChangeAction.Remove)
            {
                eventArgs.Item = "two";
            }
        };

        var wasRemoved = sut.Remove("one");

        Assert.True(wasRemoved);
        Assert.DoesNotContain("two", inner);
        Assert.Contains("one", inner);
    }

    [Fact]
    public void Clear_DoesNot_Modify_Collection_When_Canceled()
    {
        var inner = new List<String> { "one" };
        var sut = new CollectionDecorator<String>(inner);

        sut.Changing += (_, eventArgs) =>
        {
            if (eventArgs.Action == CollectionChangeAction.Clear)
            {
                eventArgs.Cancel = true;
            }
        };

        sut.Clear();

        Assert.Single(inner);
    }
}
