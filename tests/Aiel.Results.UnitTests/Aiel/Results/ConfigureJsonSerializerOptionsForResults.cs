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

namespace Aiel.Results;

// [Collection("Isolated_Tests")] serializes execution within the collection but does not
// prevent other collections from running concurrently. The implementation now creates a fresh
// mutable copy of JSO rather than mutating the shared instance, so the frozen-instance race
// is fixed at the source. The attribute is kept for extra safety.
[Collection("Isolated_Tests")]
public class ConfigureJsonSerializerOptionsForResults
{
    [Fact]
    public void ConfigureJsonSerializerOptionsForResults_WithConfigureAction_ShouldApplyCustomization()
    {
        var optionsUsed = false;
        Results.ConfigureJsonSerializerOptionsForResults(jso =>
        {
            optionsUsed = true;
            jso.Should().NotBeNull();
            jso.IsReadOnly.Should().BeFalse("the action must receive a mutable copy, not the frozen shared instance");
        });

        optionsUsed.Should().BeTrue();
    }

    [Fact]
    public void ConfigureJsonSerializerOptionsForResults_AfterJsoIsFrozen_ShouldNotThrow()
    {
        // Ensure PrivateJSO has a stored value before we freeze it.
        Results.ConfigureJsonSerializerOptionsForResults(_ => { });

        // Freeze that stored instance by using it for serialization.
        _ = System.Text.Json.JsonSerializer.Serialize(Result.Success(1), Results.JSO);
        Results.JSO.IsReadOnly.Should().BeTrue("serialization freezes the options");

        // The method must still succeed by working on a fresh copy.
        var act = () => Results.ConfigureJsonSerializerOptionsForResults(_ => { });
        act.Should().NotThrow();
    }
}
