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

// This collection attribute ensures that tests in this class run in isolation,
// preventing side effects from shared state. In this particular case, it
// helps to ensure that any modifications to JsonSerializerOptions in other
// tests do not cause errors due to read-only issues. That said, it still seems
// to fail from time to time, so either the isolation is not perfect or there
// are other factors at play.
[Collection("Isolated_Tests")]
public class ConfigureJsonSerializerOptionsForResults
{
    [Fact]
    public void ConfigureJsonSerializerOptionsForResults_WithConfigureAction_ShouldApplyCustomization()
    {
        // Test that the method accepts and invokes the configuration action
        var optionsUsed = false;
        Results.ConfigureJsonSerializerOptionsForResults(jso =>
        {
            optionsUsed = true;
            jso.Should().NotBeNull();
            // Don't modify JSO here to avoid read-only issues
        });

        optionsUsed.Should().BeTrue();
    }
}
