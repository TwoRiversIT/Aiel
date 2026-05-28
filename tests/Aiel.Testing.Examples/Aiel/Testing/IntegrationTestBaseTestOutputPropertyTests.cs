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

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Aiel.Testing;

/// <summary>
/// Tests for the <see cref="IntegrationTestBase{TFixture}.TestOutput"/> property with mocked dependencies.
/// </summary>
public class IntegrationTestBaseTestOutputPropertyTests(TestFixture fixture, ITestOutputHelper output)
    : IntegrationTestBase<TestFixture>(fixture, output)
{
    /// <summary>
    /// When accessing TestOutput property, it should return the ITestOutputHelper from the fixture.
    /// </summary>
    [Fact]
    public void TestOutput_ShouldReturnFixtureTestOutputHelper()
    {
        // Arrange
        var mockOutput = new Mock<ITestOutputHelper>();
        Fixture.TestOutputHelper = mockOutput.Object;

        // Act
        var result = TestOutput;

        // Assert
        Assert.Same(mockOutput.Object, result);
    }

    /// <summary>
    /// When calling WriteLine through TestOutput, it should forward the call to the underlying helper.
    /// </summary>
    [Fact]
    public void TestOutput_WriteLine_ShouldForwardToUnderlyingHelper()
    {
        // Arrange
        var mockOutput = new Mock<ITestOutputHelper>();
        Fixture.TestOutputHelper = mockOutput.Object;
        const String message = "Test message";

        // Act
        TestOutput.WriteLine(message);

        // Assert
        mockOutput.Verify(x => x.WriteLine(message), Times.Once);
    }

    /// <summary>
    /// When calling WriteLine with various formats, all calls should be forwarded correctly.
    /// </summary>
    [Theory]
    [InlineData("Simple message")]
    [InlineData("")]
    public void TestOutput_WriteLine_WithMessages_ShouldForwardCorrectly(String message)
    {
        // Arrange
        var mockOutput = new Mock<ITestOutputHelper>();
        Fixture.TestOutputHelper = mockOutput.Object;

        // Act
        TestOutput.WriteLine(message);

        // Assert
        mockOutput.Verify(x => x.WriteLine(message), Times.Once);
    }

    /// <summary>
    /// When accessing TestOutput multiple times, it should always return the same instance.
    /// </summary>
    [Fact]
    public void TestOutput_MultipleAccesses_ShouldReturnSameInstance()
    {
        // Arrange
        var mockOutput = new Mock<ITestOutputHelper>();
        Fixture.TestOutputHelper = mockOutput.Object;

        // Act
        var first = TestOutput;
        var second = TestOutput;
        var third = TestOutput;

        // Assert
        Assert.Same(first, second);
        Assert.Same(second, third);
        Assert.Same(first, mockOutput.Object);
    }

    /// <summary>
    /// When TestOutput is accessed, it should be the exact instance set on the fixture.
    /// </summary>
    [Fact]
    public void TestOutput_ShouldBeIdenticalToFixtureInstance()
    {
        // Arrange
        var mockOutput = new Mock<ITestOutputHelper>();
        Fixture.TestOutputHelper = mockOutput.Object;

        // Act
        var testOutput = TestOutput;
        var fixtureOutput = Fixture.TestOutputHelper;

        // Assert
        Assert.Same(testOutput, fixtureOutput);
    }
}

/// <summary>
/// Tests for the generic <see cref="IntegrationTestBase{TSut, TFixture}.TestOutput"/> property with mocked dependencies.
/// </summary>
public class IntegrationTestBaseGenericTestOutputPropertyTests(TestFixture fixture, ITestOutputHelper output)
    : IntegrationTestBase<DummyService, TestFixture>(fixture, output)
{
    /// <summary>
    /// When accessing TestOutput in the generic test base, it should delegate to the fixture's helper.
    /// </summary>
    [Fact]
    public void GenericTestBase_TestOutput_ShouldReturnFixtureHelper()
    {
        // Arrange
        var mockOutput = new Mock<ITestOutputHelper>();
        Fixture.TestOutputHelper = mockOutput.Object;

        // Act
        var result = TestOutput;

        // Assert
        Assert.Same(mockOutput.Object, result);
    }

    /// <summary>
    /// When calling WriteLine through TestOutput in the generic base, calls should be forwarded.
    /// </summary>
    [Fact]
    public void GenericTestBase_TestOutput_WriteLine_ShouldForwardToMockedHelper()
    {
        // Arrange
        var mockOutput = new Mock<ITestOutputHelper>();
        Fixture.TestOutputHelper = mockOutput.Object;
        const String message = "Generic test message";

        // Act
        TestOutput.WriteLine(message);

        // Assert
        mockOutput.Verify(x => x.WriteLine(message), Times.Once);
    }

    /// <summary>
    /// When TestOutput is accessed alongside SUT, both should work correctly with mocked output.
    /// </summary>
    [Fact]
    public void GenericTestBase_TestOutput_WithSUT_ShouldWorkWithMockedHelper()
    {
        // Arrange
        var mockOutput = new Mock<ITestOutputHelper>();
        Fixture.TestOutputHelper = mockOutput.Object;

        // Act
        var testOutput = TestOutput;
        var sut = SUT;
        testOutput.WriteLine("Test from SUT");

        // Assert
        Assert.NotNull(sut);
        mockOutput.Verify(x => x.WriteLine("Test from SUT"), Times.Once);
    }
}

/// <summary>
/// Dummy service for testing purposes.
/// </summary>
public class DummyService;

/// <summary>
/// Test fixture for IntegrationTestBase tests.
/// </summary>
public class TestFixture : IntegrationTestFixture
{
    /// <summary>
    /// Configures the services for the test fixture.
    /// </summary>
    protected override void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<DummyService>();
    }
}
