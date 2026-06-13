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

namespace Aiel.Testing;

public class IntegrationTestFixtureConfigurationTests
{
    [Fact]
    public async Task InitializeAsync_ShouldLoadWithoutTestingConfigurationFile()
    {
        using var workspace = TestWorkspace.Create();
        await using var fixture = new ConfigurableIntegrationTestFixture(workspace.Path)
        {
            TestOutputHelper = new Mock<ITestOutputHelper>().Object
        };

        await fixture.InitializeAsync();

        Assert.NotNull(fixture.Configuration);
        Assert.Null(fixture.Configuration["Sample:Value"]);
    }

    [Fact]
    public async Task InitializeAsync_ShouldApplyTestingConfigurationFileWhenPresent()
    {
        using var workspace = TestWorkspace.Create();
        await File.WriteAllTextAsync(Path.Combine(workspace.Path, "appsettings.Testing.json"), """
            {
              "Sample": {
                "Value": "Configured"
              }
            }
            """, TestContext.Current.CancellationToken);

        await using var fixture = new ConfigurableIntegrationTestFixture(workspace.Path)
        {
            TestOutputHelper = new Mock<ITestOutputHelper>().Object
        };

        await fixture.InitializeAsync();

        Assert.Equal("Configured", fixture.Configuration["Sample:Value"]);
    }

    private sealed class ConfigurableIntegrationTestFixture(String configurationBasePath) : IntegrationTestFixture
    {
        protected override String GetConfigurationBasePath()
        {
            return configurationBasePath;
        }
    }

    private sealed class TestWorkspace(String path) : IDisposable
    {
        public String Path { get; } = path;

        public static TestWorkspace Create()
        {
            var root = System.IO.Path.Combine(AppContext.BaseDirectory, "IntegrationTestFixtureConfigurationTests");
            Directory.CreateDirectory(root);

            var workspacePath = System.IO.Path.Combine(root, Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(workspacePath);
            return new TestWorkspace(workspacePath);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
